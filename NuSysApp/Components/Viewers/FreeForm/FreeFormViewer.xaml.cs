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

                _tagMode = new TagNodeMode(this);
                _linkMode = new LinkMode(this);
                _mainMode = new MultiMode(this, _selectMode, _gestureMode, _nodeManipulationMode, _createGroupMode, _duplicateMode, _panZoomMode, _tagMode, _linkMode);
                _simpleEditMode = new MultiMode(this, _selectMode);
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


            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                //SwitchMode(Options.SelectNode, false);
            };

            inqCanvasModel.AppSuspended += delegate()
            {
                //_inqCanvas.DisposeResources();
            };

            vm.SelectionChanged += delegate(object source)
            {
                
                if (vm.Selections.Count == 0)
                {
                    SetViewMode(_mainMode);
                    var oldIndex = xOuterWrapper.Children.IndexOf(_inqCanvas);
                    xOuterWrapper.Children.Move((uint)oldIndex, (uint)(xOuterWrapper.Children.Count-1));
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
            };

            vm.Controller.Model.InqCanvas.LineFinalized += async delegate (InqLineModel model)
            {
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
            };
        }


        //called after translation event to make nodes not currently visible not display XAML
        public void CullNodes()
        {
            var vm = this.DataContext as FreeFormViewerViewModel;
            if (vm == null)
            {
                return;
            }

            var trans = vm.CompositeTransform;
            var sessionView = SessionController.Instance.SessionView;

            Point upperLeft = trans.Inverse.TransformPoint(new Point(0, 0));
            Point bottomRight =
                trans.Inverse.TransformPoint(new Point(sessionView.ActualWidth, sessionView.ActualHeight));
            Rect screenBounds = new Rect(upperLeft, bottomRight);

            foreach (var atom in vm.AtomViewList)
            {
                var atomVm = atom.DataContext as ElementViewModel;
                if (atomVm == null)
                {
                    continue;
                }

                var atomTopLeft = new Point(atomVm.Anchor.X, atomVm.Anchor.Y);
                var atomBottomRight = new Point(atomTopLeft.X + atomVm.Width, atomTopLeft.Y + atomVm.Height);
                if (!screenBounds.Contains(atomTopLeft) && !screenBounds.Contains(atomBottomRight)
                    && !screenBounds.Contains(new Point(atomTopLeft.X, atomBottomRight.Y)) && !screenBounds.Contains(new Point(atomTopLeft.Y, atomBottomRight.X)))
                {
                    atomVm.IsOnScreen = false;
                }
                else
                {
                    atomVm.IsOnScreen = true;
                }
            }
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