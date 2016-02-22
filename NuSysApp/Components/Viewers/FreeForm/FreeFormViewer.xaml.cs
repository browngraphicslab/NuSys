using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media.Animation;

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

        public FreeFormViewer(FreeFormViewerViewModel vm)
        {
            this.InitializeComponent();
            var wsModel = (WorkspaceModel)vm.Model;

            var inqCanvasModel = wsModel.InqCanvas;
            var inqCanvasViewModel = new InqCanvasViewModel(inqCanvasModel, new Size(Constants.MaxCanvasSize, Constants.MaxCanvasSize));
            _inqCanvas = new InqCanvasView(inqCanvasViewModel);
            _inqCanvas.Width = Window.Current.Bounds.Width;
            _inqCanvas.Height = Window.Current.Bounds.Height;
            xOuterWrapper.Children.Add(_inqCanvas);
            CompositeTransform ct = new CompositeTransform();
            ct.CenterX = wsModel.CenterX;
            ct.CenterY = wsModel.CenterY;
            ct.ScaleX = wsModel.Zoom;
            ct.ScaleY = wsModel.Zoom;
            ct.TranslateX = wsModel.LocationX;
            ct.TranslateY = wsModel.LocationY;
            _inqCanvas.Transform = ct;


            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                SwitchMode(Options.SelectNode, false);
            };

            inqCanvasModel.AppSuspended += delegate()
            {
                //_inqCanvas.DisposeResources();
            };

            wsModel.InqCanvas.LineFinalized += async delegate (InqLineModel model)
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
            var deactivate = _mode?.Deactivate();
            if (deactivate != null) await deactivate;
            _mode = mode;
            await _mode.Activate();
        }

        public async void SwitchMode(Options mode, bool isFixed)
        {
            SessionController.Instance.SessionView.HideRecorder();

            switch (mode)
            {
                case Options.SelectNode:
                    var nodeManipulationMode = new NodeManipulationMode(this);
                    await
                        SetViewMode(new MultiMode(this, nodeManipulationMode, new DuplicateNodeMode(this),
                            new PanZoomMode(this), new SelectMode(this), new TagNodeMode(this),
                            new FloatingMenuMode(this), new CreateGroupMode(this, nodeManipulationMode)));
                    break;
                case Options.SelectMarquee:
                    await SetViewMode(new MultiMode(this, new MultiSelectMode(this), new FloatingMenuMode(this)));
                    break;
                case Options.MainSearch:
                    SearchWindowView.SetFocus();
                    SessionController.Instance.SessionView.SearchView();
                    break;
                case Options.PenGlobalInk:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new LinkMode(this), new GestureMode(this)));
                    break;
                case Options.AddTextNode:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, ElementType.Text, isFixed),
                            new FloatingMenuMode(this)));
                    break;
                case Options.AddWeb:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, ElementType.Web, isFixed),
                            new FloatingMenuMode(this)));
                    break;
                case Options.AddAudioCapture:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, ElementType.Audio, isFixed),
                            new FloatingMenuMode(this)));
                    break;
                case Options.AddMedia:
                    await
                        SetViewMode(new MultiMode(this, new SelectMode(this),
                            new AddNodeMode(this, ElementType.Document, isFixed), new FloatingMenuMode(this)));
                    break;
                case Options.AddRecord:
                    await SetViewMode(new MultiMode(this, new SelectMode(this), new FloatingMenuMode(this), new DuplicateNodeMode(this),
                            new PanZoomMode(this), new SelectMode(this), new TagNodeMode(this)));
                    var sessionView = SessionController.Instance.SessionView;
                    sessionView.ShowRecorder();
                    break;
                case Options.PenErase:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    // TODO: delegate to workspaceview
                    //InqCanvas.SetErasing(true);
                    break;
                case Options.PenHighlight:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    // TODO: delegate to workspaceview
                    //                    InqCanvas.SetHighlighting(true);
                    break;
                case Options.Save:
                    SessionController.Instance.SaveWorkspace();
                    SessionController.Instance.SessionView.FloatingMenu.Reset();
                    break;
                case Options.Load:
                    SessionController.Instance.LoadWorkspace();
                    SessionController.Instance.SessionView.FloatingMenu.Reset();
                    break;
                case Options.MiscPin:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new PinMode(this)));
                    break;
                case Options.AddBucket:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this)));
                    break;
                case Options.AddVideo:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, ElementType.Video, isFixed),
                            new FloatingMenuMode(this)));
                    break;
            }
        }
    }
}