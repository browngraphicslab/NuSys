using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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

        private bool _isZooming;
        private bool _isManipulationEnabled;
        private AbstractWorkspaceViewMode _mode;

        public static bool CortanaRunning { get; set; }
        private readonly CortanaContinuousRecognition.CortanaMode _cortanaModeInstance;

        #endregion Private Members

        public WorkspaceView()
        {
            this.InitializeComponent();
            this.DataContext = new WorkspaceViewModel();
            _isZooming = false;
            var vm = (WorkspaceViewModel)this.DataContext;
            _cortanaModeInstance = new CortanaContinuousRecognition.CortanaMode(this);
            CortanaRunning = false;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new FloatingMenuMode(this)));
        }

        private async Task SetViewMode(AbstractWorkspaceViewMode mode)
        {
            var deactivate = _mode?.Deactivate();
            if (deactivate != null) await deactivate;
            _mode = mode;
            await _mode.Activate();
        }

        public InqCanvas InqCanvas
        {
            get { return inqCanvas; }
        }

        public FloatingMenu FloatingMenu
        {
            get { return floatingMenu; }
        }

        public bool IsManipulationEnabled
        {
            get
            {
                return _isManipulationEnabled;
            }

            set
            {
                mainFrame.ManipulationMode = value ? ManipulationModes.None : ManipulationModes.All;
                _isManipulationEnabled = value;
            }
        }

        private async void OnModeChange(Options mode)
        {
            switch (mode)
            {
                case Options.Select:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new FloatingMenuMode(this)));
                    break;
                case Options.GlobalInk:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    InqCanvas.SetErasing(false);
                    break;
                case Options.AddTextNode:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new AddNodeMode(this, NodeType.Text),
                        new FloatingMenuMode(this)));
                    break;
                case Options.PromoteInk:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new PromoteInkMode(this)));
                    break;
                case Options.AddInkNode:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new AddNodeMode(this, NodeType.Ink), new FloatingMenuMode(this)));
                    break;
                case Options.Document:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new AddNodeMode(this, NodeType.Document), new FloatingMenuMode(this)));
                    break;
                case Options.Cortana:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new FloatingMenuMode(this)));
                    // toggle continuous Cortana listening on and off
                    if (CortanaRunning)
                    {
                        _cortanaModeInstance.Deactivate();
                        CortanaRunning = false;
                    }
                    else
                    {
                        _cortanaModeInstance.Activate();
                        CortanaRunning = true;
                    }
                    break;
                case Options.AudioCapture:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new AddNodeMode(this, NodeType.Audio), new FloatingMenuMode(this)));
                    break;
                case Options.Erase:
                    InqCanvas.SetErasing(true);
                    break;
                case Options.Color:
                    InqCanvas.SetHighlighting(true);
                    break;
                case Options.Save:
                    await SetViewMode(new MultiMode(this, new SaveMode(this), new SelectMode(this)));
                    break;
            }
        }
    }
}