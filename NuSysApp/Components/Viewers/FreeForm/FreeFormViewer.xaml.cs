using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NetTopologySuite.Geometries;
using NusysIntermediate;
using NuSysApp.Components.NuSysRenderer.UI;
using WinRTXamlToolkit.IO.Extensions;
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
        private static float ARRANGE_BORDER = 55.0f;
        private List<PointModel> _latestStroke;
        private RenderItemInteractionManager _canvasInteractionManager;

        /// <summary>
        /// getter for canvasinteractionmanager
        /// </summary>
        public RenderItemInteractionManager CanvasInteractionManager
        {
            get { return _canvasInteractionManager; }
        }

        private CollectionInteractionManager _collectionInteractionManager;

        private FreeFormViewerViewModel _vm;

        private Dictionary<ElementViewModel, RenderItemTransform> _transformables =
            new Dictionary<ElementViewModel, RenderItemTransform>();

        public CollectionRenderItem CurrentCollection { get; private set; }

        public CollectionRenderItem InitialCollection { get; private set; }

        public ObservableCollection<ElementRenderItem> Selections { get; set; } =
            new ObservableCollection<ElementRenderItem>();


        public CanvasAnimatedControl RenderCanvas => xRenderCanvas;
        public BaseMediaPlayer VideoPlayer => xVideoPlayer;
        public AudioPlayer AudioPlayer => xAudioPlayer;

        public VideoElementRenderItem ActiveVideoRenderItem;
        public AudioElementRenderItem ActiveAudioRenderItem;

        public MinimapRenderItem _minimap;

        private Matrix3x2 _transform = Matrix3x2.Identity;
        public bool ToolsAreBeingInteractedWith { get; set; }

        private ElementController _currentAudioElementController;
        private ElementController _currentVideoElementController;
        private BaseRenderItem _pressedRenderItem;
        private BaseRenderItem _selectedLink;

        private bool _inkPressed;

        private SessionRootRenderItem _renderRoot;
        public NuSysRenderer RenderEngine { get; private set; }

        /// <summary>
        /// get the private transform of the free form viewer
        /// </summary>
        public Matrix3x2 Transform
        {
            get { return _transform; }
        }
        // Manages the focus of the render items, instantiated in constructor
        public FocusManager FocusManager { get; private set; }

        private LayoutWindowUIElement _layoutWindow;
        private EditTagsUIElement _editTagsElement;
        private bool _customLayoutDrawing = false;

        public event EventHandler<bool> CanvasPanned;

        public FreeFormViewer()
        {
            this.InitializeComponent();

            SizeChanged += OnSizeChanged;
            xMinimapCanvas.IsHitTestVisible = false;
            xMinimapCanvas.Width = 300;
            xMinimapCanvas.Height = 300;

            xFullScreenImageViewer.ImageClosed += XFullScreenImageViewerOnImageClosed;

            _renderRoot = new SessionRootRenderItem(null, xRenderCanvas);
            RenderEngine = new NuSysRenderer(xRenderCanvas, _renderRoot);
            xFullScreenImageViewer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Event fired when the full screen image viewer is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void XFullScreenImageViewerOnImageClosed(object sender, EventArgs eventArgs)
        {
            xFullScreenImageViewer.IsHitTestVisible = false;
            xWrapper.IsHitTestVisible = true;
        }

        public void Clear()
        {
           // vm.Controller.Disposed -= ControllerOnDisposed;
          //  vm.Elements.CollectionChanged -= ElementsOnCollectionChanged;
            InitialCollection?.Dispose();
            xRenderCanvas.Invalidate();
            _minimap?.Dispose();

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

        public async Task LoadInitialCollection(FreeFormViewerViewModel vm)
        {
            _vm = vm;
            _vm.X = 0;
            _vm.Y = 0;
            _vm.Width = xRenderCanvas.Width;
            _vm.Height = xRenderCanvas.Height;
            DataContext = _vm;

            // NOTE IF YOU ARE CREATING OBJECTS IN THIS METHOD. BE AWARE THAT THE METHOD IS CALLED 
            // MULTIPLE TIMES!!! WE DO NOT WANT TO END UP WITH MULTIPLE INSTANCES OF OBJECTS
            // THAT SHOULD ONLY EXIST ONCE.
            // THIS CAN EASILY BE ACCOMPLISHED WITH A NULL CHECK AS SHOWN DIRECTLY BELOW THIS LINE!!!

            // Make sure the _canvasInteractionManager is only implemented once
            if (_canvasInteractionManager == null)
            {
                _canvasInteractionManager = new RenderItemInteractionManager(RenderEngine, xWrapper);
            }

            // Make sure the FocusManager is only implemented once
            if (FocusManager == null)
            {
                FocusManager = new FocusManager(_canvasInteractionManager, RenderEngine);
            }

            if (_vm != null)
            {
                vm.Controller.Disposed -= ControllerOnDisposed;
                vm.Elements.CollectionChanged -= ElementsOnCollectionChanged;
            } 

            vm.Controller.Disposed += ControllerOnDisposed;
            vm.Elements.CollectionChanged += ElementsOnCollectionChanged;

         
            InitialCollection = new CollectionRenderItem(_vm, null, xRenderCanvas, true);
            SwitchCollection(InitialCollection);

            RenderEngine.Root.ClearChildren();

            InitialCollection.Transform.SetParent(RenderEngine.Root.Transform);

            RenderEngine.Root.AddChild(InitialCollection);

            RenderEngine.Start();

            RenderEngine.BtnDelete.Tapped -= BtnDeleteOnTapped;
            RenderEngine.BtnDelete.Tapped += BtnDeleteOnTapped;

            RenderEngine.BtnExportTrail.Tapped -= BtnExportTrailOnTapped;
            RenderEngine.BtnExportTrail.Tapped += BtnExportTrailOnTapped;

            _minimap = new MinimapRenderItem(InitialCollection, null, xMinimapCanvas);
        }

        /// <summary>
        /// exports trail to HTML when export button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void BtnExportTrailOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_selectedLink is TrailRenderItem)
            {
                //get trail as a list of nodes
                List<LibraryElementController> trailList = GetTrailAsList((_selectedLink as TrailRenderItem).ViewModel.Model);

                for (int i = 0; i < trailList.Count; i++)
                {
                    var currElement = trailList[i];
                    string prev = null;
                    string next = null;
                    if (i > 0)
                    {
                        prev = trailList[i - 1].Title;
                    }
                    if (i < trailList.Count - 1)
                    {
                        next = trailList[i + 1].Title;
                    }

                    await currElement.ExportToHTML(prev, next);
                }
                
                StorageFolder htmlFolder = await NuSysStorages.NuSysTempFolder.GetFolderAsync("HTML");
                var firstPage = await htmlFolder.GetFileAsync(trailList[0].Title + ".html");

                var exportPopup = new CenteredPopup(RenderEngine.Root, xRenderCanvas,
                    "You have exported your trail! \n \n" +
                    "Find it in your Documents/NuSys/HTML.");
                RenderEngine.Root.AddChild(exportPopup);

                //open the exported html in browser
                await Windows.System.Launcher.LaunchFileAsync(firstPage);
            }
        }

        /// <summary>
        /// gets trail elements as a list
        /// </summary>
        /// <param name="trail"></param>
        /// <returns></returns>
        private List<LibraryElementController> GetTrailAsList(PresentationLinkModel trail)
        {
            List<LibraryElementController> elements = new List<LibraryElementController>();
            var currTrail = trail;
            while (currTrail != null) 
            {
                var inNode = SessionController.Instance.ElementModelIdToElementController[currTrail.InElementId].LibraryElementController;
                var outNode =
                    SessionController.Instance.ElementModelIdToElementController[currTrail.OutElementId].LibraryElementController;
                if (!elements.Contains(inNode))
                {
                    elements.Add(inNode);
                }
                if (elements.Contains(outNode))
                {
                    break;
                }
                elements.Add(outNode);

                var oldTrail = currTrail;
                var models = PresentationLinkViewModel.Models;

                currTrail = models.FirstOrDefault(vm => vm.InElementId == oldTrail.OutElementId);
            }

            return elements;
        }

        private void ElementsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            _minimap?.Invalidate();
        }

        private void SwitchCollection(CollectionRenderItem collection)
        {

            if (CurrentCollection != null)
            {
                _collectionInteractionManager.ItemSelected -= CollectionInteractionManagerOnItemTapped;
                _collectionInteractionManager.LinkSelected -= CollectionInteractionManagerOnLinkSelected;
                _collectionInteractionManager.TrailSelected -= CollectionInteractionManagerOnTrailSelected;
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
                _collectionInteractionManager.SelectionInkPressed -= CollectionInteractionManagerOnSelectionInkPressed;
                _collectionInteractionManager.ResizerStarted -= CollectionInteractionManagerOnResizerStarted;
                _collectionInteractionManager.ResizerStopped -= CollectionInteractionManagerOnResizerStopped;
                _collectionInteractionManager.LinkCreated -= CollectionInteractionManagerOnLinkCreated;
                _collectionInteractionManager.TrailCreated -= CollectionInteractionManagerOnTrailCreated;
                _collectionInteractionManager.ElementAddedToCollection -= CollectionInteractionManagerOnElementAddedToCollection;
                _collectionInteractionManager.MultimediaElementActivated -= CollectionInteractionManagerOnMultimediaElementActivated;
                _canvasInteractionManager.PointerPressed -= CanvasInteractionManagerOnPointerPressed;
                _canvasInteractionManager.AllPointersReleased -= CanvasInteractionManagerOnAllPointersReleased;
                multiMenu.CreateCollection -= MultiMenuOnCreateCollection;
                _canvasInteractionManager.ItemTapped -= CanvasInteractionManagerOnItemTapped;

                _collectionInteractionManager.Dispose();
                //Remove focus from FocusManager
                FocusManager.ClearFocus();
            }

            CurrentCollection = collection;
            _collectionInteractionManager = new CollectionInteractionManager(_canvasInteractionManager, collection);

            if (!collection.ViewModel.IsFinite || collection == InitialCollection)
            {
                _collectionInteractionManager.Panned += CollectionInteractionManagerOnPanned;
                _collectionInteractionManager.PanZoomed += CollectionInteractionManagerOnPanZoomed;
            }


            if (!SessionController.IsReadonly) { 
                _collectionInteractionManager.DoubleTapped += OnItemDoubleTapped;
                _collectionInteractionManager.SelectionPanZoomed += CollectionInteractionManagerOnSelectionPanZoomed;
                _collectionInteractionManager.ItemMoved += CollectionInteractionManagerOnItemMoved;
                _collectionInteractionManager.LinkSelected += CollectionInteractionManagerOnLinkSelected;
                _collectionInteractionManager.TrailSelected += CollectionInteractionManagerOnTrailSelected;
                _collectionInteractionManager.DuplicateCreated += CollectionInteractionManagerOnDuplicateCreated;
                _collectionInteractionManager.InkStarted += CollectionInteractionManagerOnInkStarted;
                _collectionInteractionManager.InkDrawing += CollectionInteractionManagerOnInkDrawing;
                _collectionInteractionManager.InkStopped += CollectionInteractionManagerOnInkStopped;
                _collectionInteractionManager.ResizerDragged += CollectionInteractionManagerOnResizerDragged;
                _collectionInteractionManager.SelectionInkPressed += CollectionInteractionManagerOnSelectionInkPressed;
                _collectionInteractionManager.ResizerStarted += CollectionInteractionManagerOnResizerStarted;
                _collectionInteractionManager.ResizerStopped += CollectionInteractionManagerOnResizerStopped;
                _collectionInteractionManager.LinkCreated += CollectionInteractionManagerOnLinkCreated;
                _collectionInteractionManager.TrailCreated += CollectionInteractionManagerOnTrailCreated;
                _collectionInteractionManager.ElementAddedToCollection += CollectionInteractionManagerOnElementAddedToCollection;
                multiMenu.CreateCollection += MultiMenuOnCreateCollection;
                //Toggle FocusManager read only variable
                FocusManager.InReadOnly = false;
            } else
            {
                //Toggle FocusManager read only variable
                FocusManager.InReadOnly = true;
            }

            _collectionInteractionManager.RenderItemPressed += OnRenderItemPressed;
            _collectionInteractionManager.CollectionSwitched += CollectionInteractionManagerOnCollectionSwitched;
            _collectionInteractionManager.ItemSelected += CollectionInteractionManagerOnItemTapped;
            _collectionInteractionManager.SelectionsCleared += CollectionInteractionManagerOnSelectionsCleared;
            _collectionInteractionManager.MultimediaElementActivated += CollectionInteractionManagerOnMultimediaElementActivated;
            _canvasInteractionManager.ItemTapped += CanvasInteractionManagerOnItemTapped;
            _canvasInteractionManager.PointerPressed += CanvasInteractionManagerOnPointerPressed;
            _canvasInteractionManager.AllPointersReleased += CanvasInteractionManagerOnAllPointersReleased;


            _minimap?.SwitchCollection(collection);
            
        }

        public void InvalidateMinimap()
        {
            _minimap.Invalidate();
        }

        private async void BtnDeleteOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_selectedLink is LinkRenderItem)
            {
                var linkItem = (LinkRenderItem) _selectedLink;
                await SessionController.Instance.LinksController.RemoveLink(linkItem.ViewModel.Controller.LibraryElementController.LibraryElementModel.LibraryElementId);
            }
            else if (_selectedLink is TrailRenderItem)
            {
                var trailItem = (TrailRenderItem)_selectedLink;
                trailItem.ViewModel.DeletePresentationLink();
            }
            RenderEngine.BtnDelete.IsVisible = false;
        }

        private void OnRenderItemPressed(BaseRenderItem item, CanvasPointer point)
        {
            if (!(item == RenderEngine.BtnDelete || item is LinkRenderItem || item is TrailRenderItem || item == RenderEngine.BtnExportTrail))
            {
                RenderEngine.BtnDelete.IsVisible = false;
                RenderEngine.BtnExportTrail.IsVisible = false;
            }
        }

        /// <summary>
        /// made edits to include HTML export
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pointer"></param>
        private void CollectionInteractionManagerOnTrailSelected(TrailRenderItem element, CanvasPointer pointer)
        {
            RenderEngine.BtnDelete.Transform.LocalPosition = pointer.CurrentPoint + new Vector2(0, -50);
            RenderEngine.BtnDelete.IsVisible = true;

            //HTML export
            RenderEngine.BtnExportTrail.Transform.LocalPosition = pointer.CurrentPoint + new Vector2(0, 40);
            RenderEngine.BtnExportTrail.IsVisible = true;

            _selectedLink = element;
        }

        private void CollectionInteractionManagerOnLinkSelected(LinkRenderItem element, CanvasPointer pointer)
        {
            RenderEngine.BtnDelete.Transform.LocalPosition = pointer.CurrentPoint + new Vector2(40,0);
            RenderEngine.BtnDelete.IsVisible = true;
            _selectedLink = element;

            RenderEngine.BtnExportTrail.IsVisible = false;
        }

        private void CollectionInteractionManagerOnSelectionPanZoomed(Vector2 center, Vector2 deltaTranslation,
            float deltaZoom)
        {
            var transform = RenderEngine.GetCollectionTransform(InitialCollection);
       
            foreach (var selection in Selections)
            {
                var elem = selection.ViewModel;
                if (elem == null)
                    continue;
                var imgCenter = new Vector2((float) (elem.X + elem.Width/2), (float) (elem.Y + elem.Height/2));
                var newCenter = Vector2.Transform(imgCenter, transform);

                RenderItemTransform t;
                if (_transformables.ContainsKey(elem))
                    t = _transformables[elem];
                else
                {
                    t = new RenderItemTransform();
                    t.LocalPosition = new Vector2((float)elem.X, (float)elem.Y);
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
                var nw = t.Size.Width*t.S.M11;
                var nh = t.Size.Height*t.S.M22;
                var dtx = (float) (t.Size.Width*t.S.M11 - t.Size.Width)/2f;
                var dty = (float) (t.Size.Height*t.S.M22 - t.Size.Height)/2f;
                var nx = t.LocalPosition.X - dtx;
                var ny = t.LocalPosition.Y - dty;
                elem.Controller.SetPosition(nx, ny);

                if (elem is AudioNodeViewModel)
                {
                    if (_currentAudioElementController?.Model?.Id == elem?.Controller?.Model?.Id &&
                        _currentAudioElementController?.Model?.Id != null)
                    {
                        xAudioPlayer?.SetSize(nw, nh);
                        var tt = ActiveAudioRenderItem.Transform.LocalToScreenMatrix;
                        var ct = (CompositeTransform) AudioPlayer.RenderTransform;
                        ct.TranslateX = tt.M31;
                        ct.TranslateY = tt.M32;
                        ct.ScaleX = tt.M11;
                        ct.ScaleY = tt.M22;
                    }
                }
                if (elem is VideoNodeViewModel)
                {
                    if (_currentVideoElementController?.Model?.Id == elem?.Controller?.Model?.Id &&
                        _currentVideoElementController?.Model?.Id != null)
                    {
                        xVideoPlayer?.SetSize(nw, nh);
                        var tt = ActiveVideoRenderItem.Transform.LocalToScreenMatrix;
                        var ct = (CompositeTransform) VideoPlayer.RenderTransform;
                        ct.TranslateX = tt.M31;
                        ct.TranslateY = tt.M32;
                        ct.ScaleX = tt.M11;
                        ct.ScaleY = tt.M22;
                    }
                }

                
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
                    (selection as CollectionRenderItem).InkRenderItem.UpdateDryInkTransform();
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
                var t = ActiveVideoRenderItem.Transform.LocalToScreenMatrix;
                var ct = (CompositeTransform)VideoPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;

                _currentVideoElementController = element?.ViewModel?.Controller;

                VideoPlayer.SetSize(element.ViewModel.Width, element.ViewModel.Height);
                VideoPlayer.SetLibraryElement(element.ViewModel.Controller.LibraryElementController as AudioLibraryElementController);
                VideoPlayer.Visibility = Visibility.Visible;
                return;
            }
            
            if (element is AudioElementRenderItem)
            {
                ActiveAudioRenderItem = (AudioElementRenderItem) element;
                var t = ActiveAudioRenderItem.Transform.LocalMatrix*RenderEngine.GetTransformUntil(ActiveAudioRenderItem);
                var ct = (CompositeTransform)AudioPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;

                _currentAudioElementController = element?.ViewModel?.Controller;

                AudioPlayer.SetSize(element.ViewModel.Width, element.ViewModel.Height);
                AudioPlayer.SetLibraryElement(element.ViewModel.Controller.LibraryElementController as AudioLibraryElementController);
                AudioPlayer.Visibility = Visibility.Visible;
                return;
            }
            
        }

        private void CollectionInteractionManagerOnResizerDragged(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            foreach (var item in Selections)
            {
                var elem = item;
                var collection = item.Parent as CollectionRenderItem;
                var s = collection.Camera.LocalToScreenMatrix.M11;
                var nw = elem.ViewModel.Width + delta.X/s;
                var nh = elem.ViewModel.Height + delta.Y/s;

                elem.ViewModel.SetSize(nw, nh); //You need to first set size of view model for proper image resizing
               
                item.ViewModel.Controller.SetSize(nw, nh);
                if(_currentAudioElementController?.Model?.Id == item?.ViewModel?.Controller?.Model?.Id && _currentAudioElementController?.Model?.Id != null)
                {
                    xAudioPlayer?.SetSize(nw, nh);
                }
                if (_currentVideoElementController?.Model?.Id == item?.ViewModel?.Controller?.Model?.Id && _currentVideoElementController?.Model?.Id != null)
                {
                    xVideoPlayer?.SetSize(nw, nh);
                }
            }

            _minimap.Invalidate();
        }

        private async void MultiMenuOnCreateCollection(bool finite, bool shaped)
        {
            var selections = Selections.ToArray();
            Selections.Clear();
           

            List<PointModel> shapePoints = null;
            double offsetX;
            double offsetY;

            Rect targetRectInCollection;
            var collectionTransform = CurrentCollection.Camera.LocalToScreenMatrix;
            if (shaped && _latestStroke != null)
            {
                targetRectInCollection = Geometry.PointCollecionToBoundingRect(_latestStroke);
                offsetX = targetRectInCollection.X - 50000;
                offsetY = targetRectInCollection.Y - 50000;
                foreach (var p in _latestStroke)
                {
                    p.X -= offsetX;
                    p.Y -= offsetY;
                }

                shapePoints = _latestStroke;
            }
            else
            {
                var selectionRect = RenderEngine.ElementSelectionRect.GetScreenBounds();;
                targetRectInCollection = Win2dUtil.TransformRect(selectionRect, Win2dUtil.Invert(collectionTransform));
            }

            if (shaped && _latestStroke != null)
            {
                shapePoints = _latestStroke;
            }
            else if ((shaped && _latestStroke == null) || finite)
            {

                shapePoints = new List<PointModel>
                {
                    new PointModel(50000, 50000),
                    new PointModel(50000 + targetRectInCollection.Width, 50000),
                    new PointModel(50000 + targetRectInCollection.Width, 50000 + targetRectInCollection.Height),
                    new PointModel(50000, 50000 + targetRectInCollection.Height),
                    new PointModel(50000, 50000)
                };
            }


            var createNewContentRequestArgs = new CreateNewCollectionContentRequestArgs()
            {
                LibraryElementArgs = new CreateNewCollectionLibraryElementRequestArgs()
                {
                    AccessType =
                        SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType,
                    LibraryElementType = NusysConstants.ElementType.Collection,
                    Title = "Unnamed Collection",
                    LibraryElementId = SessionController.Instance.GenerateId(),
                    IsFiniteCollection = finite,
                },
                ContentId = SessionController.Instance.GenerateId(),
                Shape = new CollectionShapeModel() {
                    ShapePoints = shapePoints,
                    AspectRatio = targetRectInCollection.Width / targetRectInCollection.Height,
                    ShapeColor = _latestStroke != null ? ColorExtensions.ToColorModel(CurrentCollection.InkRenderItem.InkColor) : ColorExtensions.ToColorModel(Colors.DarkSeaGreen)
                }
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
                Height = targetRectInCollection.Height,
                Width = targetRectInCollection.Width,
                X = targetRectInCollection.X,
                Y = targetRectInCollection.Y
            };

            // execute the add element to collection request
            var elementRequest = new NewElementRequest(newElementRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(
                    createNewContentRequestArgs.ContentId);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

            await elementRequest.AddReturnedElementToSessionAsync();

            offsetX = targetRectInCollection.X - 50000;
            offsetY = targetRectInCollection.Y - 50000;
            foreach (var element in selections)
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

        private async void CanvasInteractionManagerOnItemTapped(CanvasPointer pointer)
        {
            var item = RenderEngine.GetRenderItemAt(pointer.CurrentPoint);
            if (Selections.Count == 0)
                return;
            if (item == RenderEngine.ElementSelectionRect.BtnDelete)
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
            if (item == RenderEngine.ElementSelectionRect.BtnGroup)
            {
                multiMenu.Show(pointer.CurrentPoint.X + 50, pointer.CurrentPoint.Y, _latestStroke != null);
            }
            if (item == RenderEngine.ElementSelectionRect.BtnPresent)
            {
                //SessionController.Instance.SessionView.EnterPresentationMode(Selections[0].ViewModel);
                SessionController.Instance.NuSessionView.EnterPresentationMode(Selections[0].ViewModel);
                ClearSelections();
            }

            if (item == RenderEngine.ElementSelectionRect.BtnEnterCollection)
            {
                var id = Selections[0].ViewModel.LibraryElementId;
                await SessionController.Instance.EnterCollection(id);
            }

            if (item == RenderEngine.ElementSelectionRect.BtnPdfLeft)
            {
                var selection = (PdfElementRenderItem) Selections[0];
                selection.GotoPage(selection.CurrentPage - 1);
            }
            if (item == RenderEngine.ElementSelectionRect.BtnPdfRight)
            {
                var selection = (PdfElementRenderItem)Selections[0];
                selection.GotoPage(selection.CurrentPage + 1);
            }
            if (item == RenderEngine.ElementSelectionRect.BtnLayoutTool)
            {
                // Show the layout panel
                _layoutWindow = new LayoutWindowUIElement(RenderEngine.Root, RenderEngine.CanvasAnimatedControl);
                _layoutWindow.DoLayout += ArrangeCallback;
                _layoutWindow.Transform.LocalPosition = RenderEngine.ElementSelectionRect.Transform.LocalPosition;
                RenderEngine.Root.AddChild(_layoutWindow);
            }
            if (item == RenderEngine.ElementSelectionRect.BtnEditTags)
            {
                // edit tags
                _editTagsElement = new EditTagsUIElement(RenderEngine.Root, RenderEngine.CanvasAnimatedControl);
                RenderEngine.ElementSelectionRect.ElementSelectionRenderItemSizeChanged +=
                    _editTagsElement.UpdatePositionWithSize;
                Rect rect = RenderEngine.ElementSelectionRect.GetLocalBounds();
                RenderEngine.ElementSelectionRect.AddChild(_editTagsElement);
                _editTagsElement.Load();
            }
        }

        /// <summary>
        /// Does the layout for a custom layout by arranging the selected nodes along a stroke.
        /// </summary>
        /// <param name="sortedSelections"></param>
        private void CustomLayout(List<ElementRenderItem> sortedSelections)
        {
            if (sortedSelections.Count <= 1)
            {
                return;
            }

            var transform = RenderEngine.GetCollectionTransform(InitialCollection);
            var latestStroke = CurrentCollection.InkRenderItem.LatestStroke;
            var points = latestStroke.GetInkPoints().Select(p => new Vector2((float)p.Position.X, (float)p.Position.Y)).ToArray();
            if (2 <= points.Length)
            {
                var lineLength = 0.0f;
                var firstPoint = points[0];
                var pointDistances = new float[points.Length];
                pointDistances[0] = 0.0f;
                for (var p = 1; p < points.Length; p++)
                {
                    var point = points[p];
                    lineLength += Vector2.Distance(firstPoint, point);
                    firstPoint = point;
                    pointDistances[p] = lineLength;
                }

                var nodeDistance = lineLength / (sortedSelections.Count - 1);
                var nodeIndexInterval = points.Length / sortedSelections.Count;
                for (var n = 0; n < sortedSelections.Count; n++)
                {
                    var a = 0;
                    var b = 0;
                    for (var p = 0; p < points.Length; p++)
                    {
                        if (n == 0)
                        {
                            a = 0;
                            b = 1;
                            break;
                        } else if (n == sortedSelections.Count - 1)
                        {
                            a = points.Length - 2;
                            b = points.Length - 1;
                            break;
                        } else if (pointDistances[p] > n * nodeDistance)
                        {
                            a = p - 1;
                            b = p;

                            break;
                        }
                    }

                    var f = 1.0f;
                    if ((pointDistances[b] - pointDistances[a]) != 0.0f)
                    {
                        f = (n * nodeDistance - pointDistances[a]) / (pointDistances[b] - pointDistances[a]);
                    }

                    var interpolatedPoint = Vector2.Lerp(points[a], points[b], f);
                    var point = Vector2.Transform(interpolatedPoint, transform);
                    sortedSelections[n].ViewModel.Controller.SetPosition(interpolatedPoint.X, interpolatedPoint.Y);
                }
                return;
            }
        }

        /// <summary>
        /// Gets the current selections sorted by the LayoutSorting of the layout panel.
        /// </summary>
        /// <param name="sorting"></param>
        /// <returns></returns>
        private List<ElementRenderItem> SortedSelections(LayoutSorting sorting)
        {
            var sortedSelections = new List<ElementRenderItem>(Selections);

            switch (sorting)
            {
                case LayoutSorting.Title:
                    sortedSelections.Sort((x, y) => String.Compare(x.ViewModel.Model.Title, y.ViewModel.Model.Title, StringComparison.CurrentCultureIgnoreCase));
                    break;
                case LayoutSorting.Date:
                    sortedSelections.OrderBy(x => SessionController.Instance.ContentController.GetLibraryElementModel(x.ViewModel.Model.LibraryId).LastEditedTimestamp).ThenBy(x => x.ViewModel.Model.Title);
                    break;
            }

            return sortedSelections;
        } 

        /// <summary>
        /// Does the arrange for a given LayoutStyle and LayoutSorting.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="sorting"></param>
        private void ArrangeCallback(LayoutStyle style, LayoutSorting sorting)
        {
            var sortedSelections = SortedSelections(sorting);

            if (sortedSelections.Count <= 1)
            {
                return;
            }

            if (style == LayoutStyle.Custom)
            {
                CustomLayout(sortedSelections);
                return;
            }

            var transform = RenderEngine.GetCollectionTransform(InitialCollection);
            Vector2 start = new Vector2(float.MaxValue, float.MaxValue);

            // Do the layout
            start = sortedSelections.Aggregate(start, (current, elementRenderItem) => new Vector2((float) Math.Min(elementRenderItem.ViewModel.X, current.X), (float) Math.Min(elementRenderItem.ViewModel.Y, current.Y)));

            var nextPosition = start;
            var rows = (int) Math.Round(Math.Sqrt(sortedSelections.Count));
            var i = 0;
            var maxHeight = 0.0f;
            foreach (var elementRenderItem in sortedSelections)
            {
                elementRenderItem.ViewModel.Controller.SetPosition(nextPosition.X, nextPosition.Y);
                switch (style)
                {
                    case LayoutStyle.Horizontal:
                        nextPosition.X = (float)(nextPosition.X + ARRANGE_BORDER + elementRenderItem.ViewModel.Width);
                        break;
                    case LayoutStyle.Vertical:
                        nextPosition.Y = (float)(nextPosition.Y + ARRANGE_BORDER + elementRenderItem.ViewModel.Height);
                        break;
                    case LayoutStyle.Grid:
                        maxHeight = (float)Math.Max(maxHeight, elementRenderItem.ViewModel.Height);
                        if (i % rows == rows - 1)
                        {
                            nextPosition.Y = (float)(nextPosition.Y + ARRANGE_BORDER + maxHeight);
                            nextPosition.X = start.X;
                            maxHeight = 0.0f;
                        }
                        else
                        {
                            nextPosition.X = (float)(nextPosition.X + ARRANGE_BORDER + elementRenderItem.ViewModel.Width);
                        }
                        i++;
                        break;
                    default:
                        break;
                }
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

            foreach (var renderItem in CurrentCollection.GetChildren().OfType<ElementRenderItem>())
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
            //_transform = CurrentCollection.Camera.LocalToScreenMatrix;
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

           // collection.ViewModel.Controller.SetSize(500,500);
          //  collection.ViewModel.Controller.SetPosition(50000,50000);

            var targetPoint = RenderEngine.ScreenPointerToCollectionPoint(pointer.CurrentPoint, collection);
            var target = new Vector2(targetPoint.X - (float) element.ViewModel.Width/2f, targetPoint.Y - (float) element.ViewModel.Height/2f);
            var elementId = element.ViewModel.Id;
            var parentCollectionId = element.ViewModel.Controller.GetParentCollectionId();
            await element.ViewModel.Controller.RequestMoveToCollection(collection.ViewModel.Model.LibraryId, target.X, target.Y);

            var oldLocationScreen = new Point2d(pointer.StartPoint.X, pointer.StartPoint.Y);
            var oldLocationCollectionV = RenderEngine.ScreenPointerToCollectionPoint(pointer.StartPoint, CurrentCollection);
            var oldLocationCollection = new Point2d(oldLocationCollectionV.X, oldLocationCollectionV.Y);
            var newLocation = new Point2d(target.X, target.Y);
            var action = new MoveToCollectionAction(elementId, parentCollectionId, collection.ViewModel.Model.LibraryId, oldLocationCollection, newLocation);
            
            ActivateUndo(action, oldLocationScreen);

            _minimap.Invalidate();
        }

        private void CollectionInteractionManagerOnInkStopped(CanvasPointer pointer)
        {
            CurrentCollection.InkRenderItem.StopInkByEvent(pointer);
            if (pointer.DistanceTraveled < 20 && (DateTime.Now - pointer.StartTime).TotalMilliseconds > 500)
            {
                var screenBounds = CoreApplication.MainView.CoreWindow.Bounds;
                var optionsBounds = RenderEngine.InkOptions.GetLocalBounds();
                var targetPoint = pointer.CurrentPoint;
                if (targetPoint.X < screenBounds.Width/2)
                {
                    targetPoint.X += 20;
                }
                else
                {
                    targetPoint.X -= (20 + (float) optionsBounds.Width);
                }
                targetPoint.Y -= (float) optionsBounds.Height/2;
                targetPoint.X = (float) Math.Min(screenBounds.Width - optionsBounds.Width, Math.Max(0, targetPoint.X));
                targetPoint.Y = (float) Math.Min(screenBounds.Height - optionsBounds.Height, Math.Max(0, targetPoint.Y));
                RenderEngine.InkOptions.Transform.LocalPosition = targetPoint;
                RenderEngine.InkOptions.IsVisible = true;
                CurrentCollection.InkRenderItem.RemoveLatestStroke();
            }
            else
            {
                _layoutWindow?.NotifyArrangeCustom();
            }
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
                Win2dUtil.Invert(RenderEngine.GetTransformUntil(element)));
            element.ViewModel.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y);
        }

        private void CollectionInteractionManagerOnItemMoved(CanvasPointer pointer, ElementRenderItem elem,
            Vector2 delta)
        {
            if (elem is PseudoElementRenderItem)
                return;
            if (elem.ViewModel == null)
                return;

            var collection = elem.Parent as CollectionRenderItem;

            var newX = elem.ViewModel.X + delta.X/(_transform.M11*collection.Camera.S.M11);
            var newY = elem.ViewModel.Y + delta.Y/(_transform.M22*collection.Camera.S.M22);

            if (!Selections.Contains(elem))
            {
                elem.ViewModel.Controller.SetPosition(newX, newY);
                if (elem is CollectionRenderItem)
                {
                    (elem as CollectionRenderItem).InkRenderItem?.UpdateDryInkTransform();
                }

            }
            else
            {
                foreach (var selectable in Selections)
                {
                    var e = selectable.ViewModel;
                    var newXe = e.X + delta.X/(_transform.M11*collection.Transform.LocalScale.X * collection.Camera.S.M11);
                    var newYe = e.Y + delta.Y/(_transform.M11 * collection.Transform.LocalScale.Y * collection.Camera.S.M11);
                    e.Controller.SetPosition(newXe, newYe);
                    if (selectable is CollectionRenderItem)
                    {
                        (selectable as CollectionRenderItem).InkRenderItem?.UpdateDryInkTransform();
                    }
                }
            }

            _minimap.Invalidate();
            UpdateMediaPlayer();
        }

        private void CanvasInteractionManagerOnPointerPressed(CanvasPointer pointer)
        {
            _transform = CurrentCollection.Transform.LocalToScreenMatrix;
            if (ActiveAudioRenderItem?.HitTest(pointer.CurrentPoint) == null ||
                ActiveVideoRenderItem?.HitTest(pointer.CurrentPoint) == null)
            {
                xAudioPlayer.Visibility = Visibility.Collapsed;
                xAudioPlayer.Pause();
                xVideoPlayer.Visibility = Visibility.Collapsed;
                xVideoPlayer.Pause();
            }
        }

        private void CollectionInteractionManagerOnPanZoomed(Vector2 center, Vector2 deltaTranslation, float deltaZoom)
        {
            PanZoom2(CurrentCollection.Camera, _transform, center, deltaTranslation.X/_transform.M11,
                deltaTranslation.Y/_transform.M11, deltaZoom);

            CurrentCollection.InkRenderItem?.UpdateDryInkTransform();
            // update the ink transform of all children. Currently one level only.
            foreach (var childCollection in CurrentCollection.GetChildren().OfType<CollectionRenderItem>())
            {
                childCollection.InkRenderItem?.UpdateDryInkTransform();
            }

            UpdateNonWin2dElements();
            _minimap.Invalidate();
        }

        private void CollectionInteractionManagerOnPanned(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            if (ToolsAreBeingInteractedWith)
                return;
            PanZoom2(CurrentCollection.Camera, _transform, point, delta.X/_transform.M11, delta.Y/_transform.M11, 1);
            CurrentCollection.InkRenderItem?.UpdateDryInkTransform();

            // update the ink transform of all children. Currently one level only.
            foreach (var childCollection in CurrentCollection.GetChildren().OfType<CollectionRenderItem>())
            {
                childCollection.InkRenderItem?.UpdateDryInkTransform();
            }

            UpdateNonWin2dElements();
            _minimap.Invalidate();

            // Maybe give this a minimum delta?
            CanvasPanned?.Invoke(this, true);
            
        }

        private void CollectionInteractionManagerOnSelectionsCleared()
        {
            if (!_inkPressed)
                ClearSelections();

            _minimap.Invalidate();

            if (_layoutWindow != null)
            {
                RenderEngine.Root.RemoveChild(_layoutWindow);
                _layoutWindow = null;
            }

            if (_editTagsElement != null)
            {
                RenderEngine.ElementSelectionRect.RemoveChild(_editTagsElement);
                _editTagsElement = null;
            }
        }

        private async void OnDuplicateCreated(ElementRenderItem element, Vector2 point)
        {
            var targetPoint = Vector2.Transform(point,
                Win2dUtil.Invert(RenderEngine.GetTransformUntil(element)));
            element.ViewModel.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y);
        }


        private void OnItemDoubleTapped(BaseRenderItem item)
        {
            if (item == CurrentCollection || item == InitialCollection)
                return;

            if (item is ElementRenderItem)
            {
                var libraryElementModelId = (item as ElementRenderItem)?.ViewModel?.Controller?.LibraryElementModel?.LibraryElementId;
                if (libraryElementModelId != null)
                {
                    var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
                    SessionController.Instance.NuSessionView.ShowDetailView(controller);
                }
            } else if (item is LinkRenderItem)
            {
                var libraryElementModelId = (item as LinkRenderItem).ViewModel.Controller.LibraryElementController.LibraryElementModel.LibraryElementId;
                var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
                SessionController.Instance.NuSessionView.ShowDetailView(controller);
            }

        }

        private void CollectionInteractionManagerOnItemTapped(ElementRenderItem element)
        {
            if (SessionController.IsReadonly)
            {
                Debug.Assert(element?.ViewModel?.Id != null);
                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.CenterCameraOnElement(element.ViewModel.Id);
            }
            else
            {
                AddToSelections(element);
            }
            // add the bread crumb

            if (element?.ViewModel?.Controller?.LibraryElementModel != null)
            {
                SessionController.Instance.NuSessionView.TrailBox.AddBreadCrumb(
                    CurrentCollection.ViewModel.Controller.LibraryElementController, element.ViewModel.Controller);
            }

        }

        public void AddToSelections(ElementRenderItem element)
        {
            if (element is ToolWindow)
            {
                return;
            }
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
                var t = ActiveVideoRenderItem.Transform.LocalMatrix * RenderEngine.GetTransformUntil(ActiveVideoRenderItem);
                var ct = (CompositeTransform)SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.RenderTransform;
                ct.TranslateX = t.M31;
                ct.TranslateY = t.M32;
                ct.ScaleX = t.M11;
                ct.ScaleY = t.M22;
            }
            
            if (ActiveAudioRenderItem != null)
            {
                var t = ActiveAudioRenderItem.Transform.LocalMatrix * RenderEngine.GetTransformUntil(ActiveAudioRenderItem);
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

            // put a bound on how much we zoom out
            if (nsx > 0.01 && nsy > 0.01)
            {
                target.LocalPosition = new Vector2(ntx, nty);
                target.LocalScaleCenter = new Vector2(ncx, ncy);
                target.LocalScale = new Vector2(nsx, nsy);
            }
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

        /// <summary>
        /// Method used to show the full-screen image of any uri. 
        /// This will be a pan-zoomable interface for viewing images
        /// </summary>
        public void ShowFullScreenImage(Uri imageUri)
        {
            UITask.Run(delegate
            {
                xFullScreenImageViewer.IsHitTestVisible = true;
                xWrapper.IsHitTestVisible = false;
                Debug.Assert(imageUri != null);
                xFullScreenImageViewer.ShowImage(imageUri);
            });
        }

        public void PlayFullScreenVideo(VideoLibraryElementController videoLibraryElementController, bool addRegionsIsVisible = false)
        {
            UITask.Run(delegate
            {
                // set the visibility of items
                xFullScreenVideoElement.Visibility = Visibility.Visible;
                xFullScreenVideoCloseButton.Visibility = Visibility.Visible;
                xFullScreenVideoBackground.Visibility = Visibility.Visible;
                xFullScreenVideoAddRegionButton.Visibility = addRegionsIsVisible
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                xAddRegionMenu.Visibility = Visibility.Collapsed;
                xAddPublicRadioButton.IsChecked = true;
                xAddPrivateRadioButton.IsChecked = false;

                xFullScreenVideoAddRegionButton.Background = new SolidColorBrush(Constants.DARK_BLUE);
                xFullScreenVideoAddRegionButton.Foreground = new SolidColorBrush(Colors.White);
                xFullScreenVideoCloseButton.Background = new SolidColorBrush(Constants.DARK_BLUE);
                xFullScreenVideoCloseButton.Foreground = new SolidColorBrush(Colors.White);
                xAddRegionMenu.Background = new SolidColorBrush(Colors.White);
                xAddRegionMenu.BorderThickness = new Thickness(1, 1, 1, 1);
                xAddRegionMenu.BorderBrush = new SolidColorBrush(Constants.DARK_BLUE);

                // set the size of the full screen element
                xFullScreenVideoElement.SetSize(SessionController.Instance.ScreenWidth,
                    SessionController.Instance.ScreenHeight - xFullScreenVideoAddRegionButton.Height);
                xFullScreenVideoElement.SetLibraryElement(videoLibraryElementController);

                // set the position of the add region button
                Canvas.SetTop(xFullScreenVideoAddRegionButton, SessionController.Instance.ScreenHeight - xFullScreenVideoAddRegionButton.Height - 20);
                Canvas.SetLeft(xFullScreenVideoAddRegionButton, SessionController.Instance.ScreenWidth/2 - xFullScreenVideoAddRegionButton.Width/2 - 150);

                // set the positon of the add region menu
                Canvas.SetTop(xAddRegionMenu, SessionController.Instance.ScreenHeight/2 - xAddRegionMenu.Height/2);
                Canvas.SetLeft(xAddRegionMenu, SessionController.Instance.ScreenWidth/2 - xAddRegionMenu.Width/2);

                // set the position of the close button
                Canvas.SetTop(xFullScreenVideoCloseButton, SessionController.Instance.ScreenHeight - xFullScreenVideoCloseButton.Height - 20);
                Canvas.SetLeft(xFullScreenVideoCloseButton, SessionController.Instance.ScreenWidth/2 - xFullScreenVideoCloseButton.Width / 2 + 150);
            });
        }

        private void XFullScreenVideoCloseButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            xFullScreenVideoElement.Visibility = Visibility.Collapsed;
            xFullScreenVideoCloseButton.Visibility = Visibility.Collapsed;
            xFullScreenVideoBackground.Visibility = Visibility.Collapsed;
            xFullScreenVideoAddRegionButton.Visibility = Visibility.Collapsed;
            xAddRegionMenu.Visibility = Visibility.Collapsed;;
            xFullScreenVideoElement.Pause();

        }

        private void XFullScreenVideoAddRegionButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {

            //toggle the visibility of the add region menu
            xAddRegionMenu.Visibility = xAddRegionMenu.Visibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private async void XFullScreenVideoSubmit_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.Assert(xAddPublicRadioButton.IsChecked == true || xAddPrivateRadioButton.IsChecked == true);


            // get appropriate new region message based on the current controller
            var controller = xFullScreenVideoElement.CurrentLibraryElementController as VideoLibraryElementController;
            Debug.Assert(controller != null);
            var videoModel = controller.VideoLibraryElementModel;
            Debug.Assert(videoModel != null);
            var regionRequestArgs = new CreateNewVideoLibraryElementRequestArgs
            {
                StartTime = videoModel.NormalizedStartTime + videoModel.NormalizedDuration * .25,
                Duration = videoModel.NormalizedDuration * .5,
                AspectRatio = videoModel.Ratio
            };

            //create the args and set the parameters that all regions will need
            regionRequestArgs.ContentId = controller.LibraryElementModel.ContentDataModelId;
            regionRequestArgs.LibraryElementType = controller.LibraryElementModel.Type;
            regionRequestArgs.Title = "Region " + controller.Title; // TODO factor out this hard-coded string to a constant
            regionRequestArgs.ParentLibraryElementId = controller.LibraryElementModel.LibraryElementId;
            regionRequestArgs.Large_Thumbnail_Url = controller.LibraryElementModel.LargeIconUrl;
            regionRequestArgs.Medium_Thumbnail_Url = controller.LibraryElementModel.MediumIconUrl;
            regionRequestArgs.Small_Thumbnail_Url = controller.LibraryElementModel.SmallIconUrl;
            regionRequestArgs.AccessType = xAddPublicRadioButton.IsChecked == true
                ? NusysConstants.AccessType.Public
                : NusysConstants.AccessType.Private;

            var request = new CreateNewLibraryElementRequest(regionRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedLibraryElementToLibrary();

            xAddRegionMenu.Visibility = Visibility.Collapsed;
        }
    }
}