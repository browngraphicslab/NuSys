using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NuSysApp
{
    /// <summary>
    /// This is the view for the entire workspace. It instantiates the WorkspaceViewModel. 
    /// </summary>
    public sealed partial class FreeFormViewer
    {
        private AbstractWorkspaceViewMode _mode;
        private InqCanvasView _inqCanvas;
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

        public FreeFormViewer(FreeFormViewerViewModel vm)
        {
            this.InitializeComponent();
 
            var inqCanvasModel = vm.Controller.Model.InqCanvas;
            var inqCanvasViewModel = new InqCanvasViewModel(inqCanvasModel, new Size(Constants.MaxCanvasSize, Constants.MaxCanvasSize));
            _inqCanvas = new InqCanvasView(inqCanvasViewModel);
            _inqCanvas.Width = Window.Current.Bounds.Width;
            _inqCanvas.Height = Window.Current.Bounds.Height;
            xOuterWrapper.Children.Add(_inqCanvas);


            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                _nodeManipulationMode = new NodeManipulationMode(this);
                _createGroupMode = new CreateGroupMode(this);
                _duplicateMode = new DuplicateNodeMode(this);
                _panZoomMode = new PanZoomMode(this);
                _gestureMode = new GestureMode(this);
                _selectMode = new SelectMode(this);
                _floatingMenuMode = new FloatingMenuMode(this);

                _tagMode = new TagNodeMode(this);
                _linkMode = new LinkMode(this);
                _mainMode = new MultiMode(this, _selectMode, _floatingMenuMode, _gestureMode, _nodeManipulationMode, _createGroupMode, _duplicateMode, _panZoomMode, _tagMode, _linkMode);
                _simpleEditMode = new MultiMode(this, _selectMode, _nodeManipulationMode, _floatingMenuMode);
                SwitchMode(Options.SelectNode, false);
            };
            
            // TODO:refactor
            /*
            CompositeTransform ct = new CompositeTransform();
            ct.CenterX = wsModel.CenterX;
            ct.CenterY = wsModel.CenterY;
            ct.ScaleX = wsModel.Zoom;
            ct.ScaleY = wsModel.Zoom;
            ct.TranslateX = wsModel.LocationX;
            ct.TranslateY = wsModel.LocationY;
            _inqCanvas.Transform = ct;
            */

            _inqCanvas.Transform = vm.CompositeTransform;

            vm.SelectionChanged += VmOnSelectionChanged;
            vm.Controller.Model.InqCanvas.LineFinalized += InqCanvasOnLineFinalized;

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void ControllerOnDisposed(object source)
        {
            _nodeManipulationMode.Deactivate();
            _createGroupMode.Deactivate();
            _duplicateMode.Deactivate();
            _panZoomMode.Deactivate();
            _gestureMode.Deactivate();
            _selectMode.Deactivate();
            _floatingMenuMode.Deactivate();

            _tagMode.Deactivate();
            _linkMode.Deactivate();
            _mainMode.Deactivate();
            _simpleEditMode.Deactivate();
 
        

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
            if (vm.Selections.Count == 0)
            {
                SetViewMode(_mainMode);
                var oldIndex = xOuterWrapper.Children.IndexOf(_inqCanvas);
                xOuterWrapper.Children.Move((uint)oldIndex, (uint)(xOuterWrapper.Children.Count - 1));
            }
            else if (vm.Selections.Count == 1)
            {
                SetViewMode(_simpleEditMode);
                var oldIndex = xOuterWrapper.Children.IndexOf(_inqCanvas);
                xOuterWrapper.Children.Move((uint)oldIndex, 0);
            }
            else
            {
                SetViewMode(_mainMode);
                var oldIndex = xOuterWrapper.Children.IndexOf(_inqCanvas);
                xOuterWrapper.Children.Move((uint)oldIndex, 0);
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

        public InqCanvasView InqCanvas
        {
            get { return _inqCanvas; }
        }

        public async Task SetViewMode(AbstractWorkspaceViewMode mode, bool isFixed = false)
        {
            if (mode == _mode)
                return;

            var deactivate = _mode?.Deactivate();
            if (deactivate != null) await deactivate;
            _mode = mode;
            await _mode.Activate();
        }

        public async void SwitchMode(Options mode, bool isFixed)
        {
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
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new LinkMode(this), _gestureMode));
                    break;
            }
        }

        public SelectMode SelectMode { get { return _selectMode; } }
    }
}