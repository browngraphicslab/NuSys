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
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.UI;
using GeoAPI.Geometries;
using Microsoft.Graphics.Canvas.Geometry;
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
        private CanvasInteractionManager _canvasInteractionManager;
        private CollectionInteractionManager _collectionInteractionManager;
        private FreeFormViewerViewModel _vm;

        private Dictionary<ElementViewModel, Transformable> _transformables = new Dictionary<ElementViewModel, Transformable>();

        public CollectionRenderItem CurrentCollection { get; private set; }

        public CollectionRenderItem InitialCollection { get; private set; }
        public ObservableCollection<ElementRenderItem> Selections { get; set; } = new ObservableCollection<ElementRenderItem>();


        public CanvasAnimatedControl RenderCanvas => xRenderCanvas;
        public VideoMediaPlayer VideoPlayer => xVideoPlayer;
        public AudioMediaPlayer AudioPlayer => xAudioPlayer;

        private Matrix3x2 _transform = Matrix3x2.Identity;
        private bool _inkPressed;

        public FreeFormViewer(FreeFormViewerViewModel vm)
        {
            this.InitializeComponent();
            
            vm.Controller.Disposed += ControllerOnDisposed;
            _vm = vm;

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                InitialCollection = new CollectionRenderItem(vm, null, xRenderCanvas, true);
                await NuSysRenderer.Instance.Init(xRenderCanvas, InitialCollection);

                vm.X = 0;
                vm.Y = 0;
                vm.Width = xRenderCanvas.Width;
                vm.Height = xRenderCanvas.Height;
                
                _canvasInteractionManager = new CanvasInteractionManager(xRenderCanvas);

                SwitchCollection(InitialCollection);

                if (vm.Controller.LibraryElementModel.AccessType == NusysConstants.AccessType.ReadOnly)
                {
                    if (vm.Controller.LibraryElementModel.Creator != SessionController.Instance.LocalUserID)
                    {
                        SwitchMode(Options.PanZoomOnly);
                    }
                   
                }
                else
                {
                    SwitchMode(Options.SelectNode);
                }
                
                var colElementModel = vm.Controller.Model as CollectionElementModel;
                if ((SessionController.Instance.ContentController.GetLibraryElementModel(colElementModel.LibraryId)as CollectionLibraryElementModel).IsFinite)
                {
                    LimitManipulation();
                }
            };
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
                    _collectionInteractionManager.ItemMoved -= CollectionInteractionManagerOnItemMoved;
                    _collectionInteractionManager.DuplicateCreated -= CollectionInteractionManagerOnDuplicateCreated;
                    _collectionInteractionManager.CollectionSwitched -= CollectionInteractionManagerOnCollectionSwitched;
                    _collectionInteractionManager.SelectionInkPressed -= CollectionInteractionManagerOnSelectionInkPressed;
                    _collectionInteractionManager.ElementAddedToCollection -= CollectionInteractionManagerOnElementAddedToCollection;
                    _collectionInteractionManager.InkStarted -= CollectionInteractionManagerOnInkStarted;
                    _collectionInteractionManager.InkDrawing -= CollectionInteractionManagerOnInkDrawing;
                    _collectionInteractionManager.InkStopped -= CollectionInteractionManagerOnInkStopped;
                    _collectionInteractionManager.ResizerDragged -= CollectionInteractionManagerOnResizerDragged;
                    _canvasInteractionManager.PointerPressed -= CanvasInteractionManagerOnPointerPressed;
                    _canvasInteractionManager.AllPointersReleased -= CanvasInteractionManagerOnAllPointersReleased;
                    _canvasInteractionManager.ItemTapped -= CanvasInteractionManagerOnItemTapped;
                    multiMenu.CreateCollection -= MultiMenuOnCreateCollection;
                    _collectionInteractionManager.Dispose();
                }

                CurrentCollection = collection;
                _collectionInteractionManager = new CollectionInteractionManager(_canvasInteractionManager, collection);
                _collectionInteractionManager.ItemSelected += CollectionInteractionManagerOnItemTapped;
                _collectionInteractionManager.DoubleTapped += OnItemDoubleTapped;
                _collectionInteractionManager.SelectionsCleared += CollectionInteractionManagerOnSelectionsCleared;
                _collectionInteractionManager.Panned += CollectionInteractionManagerOnPanned;
                _collectionInteractionManager.PanZoomed += CollectionInteractionManagerOnPanZoomed;
                _collectionInteractionManager.ItemMoved += CollectionInteractionManagerOnItemMoved;
                _collectionInteractionManager.DuplicateCreated += CollectionInteractionManagerOnDuplicateCreated;
                _collectionInteractionManager.CollectionSwitched += CollectionInteractionManagerOnCollectionSwitched;
                _collectionInteractionManager.InkStarted += CollectionInteractionManagerOnInkStarted;
                _collectionInteractionManager.InkDrawing += CollectionInteractionManagerOnInkDrawing;
                _collectionInteractionManager.InkStopped += CollectionInteractionManagerOnInkStopped;
                _collectionInteractionManager.ResizerDragged += CollectionInteractionManagerOnResizerDragged;
                _collectionInteractionManager.SelectionInkPressed += CollectionInteractionManagerOnSelectionInkPressed;
                _collectionInteractionManager.ElementAddedToCollection += CollectionInteractionManagerOnElementAddedToCollection;
                _canvasInteractionManager.PointerPressed += CanvasInteractionManagerOnPointerPressed;
                _canvasInteractionManager.AllPointersReleased += CanvasInteractionManagerOnAllPointersReleased;
                multiMenu.CreateCollection += MultiMenuOnCreateCollection;
                _canvasInteractionManager.ItemTapped += CanvasInteractionManagerOnItemTapped;
            }
        }

        private void CollectionInteractionManagerOnResizerDragged(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            foreach (var item in Selections)
            {
                var elem = item;
                var collection = item.Parent;
                var nw = elem.ViewModel.Width + delta.X / (_transform.M11 * collection.S.M11 * collection.Camera.S.M11);
                var nh = elem.ViewModel.Height + delta.Y / (_transform.M22 * collection.S.M22 * collection.Camera.S.M22);
                item.ViewModel.Controller.SetSize(nw, nh);
            }
        }

        private async void MultiMenuOnCreateCollection(bool finite, bool shaped)
        {
            var transform = NuSysRenderer.Instance.GetTransformUntil(Selections.First());
            var shapeStroke = CurrentCollection.InkRenderItem.LatestStroke;
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
                    ShapePoints = shapeStroke != null ? CurrentCollection.InkRenderItem.LatestStroke.GetInkPoints().Select(p => new PointModel(p.Position.X, p.Position.Y)).ToList() : null

                   },
                ContentId = SessionController.Instance.GenerateId()
            };

            // execute the content request
            var contentRequest = new CreateNewContentRequest(createNewContentRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(contentRequest);
            contentRequest.AddReturnedLibraryElementToLibrary();

            var targetScreenRect = NuSysRenderer.Instance.ElementSelectionRenderItem._screenRect;
            var targetPointTl = NuSysRenderer.Instance.ScreenPointerToCollectionPoint(new Vector2((float)targetScreenRect.X, (float)targetScreenRect.Y), CurrentCollection);
            var targetPointBr = NuSysRenderer.Instance.ScreenPointerToCollectionPoint(new Vector2((float)(targetScreenRect.X + targetScreenRect.Width), (float)(targetScreenRect.Y + targetScreenRect.Height)), CurrentCollection);

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
            await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(createNewContentRequestArgs.ContentId);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

            await elementRequest.AddReturnedElementToSessionAsync();


            foreach (var element in Selections)
            {
                var target = new Vector2(50000 - (float)element.ViewModel.Width / 2f, 50000 - (float)element.ViewModel.Height / 2f);
                await element.ViewModel.Controller.RequestMoveToCollection(createNewContentRequestArgs.LibraryElementArgs.LibraryElementId, target.X, target.Y);
            }

        }

        private void CanvasInteractionManagerOnItemTapped(CanvasPointer pointer)
        {
            var item = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint);
            if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnDelete)
            {
                foreach (var elementRenderItem in Selections)
                {
                    elementRenderItem.ViewModel.Controller?.RequestDelete();
                }
            }
            if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnGroup)
            {
                multiMenu.Show(pointer.CurrentPoint.X + 50, pointer.CurrentPoint.Y);
            }
        }

        private async void CollectionInteractionManagerOnSelectionInkPressed(CanvasPointer pointer, IEnumerable<Vector2> ink)
        {
            _inkPressed = true;
            ClearSelections();
            var multipoint = new MultiPoint(ink.Select(p => new NetTopologySuite.Geometries.Point(p.X, p.Y)).ToArray());
            var ch = multipoint.ConvexHull();
            foreach (var renderItem in CurrentCollection.GetRenderItems().OfType<ElementRenderItem>())
            {
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
            _transformables.Clear();
        }

        private async void CollectionInteractionManagerOnElementAddedToCollection(ElementRenderItem element, CollectionRenderItem collection, CanvasPointer pointer)
        {
            var targetPoint = NuSysRenderer.Instance.ScreenPointerToCollectionPoint(pointer.CurrentPoint, collection);
            var target = new Vector2(targetPoint.X - (float)element.ViewModel.Width/2f, targetPoint.Y - (float)element.ViewModel.Height/2f);
            await element.ViewModel.Controller.RequestMoveToCollection(collection.ViewModel.Model.LibraryId, target.X, target.Y);
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
            SwitchCollection(collection);
        }

        private void CollectionInteractionManagerOnDuplicateCreated(ElementRenderItem element, Vector2 point)
        {
            var targetPoint = Vector2.Transform(point, Win2dUtil.Invert(NuSysRenderer.Instance.GetTransformUntil(element)));
            element.ViewModel.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y);
        }

        private void CollectionInteractionManagerOnItemMoved(CanvasPointer pointer, ElementRenderItem element, Vector2 delta)
        {
            var elem = element;
            var collection = element.Parent;

            var newX = elem.ViewModel.X + delta.X / (_transform.M11 * collection.S.M11 * collection.Camera.S.M11);
            var newY = elem.ViewModel.Y + delta.Y / (_transform.M22 * collection.S.M22 * collection.Camera.S.M22);

            if (!Selections.Contains(elem))
            {
                elem.ViewModel.Controller.SetPosition(newX, newY);
            }
            else
            {
                foreach (var selectable in Selections)
                {
                    var e = selectable.ViewModel;
                    var newXe = e.X + delta.X / (_transform.M11 * collection.S.M11 * collection.Camera.S.M11);
                    var newYe = e.Y + delta.Y / (_transform.M11 * collection.S.M11 * collection.Camera.S.M11);
                    e.Controller.SetPosition(newXe, newYe);
                }
            }
        }

        private void CanvasInteractionManagerOnPointerPressed(CanvasPointer pointer)
        {
            var until = NuSysRenderer.Instance.GetTransformUntil(CurrentCollection);
            _transform = Win2dUtil.Invert(CurrentCollection.C) * CurrentCollection.S * CurrentCollection.C * CurrentCollection.T * until;

            var item = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint);

            return;

                if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnDelete)
            {
                foreach (var elementRenderItem in Selections)
                {
                    elementRenderItem.ViewModel.Controller.RequestDelete();
                }
            }
            if (item == NuSysRenderer.Instance.ElementSelectionRenderItem.BtnPresent)
            {
                Debug.WriteLine("BTUN PRESEENT");
            }
        }

        private void CollectionInteractionManagerOnPanZoomed(Vector2 center, Vector2 deltaTranslation, float deltaZoom)
        {
            if (Selections.Count > 0)
            {
                foreach (var selection in Selections)
                {
                    var elem = (ElementViewModel)selection.ViewModel;
                    var imgCenter = new Vector2((float)(elem.X + elem.Width / 2), (float)(elem.Y + elem.Height / 2));
                    var newCenter = InitialCollection.ObjectPointToScreenPoint(imgCenter);

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

                    elem.Controller.SetSize(t.Size.Width * t.S.M11, t.Size.Height * t.S.M22);
                    var dtx = (float)(t.Size.Width * t.S.M11 - t.Size.Width) / 2f;
                    var dty = (float)(t.Size.Height * t.S.M22 - t.Size.Height) / 2f;
                    var nx = t.Position.X - dtx;
                    var ny = t.Position.Y - dty;
                    elem.Controller.SetPosition(nx, ny);

                    if (elem is ElementCollectionViewModel)
                    {
                        var elemc = elem as ElementCollectionViewModel;
                        var ct = Matrix3x2.CreateTranslation(t.CameraTranslation);
                        var cc = Matrix3x2.CreateTranslation(t.CameraCenter);
                        var cs = Matrix3x2.CreateScale(t.CameraScale);

                        var et = Matrix3x2.CreateTranslation(new Vector2((float)elem.X, (float)elem.Y));

                        var tran = Win2dUtil.Invert(cc) * cs * cc * ct * et;
                        var tranInv = Win2dUtil.Invert(tran);

                        var controller = elemc.Controller as ElementCollectionController;
                        controller.SetCameraPosition(ct.M31 + dtx * tranInv.M11, ct.M32 + dty * tranInv.M22);
                        controller.SetCameraCenter(cc.M31 - dtx * tranInv.M11, cc.M32 - dty * tranInv.M22);
                    }
                }
            }
            else
            {
                PanZoom2(CurrentCollection.Camera, _transform, center, deltaTranslation.X / _transform.M11, deltaTranslation.Y / _transform.M11, deltaZoom);
            }
        }

        private void CollectionInteractionManagerOnPanned(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            PanZoom2(CurrentCollection.Camera, _transform, point, delta.X/_transform.M11, delta.Y/_transform.M11, 1);
        }

        private void CollectionInteractionManagerOnSelectionsCleared()
        {
            if (!_inkPressed)
                ClearSelections();
        }

        private async void OnDuplicateCreated(ElementRenderItem element, Vector2 point)
        {
            var targetPoint = Vector2.Transform(point, Win2dUtil.Invert(NuSysRenderer.Instance.GetTransformUntil(element)));
            element.ViewModel.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y);
        }


        private void OnItemDoubleTapped(ElementRenderItem element)
        {
            var libraryElementModelId = element.ViewModel.Controller.LibraryElementModel.LibraryElementId;
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
            SessionController.Instance.SessionView.ShowDetailView(controller);
        }

        private void OnItemLongTapped(BaseRenderItem element, PointerRoutedEventArgs args)
        {
            return;
            /*
            if (element is VideoElementRenderItem)
            {
                ActiveVideoRenderItem = (VideoElementRenderItem)element;
                var t = ActiveVideoRenderItem.GetTransform() * NuSysRenderer.Instance.GetTransformUntil(NuSysRenderer.Instance.ActiveVideoRenderItem);
                var ct = (CompositeTransform)SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;
                SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.Source = new Uri(ActiveVideoRenderItem.ViewModel.Controller.LibraryElementController.Data);
                SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.Visibility = Visibility.Visible;
                return;
            }
            if (element is AudioElementRenderItem)
            {
                ActiveAudioRenderItem = (AudioElementRenderItem)element;
                var t = ActiveAudioRenderItem.GetTransform() * NuSysRenderer.Instance.GetTransformUntil(NuSysRenderer.Instance.ActiveAudioRenderItem);
                var ct = (CompositeTransform)SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;
                SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.AudioSource = new Uri(ActiveAudioRenderItem.ViewModel.Controller.LibraryElementController.Data); ;
                SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.Visibility = Visibility.Visible;
                return;
            }




            //     var collection = element as CollectionRenderItem;
            //     SwitchCollection(collection);
            */
        }

        private void CollectionInteractionManagerOnItemTapped(ElementRenderItem element)
        {
            AddToSelections(element);
        }

        public void AddToSelections(ElementRenderItem element)
        {
            element.ViewModel.IsSelected = true;
            Selections.Add(element);
            var elementSelectionRenderItem = NuSysRenderer.Instance.ElementSelectionRenderItem;
            //multiMenu.Show(elementSelectionRenderItem._screenRect.X, elementSelectionRenderItem._screenRect.Y);
        }

        private void ClearSelections()
        {
            // SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.Visibility = Visibility.Collapsed;
            foreach (var selection in Selections)
            {
                selection.ViewModel.IsSelected = false;
            }
            Selections.Clear();
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

        private async void SwitchMode(Options mode)
        {
            return;
            
        }


        public PanZoomMode PanZoom
        {
            get { return null; }
        }


        public SelectMode SelectMode { get { return null; } }

        public void ChangeMode(object source, Options mode)
        {
            SwitchMode(mode);
        }

        public void LimitManipulation()
        {

        }

        public FrameworkElement GetAdornment()
        {
            var items = _vm.AtomViewList.Where(element => element is AdornmentView);
            var adornment = items.FirstOrDefault();
            return adornment;
        }
    }
}