using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Xaml.Input;
using Windows.UI.Input.Inking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.Devices.Input;
using Windows.UI;
using GeoAPI.Geometries;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NetTopologySuite.Geometries;
using NusysIntermediate;
using PathGeometry = SharpDX.Direct2D1.PathGeometry;
using Point = Windows.Foundation.Point;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NuSysApp
{
    /// <summary>
    /// This is the view for the entire workspace. It instantiates the WorkspaceViewModel. 
    /// </summary>
    public sealed partial class FreeFormViewer
    {
        private List<PointModel> _latestStroke;
        private CanvasInteractionManager _canvasInteractionManager;
        private CollectionInteractionManager _collectionInteractionManager;
        private FreeFormViewerViewModel _vm;

        private Dictionary<ElementViewModel, Transformable> _transformables =
            new Dictionary<ElementViewModel, Transformable>();

        public CollectionRenderItem CurrentCollection { get; private set; }

        public CollectionRenderItem InitialCollection { get; private set; }

        public ObservableCollection<ElementRenderItem> Selections { get; set; } =
            new ObservableCollection<ElementRenderItem>();


        public CanvasAnimatedControl RenderCanvas => xRenderCanvas;
        public VideoMediaPlayer VideoPlayer => xVideoPlayer;
        public AudioMediaPlayer AudioPlayer => xAudioPlayer;

        public VideoElementRenderItem ActiveVideoRenderItem;
        public AudioElementRenderItem ActiveAudioRenderItem;

        private MinimapRenderItem _minimap;

        private Matrix3x2 _transform = Matrix3x2.Identity;
        public bool ToolsAreBeingInteractedWith { get; set; }
        private bool _inkPressed;
        private bool _renderCanvasInitialized;
        private bool _minimapInitialized;


        public FreeFormViewer()
        {
            this.InitializeComponent();

            SizeChanged += OnSizeChanged;

            xMinimapCanvas.Width = 300;
            xMinimapCanvas.Height = 300;
            xMinimapCanvas.CreateResources+=
                delegate(CanvasControl sender, CanvasCreateResourcesEventArgs args)
                {
                    _minimapInitialized = true;
                    TryInitialize();
                };

            xRenderCanvas.CreateResources += async delegate
            {
                _renderCanvasInitialized = true;
                TryInitialize();
            };
        }

        public void ActivateUndo(IUndoable action, Point2d location)
        {
            xUndoButton.MoveTo(location);
            xUndoButton.Activate(action);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Canvas.SetLeft(xMinimapCanvas, sizeChangedEventArgs.NewSize.Width-xMinimapCanvas.Width);
            Canvas.SetTop(xMinimapCanvas, sizeChangedEventArgs.NewSize.Height- xMinimapCanvas.Height);
        }

        public void LoadInitialCollection(FreeFormViewerViewModel vm)
        {

            if (_vm != null)
            {
                vm.Controller.Disposed -= ControllerOnDisposed;
                vm.Elements.CollectionChanged -= ElementsOnCollectionChanged;
            } 
            InitialCollection?.Dispose();
            _canvasInteractionManager?.Dispose();
            vm.Controller.Disposed += ControllerOnDisposed;
            vm.Elements.CollectionChanged += ElementsOnCollectionChanged;
            _vm = vm;
            DataContext = _vm;

            TryInitialize();
           
        }

        private void ElementsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            _minimap?.Invalidate();
        }

        private async void TryInitialize()
        {
            if (!(_renderCanvasInitialized && _minimapInitialized))
                return;

            NuSysRenderer.Instance.Stop();
            InitialCollection = new ShapedCollectionRenderItem(_vm, null, xRenderCanvas, true);
            await NuSysRenderer.Instance.Init(xRenderCanvas, InitialCollection);
            _vm.X = 0;
            _vm.Y = 0;
            _vm.Width = xRenderCanvas.Width;
            _vm.Height = xRenderCanvas.Height;

            if (_canvasInteractionManager == null)
                _canvasInteractionManager = new CanvasInteractionManager(SessionController.Instance.SessionView.MainCanvas);

            SwitchCollection(InitialCollection);

            /*
            if (_vm.Controller.LibraryElementModel.AccessType == NusysConstants.AccessType.ReadOnly)
            {
                if (_vm.Controller.LibraryElementModel.Creator != SessionController.Instance.LocalUserID)
                {
                    SwitchMode(Options.PanZoomOnly);
                }

            }
            else
            {
                SwitchMode(Options.SelectNode);
            }

             */


            _minimap = new MinimapRenderItem(InitialCollection, null, xMinimapCanvas);
        }


        private void SwitchCollection(CollectionRenderItem collection)
        {
            if (collection != CurrentCollection && collection != null)
            {
                if (CurrentCollection != null)
                {
                    _collectionInteractionManager.ItemSelected -= CollectionInteractionManagerOnItemTapped;
                    _collectionInteractionManager.DoubleTapped -= OnItemDoubleTapped;
                    _collectionInteractionManager.SelectionsCleared -= CollectionInteractionManagerOnSelectionsCleared;
                    _collectionInteractionManager.Panned -= CollectionInteractionManagerOnPanned;
                    _collectionInteractionManager.PanZoomed -= CollectionInteractionManagerOnPanZoomed;
                    _collectionInteractionManager.SelectionPanZoomed -= CollectionInteractionManagerOnSelectionPanZoomed;
                    _collectionInteractionManager.ItemMoved -= CollectionInteractionManagerOnItemMoved;
                    _collectionInteractionManager.DuplicateCreated -= CollectionInteractionManagerOnDuplicateCreated;
                    _collectionInteractionManager.CollectionSwitched -= CollectionInteractionManagerOnCollectionSwitched;
                    _collectionInteractionManager.InkStarted -= CollectionInteractionManagerOnInkStarted;
                    _collectionInteractionManager.InkDrawing -= CollectionInteractionManagerOnInkDrawing;
                    _collectionInteractionManager.InkStopped -= CollectionInteractionManagerOnInkStopped;
                    _collectionInteractionManager.ResizerDragged -= CollectionInteractionManagerOnResizerDragged;
                    _collectionInteractionManager.SelectionInkPressed -=
                        CollectionInteractionManagerOnSelectionInkPressed;
                    _collectionInteractionManager.ResizerStarted -= CollectionInteractionManagerOnResizerStarted;
                    _collectionInteractionManager.ResizerStopped -= CollectionInteractionManagerOnResizerStopped;
                    _collectionInteractionManager.LinkCreated -= CollectionInteractionManagerOnLinkCreated;
                    _collectionInteractionManager.TrailCreated -= CollectionInteractionManagerOnTrailCreated;
                    _collectionInteractionManager.ElementAddedToCollection -=
                        CollectionInteractionManagerOnElementAddedToCollection;
                    _collectionInteractionManager.MultimediaElementActivated -=
                        CollectionInteractionManagerOnMultimediaElementActivated;
                    _canvasInteractionManager.PointerPressed -= CanvasInteractionManagerOnPointerPressed;
                    _canvasInteractionManager.AllPointersReleased -= CanvasInteractionManagerOnAllPointersReleased;
                    multiMenu.CreateCollection -= MultiMenuOnCreateCollection;
                    _canvasInteractionManager.ItemTapped -= CanvasInteractionManagerOnItemTapped;

                    _collectionInteractionManager.Dispose();
                }

                CurrentCollection = collection;
                _collectionInteractionManager = new CollectionInteractionManager(_canvasInteractionManager, collection);
                _collectionInteractionManager.ItemSelected += CollectionInteractionManagerOnItemTapped;
                _collectionInteractionManager.DoubleTapped += OnItemDoubleTapped;
                _collectionInteractionManager.SelectionsCleared += CollectionInteractionManagerOnSelectionsCleared;
                if (!collection.ViewModel.IsFinite || collection == InitialCollection)
                {
                    _collectionInteractionManager.Panned += CollectionInteractionManagerOnPanned;
                    _collectionInteractionManager.PanZoomed += CollectionInteractionManagerOnPanZoomed;
                }

                _collectionInteractionManager.SelectionPanZoomed += CollectionInteractionManagerOnSelectionPanZoomed;
                _collectionInteractionManager.ItemMoved += CollectionInteractionManagerOnItemMoved;
                _collectionInteractionManager.DuplicateCreated += CollectionInteractionManagerOnDuplicateCreated;
                _collectionInteractionManager.CollectionSwitched += CollectionInteractionManagerOnCollectionSwitched;
                _collectionInteractionManager.InkStarted += CollectionInteractionManagerOnInkStarted;
                _collectionInteractionManager.InkDrawing += CollectionInteractionManagerOnInkDrawing;
                _collectionInteractionManager.InkStopped += CollectionInteractionManagerOnInkStopped;
                _collectionInteractionManager.ResizerDragged += CollectionInteractionManagerOnResizerDragged;
                _collectionInteractionManager.SelectionInkPressed += CollectionInteractionManagerOnSelectionInkPressed;
                _collectionInteractionManager.ResizerStarted += CollectionInteractionManagerOnResizerStarted;
                _collectionInteractionManager.ResizerStopped += CollectionInteractionManagerOnResizerStopped;
                _collectionInteractionManager.LinkCreated += CollectionInteractionManagerOnLinkCreated;
                _collectionInteractionManager.TrailCreated += CollectionInteractionManagerOnTrailCreated;
                _collectionInteractionManager.ElementAddedToCollection +=
                    CollectionInteractionManagerOnElementAddedToCollection;
                _collectionInteractionManager.MultimediaElementActivated +=
                    CollectionInteractionManagerOnMultimediaElementActivated;
                _canvasInteractionManager.PointerPressed += CanvasInteractionManagerOnPointerPressed;
                _canvasInteractionManager.AllPointersReleased += CanvasInteractionManagerOnAllPointersReleased;
                multiMenu.CreateCollection += MultiMenuOnCreateCollection;
                _canvasInteractionManager.ItemTapped += CanvasInteractionManagerOnItemTapped;

                _minimap?.SwitchCollection(collection);
            }
        }

        private void CollectionInteractionManagerOnSelectionPanZoomed(Vector2 center, Vector2 deltaTranslation,
            float deltaZoom)
        {
            var transform = NuSysRenderer.Instance.GetCollectionTransform(InitialCollection);
       
            foreach (var selection in Selections)
            {
                var elem = selection.ViewModel;
                var imgCenter = new Vector2((float) (elem.X + elem.Width/2), (float) (elem.Y + elem.Height/2));
                var newCenter = Vector2.Transform(imgCenter, transform);

                Transformable t;
                if (_transformables.ContainsKey(elem))
                    t = _transformables[elem];
                else
                {
                    t = new Transformable();
                    t.Position = new Point(elem.X, elem.Y);
                    t.Size = new Size(elem.Width, elem.Height);
                    if (elem is ElementCollectionViewModel)
                    {
                        var elemc = elem as ElementCollectionViewModel;
                        t.CameraTranslation = elemc.CameraTranslation;
                        t.CameraCenter = elemc.CameraCenter;
                        t.CameraScale = elemc.CameraScale;
                        _transformables.Add(elem, t);
                    }
                }

                PanZoom2(t, _transform, newCenter, deltaTranslation.X, deltaTranslation.Y, deltaZoom);

                elem.Controller.SetSize(t.Size.Width*t.S.M11, t.Size.Height*t.S.M22);
                var dtx = (float) (t.Size.Width*t.S.M11 - t.Size.Width)/2f;
                var dty = (float) (t.Size.Height*t.S.M22 - t.Size.Height)/2f;
                var nx = t.Position.X - dtx;
                var ny = t.Position.Y - dty;
                elem.Controller.SetPosition(nx, ny);

                if (elem is ElementCollectionViewModel)
                {
                    var elemc = elem as ElementCollectionViewModel;
                    if (!elemc.IsFinite || CurrentCollection.ViewModel == elemc)
                    {
                        var ct = Matrix3x2.CreateTranslation(t.CameraTranslation);
                        var cc = Matrix3x2.CreateTranslation(t.CameraCenter);
                        var cs = Matrix3x2.CreateScale(t.CameraScale);

                        var et = Matrix3x2.CreateTranslation(new Vector2((float) elem.X, (float) elem.Y));

                        var tran = Win2dUtil.Invert(cc)*cs*cc*ct*et;
                        var tranInv = Win2dUtil.Invert(tran);

                        var controller = elemc.Controller as ElementCollectionController;
                        controller.SetCameraPosition(ct.M31 + dtx*tranInv.M11, ct.M32 + dty*tranInv.M22);
                        controller.SetCameraCenter(cc.M31 - dtx*tranInv.M11, cc.M32 - dty*tranInv.M22);
                    }
                }
            }

            _minimap.Invalidate();
        }

        private async void CollectionInteractionManagerOnTrailCreated(ElementRenderItem element1,
            ElementRenderItem element2)
        {
            // create a new instance of CreateNewPresentationLinkRequestArgs
            var createNewPresentationLinkRequestArgs = new CreateNewPresentationLinkRequestArgs();
            // pass in the id of the current element view model as the ElementViewModelInId.
            createNewPresentationLinkRequestArgs.ElementViewModelOutId = element1.ViewModel.Id;
            // pass in the parent collection id of the element model as the parent collection id
            createNewPresentationLinkRequestArgs.ParentCollectionId = element1.ViewModel.Model.ParentCollectionId;

            // if an element exists at the current point
            createNewPresentationLinkRequestArgs.ElementViewModelInId = element2.ViewModel.Id;
            var request = new CreateNewPresentationLinkRequest(createNewPresentationLinkRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddPresentationLinkToLibrary();
        }


        private async void CollectionInteractionManagerOnLinkCreated(ElementRenderItem element1,
            ElementRenderItem element2)
        {
            // Diable linking to links and tools
            // TODO: Enable linking to links 
            if (element1.ViewModel.ElementType == NusysConstants.ElementType.Link ||
                element2.ViewModel.ElementType == NusysConstants.ElementType.Tools)
            {
                return;
            }
            var createNewLinkLibraryElementRequestArgs = new CreateNewLinkLibraryElementRequestArgs();
            createNewLinkLibraryElementRequestArgs.LibraryElementModelInId = element1.ViewModel.LibraryElementId;
            createNewLinkLibraryElementRequestArgs.LibraryElementType = NusysConstants.ElementType.Link;
            createNewLinkLibraryElementRequestArgs.LibraryElementModelOutId = element2.ViewModel.LibraryElementId;
            createNewLinkLibraryElementRequestArgs.Title =
                $"Link from {element1.ViewModel.Model.Title} to {element2.ViewModel.Model.Title}";
            if (createNewLinkLibraryElementRequestArgs.LibraryElementModelInId !=
                createNewLinkLibraryElementRequestArgs.LibraryElementModelOutId &&
                SessionController.Instance.LinksController.GetLinkLibraryElementControllerBetweenContent(
                    createNewLinkLibraryElementRequestArgs.LibraryElementModelInId,
                    createNewLinkLibraryElementRequestArgs.LibraryElementModelOutId) == null)
            {
                var contentRequestArgs = new CreateNewContentRequestArgs();
                contentRequestArgs.LibraryElementArgs = createNewLinkLibraryElementRequestArgs;
                var request = new CreateNewContentRequest(contentRequestArgs);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                request.AddReturnedLibraryElementToLibrary();
            }
        }

        private void CollectionInteractionManagerOnResizerStopped()
        {
            xMultimediaCanvas.IsHitTestVisible = true;
        }

        private void CollectionInteractionManagerOnResizerStarted()
        {
            xMultimediaCanvas.IsHitTestVisible = false;
        }

        private void CollectionInteractionManagerOnMultimediaElementActivated(ElementRenderItem element)
        {
            if (element is VideoElementRenderItem)
            {
                ActiveVideoRenderItem = (VideoElementRenderItem) element;
                var t = ActiveVideoRenderItem.GetTransform()*
                        NuSysRenderer.Instance.GetTransformUntil(ActiveVideoRenderItem);
                var ct =
                    (CompositeTransform)
                        SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;
                SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.AudioWrapper.Controller =
                    element.ViewModel.Controller.LibraryElementController;

                SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.Source =
                    new Uri(ActiveVideoRenderItem.ViewModel.Controller.LibraryElementController.Data);

                SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.SetVideoSize(element.ViewModel.Width, element.ViewModel.Height);

                SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.Visibility = Visibility.Visible;
                return;
            }
            if (element is AudioElementRenderItem)
            {
                ActiveAudioRenderItem = (AudioElementRenderItem) element;
                var t = ActiveAudioRenderItem.GetTransform()*
                        NuSysRenderer.Instance.GetTransformUntil(ActiveAudioRenderItem);
                var ct =
                    (CompositeTransform)
                        SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;
               
                SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.AudioWrapper.Controller = element.ViewModel.Controller.LibraryElementController;
                SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.AudioSource = new Uri(ActiveAudioRenderItem.ViewModel.Controller.LibraryElementController.Data);
                SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.SetAudioSize(element.ViewModel.Width, element.ViewModel.Height);

                SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.Visibility = Visibility.Visible;
                return;
            }
        }

        private void CollectionInteractionManagerOnResizerDragged(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            foreach (var item in Selections)
            {
                var elem = item;
                var collection = item.Parent;
                var nw = elem.ViewModel.Width + delta.X/(_transform.M11*collection.S.M11*collection.Camera.S.M11);
                var nh = elem.ViewModel.Height + delta.Y/(_transform.M22*collection.S.M22*collection.Camera.S.M22);
                item.ViewModel.Controller.SetSize(nw, nh);
            }

            _minimap.Invalidate();
        }

        private async void MultiMenuOnCreateCollection(bool finite, bool shaped)
        {
            List<PointModel> shapePoints = null;
            Rect strokeBoundingBox;
            double offsetX = 0;
            double offsetY = 0;

            Rect targetScreenRect;
            var collectionTransform = NuSysRenderer.Instance.GetCollectionTransform(CurrentCollection);
            if (shaped && _latestStroke != null)
            {
                strokeBoundingBox = Geometry.PointCollecionToBoundingRect(_latestStroke);
                offsetX = strokeBoundingBox.X - 50000;
                offsetY = strokeBoundingBox.Y - 50000;
                foreach (var p in _latestStroke)
                {
                    p.X -= offsetX;
                    p.Y -= offsetY;
                }

                shapePoints = _latestStroke;
                targetScreenRect = Win2dUtil.TransformRect(strokeBoundingBox, collectionTransform);
            }
            else
            {
                targetScreenRect = NuSysRenderer.Instance.ElementSelectionRenderItem._screenRect;

            }

            var targetPointTl =
                NuSysRenderer.Instance.ScreenPointerToCollectionPoint(
                    new Vector2((float) targetScreenRect.X, (float) targetScreenRect.Y), CurrentCollection);
            var targetPointBr =
                NuSysRenderer.Instance.ScreenPointerToCollectionPoint(
                    new Vector2((float) (targetScreenRect.X + targetScreenRect.Width),
                        (float) (targetScreenRect.Y + targetScreenRect.Height)), CurrentCollection);

            if (shaped && _latestStroke != null)
            {
                shapePoints = _latestStroke;
            }
            else if ((shaped && _latestStroke == null) || finite)
            {
                var w = targetPointBr.X - targetPointTl.X;
                var h = targetPointBr.Y - targetPointTl.Y;
                shapePoints = new List<PointModel>
                {
                    new PointModel(50000, 50000),
                    new PointModel(50000 + w, 50000),
                    new PointModel(50000 + w, 50000 + h),
                    new PointModel(50000, 50000 + h),
                    new PointModel(50000, 50000)
                };
            }


            var createNewContentRequestArgs = new CreateNewContentRequestArgs
            {
                LibraryElementArgs = new CreateNewCollectionLibraryElementRequestArgs()
                {
                    AccessType =
                        SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType,
                    LibraryElementType = NusysConstants.ElementType.Collection,
                    Title = "Unnamed Collection",
                    LibraryElementId = SessionController.Instance.GenerateId(),
                    IsFiniteCollection = finite,
                    ShapePoints = shapePoints

                },
                ContentId = SessionController.Instance.GenerateId()
            };

            // execute the content request
            var contentRequest = new CreateNewContentRequest(createNewContentRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(contentRequest);
            contentRequest.AddReturnedLibraryElementToLibrary();


            // create a new add element to collection request
            var newElementRequestArgs = new NewElementRequestArgs
            {
                LibraryElementId = createNewContentRequestArgs.LibraryElementArgs.LibraryElementId,
                ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                Height = targetPointBr.Y - targetPointTl.Y,
                Width = targetPointBr.X - targetPointTl.X,
                X = targetPointTl.X,
                Y = targetPointTl.Y
            };

            // execute the add element to collection request
            var elementRequest = new NewElementRequest(newElementRequestArgs);
            await
                SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(
                    createNewContentRequestArgs.ContentId);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

            await elementRequest.AddReturnedElementToSessionAsync();

            offsetX = targetPointTl.X - 50000;
            offsetY = targetPointTl.Y - 50000;
            foreach (var element in Selections)
            {
                var target = new Vector2((float) (element.ViewModel.X - offsetX),
                    (float) (element.ViewModel.Y - offsetY));
                await
                    element.ViewModel.Controller.RequestMoveToCollection(
                        createNewContentRequestArgs.LibraryElementArgs.LibraryElementId, target.X, target.Y,
                        element.ViewModel.Width*collectionTransform.M11,
                        element.ViewModel.Height*collectionTransform.M11);
            }

            CurrentCollection.InkRenderItem.RemoveLatestStroke();
            ClearSelections();
        }

        private void CanvasInteractionManagerOnItemTapped(CanvasPointer pointer)
        {
            var item = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint);
            if (Selections.Count == 0)
                return;
            if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnDelete)
            {
                foreach (var elementRenderItem in Selections)
                {
                    if (elementRenderItem is AudioElementRenderItem || elementRenderItem is VideoElementRenderItem)
                        xMultimediaCanvas.Visibility = Visibility.Collapsed;

                    var removeElementAction = new DeleteElementAction(elementRenderItem.ViewModel.Controller);

                    //Creates an undo button and places it in the correct position.
                    var screenCenter = elementRenderItem.GetCenterOnScreen();
                    ActivateUndo(removeElementAction, new Point2d(screenCenter.X - xUndoButton.ActualWidth/2, screenCenter.Y - xUndoButton.ActualHeight / 2));

                    elementRenderItem.ViewModel.Controller?.RequestDelete();
                }
                ClearSelections();
                
            }
            if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnGroup)
            {
                multiMenu.Show(pointer.CurrentPoint.X + 50, pointer.CurrentPoint.Y, _latestStroke != null);
            }
            if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnPresent)
            {
                SessionController.Instance.SessionView.EnterPresentationMode(Selections[0].ViewModel);
                ClearSelections();
            }

            if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnPdfLeft)
            {
                var selection = (PdfElementRenderItem) Selections[0];
                selection.GotoPage(selection.CurrentPage - 1);
            }
            if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnPdfRight)
            {
                var selection = (PdfElementRenderItem)Selections[0];
                selection.GotoPage(selection.CurrentPage + 1);
            }
        }

        private async void CollectionInteractionManagerOnSelectionInkPressed(CanvasPointer pointer,
            IEnumerable<Vector2> ink)
        {
            _inkPressed = true;
            ClearSelections();
            var multipoint = new MultiPoint(ink.Select(p => new NetTopologySuite.Geometries.Point(p.X, p.Y)).ToArray());
            var ch = multipoint.ConvexHull();
            _latestStroke = ch.Coordinates.Select(p => new PointModel(p.X, p.Y)).ToList();

            foreach (var renderItem in CurrentCollection.GetRenderItems().OfType<ElementRenderItem>())
            {
                if (renderItem is PseudoElementRenderItem)
                    continue;
                var vm = renderItem.ViewModel;
                var anchor = new NetTopologySuite.Geometries.Point(vm.Anchor.X, vm.Anchor.Y);
                if (ch.Contains(anchor))
                {
                    AddToSelections(renderItem);
                }
            }

            await Task.Delay(500);
            _inkPressed = false;
        }

        private void CanvasInteractionManagerOnAllPointersReleased()
        {
            var until = NuSysRenderer.Instance.GetTransformUntil(CurrentCollection);
            _transform = Win2dUtil.Invert(CurrentCollection.C)*CurrentCollection.S*CurrentCollection.C*
                         CurrentCollection.T*until;

            _transformables.Clear();
        }

        private async void CollectionInteractionManagerOnElementAddedToCollection(ElementRenderItem element,
            CollectionRenderItem collection, CanvasPointer pointer)
        {
            if (element is VideoElementRenderItem)
            {
                xVideoPlayer.Visibility = Visibility.Collapsed;
            }
            if (element is AudioElementRenderItem)
            {
                xVideoPlayer.Visibility = Visibility.Collapsed;
            }

            var targetPoint = NuSysRenderer.Instance.ScreenPointerToCollectionPoint(pointer.CurrentPoint, collection);
            var target = new Vector2(targetPoint.X - (float) element.ViewModel.Width/2f, targetPoint.Y - (float) element.ViewModel.Height/2f);
            var elementId = element.ViewModel.Id;
            var parentCollectionId = element.ViewModel.Controller.GetParentCollectionId();
            await element.ViewModel.Controller.RequestMoveToCollection(collection.ViewModel.Model.LibraryId, target.X, target.Y);

            var oldLocationScreen = new Point2d(pointer.StartPoint.X, pointer.StartPoint.Y);
            var oldLocationCollectionV = NuSysRenderer.Instance.ScreenPointerToCollectionPoint(pointer.StartPoint, collection);
            var oldLocationCollection = new Point2d(oldLocationCollectionV.X, oldLocationCollectionV.Y);
            var newLocation = new Point2d(target.X, target.Y);
            var action = new MoveToCollectionAction(elementId, parentCollectionId, collection.ViewModel.Model.LibraryId, oldLocationCollection, newLocation);
            
            ActivateUndo(action, oldLocationScreen);


            _minimap.Invalidate();
        }

        private void CollectionInteractionManagerOnInkStopped(CanvasPointer pointer)
        {
            CurrentCollection.InkRenderItem.StopInkByEvent(pointer);

        }

        private void CollectionInteractionManagerOnInkDrawing(CanvasPointer pointer)
        {
            CurrentCollection.InkRenderItem.UpdateInkByEvent(pointer);
        }

        private void CollectionInteractionManagerOnInkStarted(CanvasPointer pointer)
        {
            CurrentCollection.InkRenderItem.StartInkByEvent(pointer);
        }

        private void CollectionInteractionManagerOnCollectionSwitched(CollectionRenderItem collection)
        {
            ClearSelections();
            SwitchCollection(collection);
        }

        private void CollectionInteractionManagerOnDuplicateCreated(ElementRenderItem element, Vector2 point)
        {
            var targetPoint = Vector2.Transform(point,
                Win2dUtil.Invert(NuSysRenderer.Instance.GetTransformUntil(element)));
            element.ViewModel.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y);
        }

        private void CollectionInteractionManagerOnItemMoved(CanvasPointer pointer, ElementRenderItem elem,
            Vector2 delta)
        {
            if (elem is PseudoElementRenderItem)
                return;
            var collection = elem.Parent;

            var newX = elem.ViewModel.X + delta.X/(_transform.M11*collection.Camera.S.M11);
            var newY = elem.ViewModel.Y + delta.Y/(_transform.M22*collection.Camera.S.M22);

            if (!Selections.Contains(elem))
            {
                elem.ViewModel.Controller.SetPosition(newX, newY);
            }
            else
            {
                foreach (var selectable in Selections)
                {
                    var e = selectable.ViewModel;
                    var newXe = e.X + delta.X/(_transform.M11*collection.S.M11*collection.Camera.S.M11);
                    var newYe = e.Y + delta.Y/(_transform.M11*collection.S.M11*collection.Camera.S.M11);
                    e.Controller.SetPosition(newXe, newYe);
                }
            }

            _minimap.Invalidate();
            UpdateMediaPlayer();
        }

        private void CanvasInteractionManagerOnPointerPressed(CanvasPointer pointer)
        {
            var until = NuSysRenderer.Instance.GetTransformUntil(CurrentCollection);
            _transform = Win2dUtil.Invert(CurrentCollection.C)*CurrentCollection.S*CurrentCollection.C*
                         CurrentCollection.T*until;
        }

        private void CollectionInteractionManagerOnPanZoomed(Vector2 center, Vector2 deltaTranslation, float deltaZoom)
        {
            PanZoom2(CurrentCollection.Camera, _transform, center, deltaTranslation.X/_transform.M11,
                deltaTranslation.Y/_transform.M11, deltaZoom);

            CurrentCollection.InkRenderItem.UpdateDryInkTransform();

            UpdateNonWin2dElements();
            _minimap.Invalidate();
        }

        private void CollectionInteractionManagerOnPanned(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            if (ToolsAreBeingInteractedWith)
                return;
            PanZoom2(CurrentCollection.Camera, _transform, point, delta.X/_transform.M11, delta.Y/_transform.M11, 1);
            CurrentCollection.InkRenderItem.UpdateDryInkTransform();
            UpdateNonWin2dElements();
            _minimap.Invalidate();
        }

        private void CollectionInteractionManagerOnSelectionsCleared()
        {
            if (!_inkPressed)
                ClearSelections();

            _minimap.Invalidate();
        }

        private async void OnDuplicateCreated(ElementRenderItem element, Vector2 point)
        {
            var targetPoint = Vector2.Transform(point,
                Win2dUtil.Invert(NuSysRenderer.Instance.GetTransformUntil(element)));
            element.ViewModel.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y);
        }


        private void OnItemDoubleTapped(ElementRenderItem element)
        {
            if (element == CurrentCollection || element == InitialCollection)
                return;

            var libraryElementModelId = element.ViewModel.Controller.LibraryElementModel.LibraryElementId;
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
            SessionController.Instance.SessionView.ShowDetailView(controller);
        }

        private void CollectionInteractionManagerOnItemTapped(ElementRenderItem element)
        {
            AddToSelections(element);
        }

        public void AddToSelections(ElementRenderItem element)
        {
            element.ViewModel.IsSelected = true;
            Selections.Add(element);
            _minimap.Invalidate();
        }

        private void ClearSelections()
        {
            // SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.Visibility = Visibility.Collapsed;
            foreach (var selection in Selections)
            {
                if (selection.ViewModel != null)
                    selection.ViewModel.IsSelected = false;
            }
            Selections.Clear();
            _latestStroke = null;
        }

        private void UpdateNonWin2dElements()
        {
            UpdateMediaPlayer();
        }

        private void UpdateMediaPlayer()
        {
            if (ActiveVideoRenderItem != null)
            {
                var t = ActiveVideoRenderItem.GetTransform() * NuSysRenderer.Instance.GetTransformUntil(ActiveVideoRenderItem);
                var ct = (CompositeTransform)SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;
            }

            if (ActiveAudioRenderItem != null)
            {
                var t = ActiveAudioRenderItem.GetTransform() * NuSysRenderer.Instance.GetTransformUntil(ActiveAudioRenderItem);
                var ct = (CompositeTransform)SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;
            }

            var vm = (FreeFormViewerViewModel)InitialCollection.ViewModel;
            vm.CompositeTransform.TranslateX = InitialCollection.Camera.T.M31;
            vm.CompositeTransform.TranslateY = InitialCollection.Camera.T.M32;
            vm.CompositeTransform.CenterX = InitialCollection.Camera.C.M31;
            vm.CompositeTransform.CenterY = InitialCollection.Camera.C.M32;
            vm.CompositeTransform.ScaleX = InitialCollection.Camera.S.M11;
            vm.CompositeTransform.ScaleY = InitialCollection.Camera.S.M22;
        }




        protected void PanZoom2(I2dTransformable target, Matrix3x2 transform, Vector2 centerPoint, float dx, float dy, float ds)
        {
            var cInv = Win2dUtil.Invert(target.C);
            var inverse = Win2dUtil.Invert(cInv * target.S * target.C * target.T * transform);

            var center = Vector2.Transform(new Vector2(centerPoint.X, centerPoint.Y), inverse);
            var tmpTranslate = Matrix3x2.CreateTranslation(target.C.M31, target.C.M32);
            Matrix3x2 tmpTranslateInv;
            Matrix3x2.Invert(tmpTranslate, out tmpTranslateInv);

            var localPoint = Vector2.Transform(center, tmpTranslateInv);

            //Now scale the point in local space
            localPoint.X *= target.S.M11;
            localPoint.Y *= target.S.M22;

            //Transform local space into world space again
            var worldPoint = Vector2.Transform(localPoint, tmpTranslate);

            //Take the actual scaling...
            var distance = new Vector2(worldPoint.X - center.X, worldPoint.Y - center.Y);

            //...and balance the jump of the changed scaling origin by changing the translation            

            var ntx = target.T.M31 + distance.X + dx;
            var nty = target.T.M32 + distance.Y + dy;

            var nsx = target.S.M11 * ds;
            var nsy = target.S.M22 * ds;

            var ncx = center.X;
            var ncy = center.Y;

            target.T = Matrix3x2.CreateTranslation((float)ntx, (float)nty);
            target.C = Matrix3x2.CreateTranslation(ncx, ncy);
            target.S = Matrix3x2.CreateScale((float)nsx, (float)nsy);
            target.Update();
        }


        public void Freeze()
        {
            _canvasInteractionManager.SetEnabled(false);
        }

        public void Unfreeze()
        {
            _canvasInteractionManager.SetEnabled(true);
        }



        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (FreeFormViewerViewModel) DataContext;
            if (vm != null)
            {
                if (vm.Controller != null)
                {
                    vm.Controller.Disposed -= ControllerOnDisposed;
                }
            }

        }

        public void Dispose()
        {
           
        }

        public Canvas AtomCanvas
        {
            get { return xAtomCanvas; }
        }

        public MultiSelectMenuView MultiMenu
        {
            get { return multiMenu; }
        }

        public Canvas Wrapper
        {
            get { return xWrapper; }
        }

        public ItemsControl IControl
        {
            get { return IC; }
        }

        public NuSysInqCanvas InqCanvas
        {
            get { return null; }
        }

        public async Task SetViewMode(AbstractWorkspaceViewMode mode, bool isFixed = false)
        {
            return;
        }




        public PanZoomMode PanZoom
        {
            get { return null; }
        }


        public SelectMode SelectMode { get { return null; } }

        public FrameworkElement GetAdornment()
        {
            var items = _vm.AtomViewList.Where(element => element is AdornmentView);
            var adornment = items.FirstOrDefault();
            return adornment;
        }
    }
}