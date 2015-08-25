using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using NuSysApp.Views.Workspace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;

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

        private bool _cortanaInitialized;
        private CortanaMode _cortanaModeInstance;

        #endregion Private Members

        public WorkspaceView()
        {
            this.InitializeComponent();
            this.DataContext = new WorkspaceViewModel(new WorkSpaceModel());
            _isZooming = false;
            var vm = (WorkspaceViewModel)this.DataContext;
            _cortanaInitialized = false;
            vm.PropertyChanged += Update;
        }

        private void Update(object sender, PropertyChangedEventArgs e)
        {
            WorkspaceViewModel vm = (WorkspaceViewModel)sender;
            switch (e.PropertyName)
            {
                case "PartialLineAdded":
                    Line l = vm.LastPartialLines;
                    InqLine inq = new InqLine();
                    inq.StrokeThickness = 7;
                    inq.AddPoint(new Point(l.X1, l.Y1));
                    inq.AddPoint(new Point((l.X1 + l.X2) / 2, (l.Y1 + l.Y2) / 2);
                    inq.AddPoint(new Point(l.X2, l.Y2));
                    InqLine[] inqs = new InqLine[1];
                    inqs[0] = inq;
                    this.InqCanvas.PasteStrokes(inqs);
                    break;
            }
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

        public void RemoveLoading()
        {
            //TODO remove a loading screen
        }
        private async void OnModeChange(Options mode)
        {
            switch (mode)
            {
                case Options.Select:
                    await SetViewMode(new MultiMode(this, new PromoteInkMode(this), new PanZoomMode(this), new SelectMode(this),
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
                    SetViewMode(new MultiMode(this, new PanZoomMode(this)));
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
                    if (!_cortanaInitialized)
                    {
                        _cortanaModeInstance = new CortanaMode(this);
                        _cortanaInitialized = true;
                    }
                    if (!_cortanaModeInstance.IsRunning)
                    {
                        await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                            _cortanaModeInstance, new FloatingMenuMode(this)));
                    }
                    else
                    {
                        await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this),
                            new FloatingMenuMode(this)));
                    }
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
                case Options.Load:
                    await SetViewMode(new MultiMode(this, new LoadMode(this), new SelectMode(this)));
                    break;
                case Options.Pin:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new PinMode(this)));
                    break;

            }
        }
    }
}