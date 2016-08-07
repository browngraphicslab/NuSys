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
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NuSysApp
{
    /// <summary>
    /// This is the view for the entire workspace. It instantiates the WorkspaceViewModel. 
    /// </summary>
    public sealed partial class FreeFormViewer
    {
        private AbstractWorkspaceViewMode _mode;
        private NodeManipulationMode _nodeManipulationMode;
        private CreateGroupMode _createGroupMode;
        private DuplicateNodeMode _duplicateMode;
        private PanZoomMode _panZoomMode;
        private SelectMode _selectMode;
        private TagNodeMode _tagMode;
        private LinkMode _linkMode;
        private GestureMode _gestureMode;
        private FloatingMenuMode _floatingMenuMode;
        private MultiMode _mainMode;
        private MultiMode _simpleEditMode;
        private MultiMode _simpleEditGroupMode;
        private GlobalInkMode _globalInkMode;
        private AbstractWorkspaceViewMode _prevMode;
        private NuSysInqCanvas _inqCanvas;
        private ExploreMode _exploreMode;
        private MultiMode _explorationMode;
        private List<uint> _activePointers = new List<uint>();

        public CanvasAnimatedControl RenderCanvas => xRenderCanvas;

        public FreeFormViewer(FreeFormViewerViewModel vm)
        {
            this.InitializeComponent();

            vm.SelectionChanged += VmOnSelectionChanged;
            vm.Controller.Model.InqCanvas.LineFinalized += InqCanvasOnLineFinalized;
            vm.Controller.Disposed += ControllerOnDisposed;

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {


            //    xRenderCanvas.PointerPressed += XRenderCanvasOnPointerPressed;

    //            _inqCanvas = new NuSysInqCanvas(wetCanvas, dryCanvas);


                var collectionModel = (CollectionLibraryElementModel)SessionController.Instance.ContentController.GetContent(vm.Controller.LibraryElementModel.LibraryElementId);
                collectionModel.OnInkAdded += delegate(string id)
                {
                    var x = InkStorage._inkStrokes[id];
                    if (x.Type == "ink")
                    {
                        NuSysRenderer.Instance.InitialCollection.AddStroke(x.Stroke);
                        //  _inqCanvas.AddStroke(x.Stroke);
                        //   _inqCanvas.Redraw();
                    }
                    else
                    {
                      //  _inqCanvas.AddAdorment(x.Stroke, x.Color, false);
                   //     _inqCanvas.Redraw();
                    }
                };

                await NuSysRenderer.Instance.Init(xRenderCanvas);

                xRenderCanvas.PointerPressed += XRenderCanvasOnPointerPressed;
                xRenderCanvas.PointerReleased += XRenderCanvasOnPointerReleased;


            };

            SizeChanged += delegate(object sender, SizeChangedEventArgs args)
            {
                xRenderCanvas.Width = args.NewSize.Width;
                xRenderCanvas.Height = args.NewSize.Height;
            };
        }

        private void XRenderCanvasOnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                _activePointers.Remove(args.Pointer.PointerId);
        }

        private void XRenderCanvasOnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                _activePointers.Add(args.Pointer.PointerId);
        }

        

        private void AdornmentRemoved(WetDryInkCanvas canvas, InkStroke stroke)
        {
            var request = InkStorage.CreateRemoveInkRequest(new InkWrapper(stroke, "adornment"));
            if (request == null)
                return;
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request.Item1);

            var m = new Message();
            m["contentId"] = ((ElementViewModel)DataContext).Controller.LibraryElementModel.LibraryElementId;
            var model = ((ElementViewModel)DataContext).Controller.LibraryElementModel as CollectionLibraryElementModel;
            model.InkLines.Remove(request.Item2);
            m["inklines"] = new HashSet<string>(model.InkLines);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(m));

        }

        private async void AdormnentAdded(WetDryInkCanvas canvas, InkStroke inkStroke)
        {
            var id = SessionController.Instance.GenerateId();
            InkStorage._inkStrokes.Add(id, new InkWrapper(inkStroke, "adornment"));//"adornment", inkStroke));

            var request = InkStorage.CreateAddInkRequest(id, inkStroke, "adornment", MultiSelectMenuView.SelectedColor);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);

            var m = new Message();
            m["contentId"] = ((ElementViewModel)DataContext).Controller.LibraryElementModel.LibraryElementId;
            var model = ((ElementViewModel)DataContext).Controller.LibraryElementModel as CollectionLibraryElementModel;
            model.InkLines.Add(id);
            m["inklines"] = new HashSet<string>(model.InkLines);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(m));
        }

        private async void InkStrokedAdded(WetDryInkCanvas canvas, InkStroke stroke)
        {
            var id = SessionController.Instance.GenerateId();
            InkStorage._inkStrokes.Add(id, new InkWrapper(stroke, "ink"));

            var request = InkStorage.CreateAddInkRequest(id, stroke, "ink", Colors.Black );
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);

            var m = new Message();
            m["contentId"] = ((ElementViewModel)DataContext).Controller.LibraryElementModel.LibraryElementId;
            var model = ((ElementViewModel)DataContext).Controller.LibraryElementModel as CollectionLibraryElementModel;
            model.InkLines.Add(id);
            m["inklines"] = new HashSet<string>(model.InkLines);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(m));
        }

        private void InkStrokedRemoved(WetDryInkCanvas canvas, InkStroke stroke)
        {
            var request = InkStorage.CreateRemoveInkRequest(new InkWrapper(stroke, "ink"));
            if (request == null)
                return;
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request.Item1);

            var m = new Message();
            m["contentId"] = ((ElementViewModel)DataContext).Controller.LibraryElementModel.LibraryElementId;
            var model = ((ElementViewModel)DataContext).Controller.LibraryElementModel as CollectionLibraryElementModel;
            model.InkLines.Remove(request.Item2);
            m["inklines"] = new HashSet<string>(model.InkLines);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(m));
        }

        private void ControllerOnDisposed(object source, object args)
        {
            _nodeManipulationMode?.Deactivate();
            _createGroupMode?.Deactivate();
            _duplicateMode?.Deactivate();
            _panZoomMode?.Deactivate();
            _gestureMode?.Deactivate();
            _selectMode?.Deactivate();
            _floatingMenuMode?.Deactivate();

            _tagMode?.Deactivate();
            _linkMode?.Deactivate();
            _mainMode?.Deactivate();
            _simpleEditMode?.Deactivate();
 
        

            var vm = (FreeFormViewerViewModel)DataContext;
            vm.SelectionChanged -= VmOnSelectionChanged;
            vm.Controller.Model.InqCanvas.LineFinalized -= InqCanvasOnLineFinalized;
            vm.Controller.Disposed -= ControllerOnDisposed;
            _mode = null;

            _inqCanvas = null;
        }

        private void InqCanvasOnLineFinalized(InqLineModel model)
        {
            var vm = (FreeFormViewerViewModel)DataContext;
            if (!model.IsGesture)
            {
                //var createdTag = await CheckForTagCreation(model);
                //if (createdTag)
                //{
                //    model.Delete();
                //}
                return;
            }

            var gestureType = GestureRecognizer.Classify(model);
            switch (gestureType)
            {
                case GestureRecognizer.GestureType.None:
                    break;
                case GestureRecognizer.GestureType.Scribble:
                    var deletedSome = vm.CheckForInkNodeIntersection(model);
                    if (deletedSome)
                        model.Delete();
                    break;
            }
        }

        private void VmOnSelectionChanged(object source)
        {
            var vm = (FreeFormViewerViewModel) DataContext;
           
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
            get { return _inqCanvas; }
        }

        public async Task SetViewMode(AbstractWorkspaceViewMode mode, bool isFixed = false)
        {
            return;
            _prevMode = _mode;
            if (mode == _mode)
                return;

            var deactivate = _mode?.Deactivate();
            if (deactivate != null) await deactivate;
            _mode = mode;
            await _mode.Activate();
        }

        public async void SwitchMode(Options mode, bool isFixed)
        {
            return;
           
            switch (mode)
            {
                case Options.Idle:
                    SetViewMode(new MultiMode(this));
                    break;
                case Options.SelectNode:
                    await
                        SetViewMode(_mainMode);
                    break;
                case Options.PenGlobalInk:
                  //  await SetViewMode(_globalInkMode);
                    break;
                case Options.Exploration:
                    SetViewMode(_explorationMode);
                    break;
            }
        }


        public PanZoomMode PanZoom
        {
            get { return _panZoomMode; }
        }


        public SelectMode SelectMode { get { return _selectMode; } }

        public void ChangeMode(object source, Options mode)
        {
            return;
            SwitchMode(mode, false);
        }
    }
}