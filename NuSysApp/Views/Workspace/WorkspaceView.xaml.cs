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
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NuSysApp
{
    /// <summary>
    /// This is the view for the entire workspace. It instantiates the WorkspaceViewModel. 
    /// </summary>
    public sealed partial class WorkspaceView : Page
    {
        #region Private Members

        private int _penSize = Constants.InitialPenSize;

        private AbstractWorkspaceViewMode _mode;

        private bool _cortanaInitialized;
        private CortanaMode _cortanaModeInstance;
       

        #endregion Private Members

        public WorkspaceView()
        {
            this.InitializeComponent();
            InqCanvasModel inqCanvasModel = new InqCanvasModel("WORKSPACE_ID");
            new InqCanvasViewModel(inqCanvas, inqCanvasModel);
            var vm = new WorkspaceViewModel(new WorkSpaceModel(inqCanvasModel));
            this.DataContext = vm;
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, Window.Current.Bounds.Width, Window.Current.Bounds.Height) };
            _cortanaInitialized = false;
            floatingMenu.WorkspaceView = this;
        }


        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new FloatingMenuMode(this)));
        }

        public async Task SetViewMode(AbstractWorkspaceViewMode mode, bool isFixed = false)
        {
            var deactivate = _mode?.Deactivate();
            if (deactivate != null) await deactivate;
            _mode = mode;
            await _mode.Activate();
        }

        public InqCanvasView InqCanvas
        {
            get { return inqCanvas; }
        }

        public FloatingMenuView FloatingMenu
        {
            get { return floatingMenu; }
        }

        public MultiSelectMenuView MultiMenu
        {
            get { return multiMenu; }
        }
        public Canvas MainCanvas
        {
            get { return mainCanvas; }
        }

        public Storyboard PinAnimationStoryboard
        {
            get { return canvasStoryboard; }
        }
        public DoubleAnimation ScaleXAnimation
        {
            get { return scaleXAnimation; }
        }
        public DoubleAnimation ScaleYAnimation
        {
            get { return scaleYAnimation; }
        }
        public DoubleAnimation TranslateXAnimation
        {
            get { return transXAnimation; }
        }
        public DoubleAnimation TranslateYAnimation
        {
            get { return transYAnimation; }
        }


        public void RemoveLoading()
        {
            //TODO remove a loading screen
        }

        public async void SwitchMode(Options mode, bool isFixed)
        {
            switch (mode)
            {
                case Options.SelectNode:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new FloatingMenuMode(this)));
                    break;
                case Options.SelectMarquee:
                    await SetViewMode(new MultiMode(this, new MultiSelectMode(this), new FloatingMenuMode(this)));
                    break;
                case Options.PenGlobalInk:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    InqCanvas.SetErasing(false);
                    break;
                case Options.AddTextNode:
                    await SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Text, isFixed), new FloatingMenuMode(this)));
                    break;
                case Options.AddAudioCapture:
                    await SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Audio, isFixed), new FloatingMenuMode(this)));
                    break;
                case Options.AddMedia:
                    await SetViewMode(new MultiMode(this, new SelectMode(this), new AddNodeMode(this, NodeType.Document, isFixed), new FloatingMenuMode(this)));
                    break;
                case Options.PenErase:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    InqCanvas.SetErasing(true);
                    break;
                case Options.PenHighlight:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    InqCanvas.SetHighlighting(true);
                    break;
                case Options.MiscSave:
                    var vm1 = (WorkspaceViewModel) this.DataContext;
                    vm1.SaveWorkspace();
                    break;
                case Options.MiscLoad:
                    var vm2 = (WorkspaceViewModel)this.DataContext;
                    await vm2.LoadWorkspace();
                    break;
                case Options.MiscPin:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new PinMode(this)));
                    break;
                case Options.AddBucket:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this)));
                    break;
                case Options.AddVideo:
                    await SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Video, isFixed), new FloatingMenuMode(this)));
                    break;
            }
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            string text = await e.Data.GetView().GetTextAsync();
            var pos = e.GetPosition(this);
            var vm = (WorkspaceViewModel)this.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);
            var props = new Dictionary<string, string>();
            props["width"] = "400";
            props["height"] = "300";
            await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), NodeType.Text.ToString(), text, null, props);
        }
    }
}