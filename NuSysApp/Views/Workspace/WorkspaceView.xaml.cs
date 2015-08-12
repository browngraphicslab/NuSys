using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using NuSysApp.Views.Workspace;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NuSysApp
{
    /// <summary>
    /// This is the view for the entire workspace. It instantiates the WorkspaceViewModel. 
    /// </summary>
    public sealed partial class WorkspaceView : Page
    {
        #region Private Members

        private int penSize = Constants.InitialPenSize;

        private bool _isZooming;
        private bool _isManipulationEnabled;
        private AbstractWorkspaceViewMode _mode;

        #endregion Private Members

        public WorkspaceView()
        {
            this.InitializeComponent();
            this.DataContext = new WorkspaceViewModel();
            _isZooming = false;
            var vm = (WorkspaceViewModel)this.DataContext;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new FloatingMenuMode(this)));
        }

        private void SetViewMode(AbstractWorkspaceViewMode mode)
        {
            _mode?.Deactivate();
            _mode = mode;
            _mode.Activate();
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

        private void OnModeChange(Options mode)
        {
            switch (mode)
            {
                case Options.Select:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new FloatingMenuMode(this)));
                    break;
                case Options.GlobalInk:
                    SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    InqCanvas.SetErasing(false);
                    break;
                case Options.AddTextNode:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new AddNodeMode(this, NodeType.Text),
                        new FloatingMenuMode(this)));
                    break;
                case Options.AddInkNode:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new AddNodeMode(this, NodeType.Ink), new FloatingMenuMode(this)));
                    break;
                case Options.Document:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new AddNodeMode(this, NodeType.Document), new FloatingMenuMode(this)));
                    break;
                case Options.Cortana:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                        new CortanaMode(this), new FloatingMenuMode(this)));
                    break;
                case Options.Erase:
                    InqCanvas.SetErasing(true);
                    break;
                case Options.Highlight:
                    InqCanvas.SetHighlighting(true);
                    break;
            }
        }
    }
}