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
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Diagnostics;

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
            InqCanvasModel inqCanvasModel = this.InqCanvas.ViewModel.Model;
            var vm = new WorkspaceViewModel(new WorkSpaceModel(inqCanvasModel));
            this.DataContext = vm;

            _cortanaInitialized = false;          
        }


        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new FloatingMenuMode(this)));
        }
        public async Task SetViewMode(AbstractWorkspaceViewMode mode)
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

        public Canvas MainCanvas
        {
            get { return mainCanvas; }
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
                    await SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Text), new FloatingMenuMode(this)));
                    break;
                case Options.AudioCapture:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new AddNodeMode(this, NodeType.Audio),
                        new FloatingMenuMode(this)));
                    break;
                case Options.PromoteInk:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this)));
                    break;
                case Options.AddInkNode:
                    await SetViewMode(new MultiMode(this, new SelectMode(this), new AddNodeMode(this, NodeType.Ink), new FloatingMenuMode(this)));
                    break;
                case Options.Document:
                    await SetViewMode(new MultiMode(this, new SelectMode(this), new AddNodeMode(this, NodeType.Document), new FloatingMenuMode(this)));
                    break;
                case Options.Erase:
                    InqCanvas.SetErasing(true);
                    break;
                case Options.Color:
                    InqCanvas.SetHighlighting(true);
                    break;
                case Options.Save:
                    var vm1 = (WorkspaceViewModel) this.DataContext;
                    vm1.SaveWorkspace();
                    break;
                case Options.Load:
                    var vm2 = (WorkspaceViewModel)this.DataContext;
                    await vm2.LoadWorkspace();
                    break;
                case Options.Pin:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new PinMode(this)));
                    break;
                case Options.Bucket:
                    await SetViewMode(new MultiMode(this));
                    break;
            }
        }
    }
}