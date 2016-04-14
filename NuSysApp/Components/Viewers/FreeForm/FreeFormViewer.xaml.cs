﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Xaml.Input;
using Windows.UI.Input.Inking;


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
        private WetDryInkCanvas _inqCanvas;

        public FreeFormViewer(FreeFormViewerViewModel vm)
        {
            this.InitializeComponent();

            // var inqCanvasModel = vm.Controller.Model.InqCanvas;
            // var inqCanvasViewModel = new InqCanvasViewModel(inqCanvasModel, new Size(Constants.MaxCanvasSize, Constants.MaxCanvasSize));
            // _inqCanvas = new InqCanvasView(inqCanvasViewModel);
            // _inqCanvas.Width = Window.Current.Bounds.Width;
            // _inqCanvas.Height = Window.Current.Bounds.Height;

            _inqCanvas = new WetDryInkCanvas(wetCanvas, dryCanvas);

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                _inqCanvas.InkStrokeAdded += InkStrokedAdded;
                _inqCanvas.InkStrokeRemoved += InkStrokedRemoved;

                _nodeManipulationMode = new NodeManipulationMode(this);
                _createGroupMode = new CreateGroupMode(this);
                _duplicateMode = new DuplicateNodeMode(this);
                _panZoomMode = new PanZoomMode(this);
                _gestureMode = new GestureMode(this);
                _selectMode = new SelectMode(this);
                _floatingMenuMode = new FloatingMenuMode(this);
                _globalInkMode = new GlobalInkMode(this);

                _tagMode = new TagNodeMode(this);
                _linkMode = new LinkMode(this);
                _mainMode = new MultiMode(this, _panZoomMode, _selectMode, _nodeManipulationMode, _createGroupMode, _floatingMenuMode);
                _simpleEditMode = new MultiMode(this, _panZoomMode, _selectMode, _nodeManipulationMode, _floatingMenuMode);
                _simpleEditGroupMode = new MultiMode(this,  _panZoomMode, _selectMode, _floatingMenuMode);

                SwitchMode(Options.SelectNode, false);
            };

            SizeChanged += delegate(object sender, SizeChangedEventArgs args)
            {
                xInqCanvasContainer.Width = args.NewSize.Width;
                xInqCanvasContainer.Height = args.NewSize.Height;
            };

            // TODO:refactor
            _inqCanvas.Transform = vm.CompositeTransform;

            vm.SelectionChanged += VmOnSelectionChanged;
            vm.Controller.Model.InqCanvas.LineFinalized += InqCanvasOnLineFinalized;

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void InkStrokedAdded(WetDryInkCanvas canvas, InkStroke stroke)
        {
            Debug.WriteLine("line added");
        }

        private void InkStrokedRemoved(WetDryInkCanvas canvas, InkStroke stroke)
        {
            Debug.WriteLine("line removed");
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
                //var oldIndex = xOuterWrapper.Children.IndexOf(_inqCanvas);
                //xOuterWrapper.Children.Move((uint)oldIndex, (uint)(xOuterWrapper.Children.Count - 1));
            }
            else if (vm.Selections.Count == 1)
            {
                if (vm.Selections[0].ElementType == ElementType.Collection)
                    SetViewMode(_simpleEditGroupMode);
                else
                    SetViewMode(_simpleEditMode);
              //  var oldIndex = xOuterWrapper.Children.IndexOf(_inqCanvas);
              //  xOuterWrapper.Children.Move((uint)oldIndex, 0);
            }
            else
            {
                SetViewMode(_mainMode);
              //  var oldIndex = xOuterWrapper.Children.IndexOf(_inqCanvas);
               // xOuterWrapper.Children.Move((uint)oldIndex, 0);
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

        public WetDryInkCanvas InqCanvas
        {
            get { return _inqCanvas; }
        }

        public async Task SetViewMode(AbstractWorkspaceViewMode mode, bool isFixed = false)
        {
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
            }
        }

        public SelectMode SelectMode { get { return _selectMode; } }
    }
}