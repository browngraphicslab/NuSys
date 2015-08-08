using System;
using System.Collections.Generic;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using Windows.UI.Popups;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
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
            Debug.WriteLine("dione initializing");
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

        public bool IsManipulationEnabled
        {
            get
            {
                return _isManipulationEnabled;
            }

            set
            {
                if (value)
                {
                    mainFrame.ManipulationMode = ManipulationModes.None;
                }
                else
                {
                    mainFrame.ManipulationMode = ManipulationModes.All;
                }
                _isManipulationEnabled = value;                
            }
        }

        #region Floating Menu Handlers
        private void FloatingMenu_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            var compositeTransform = vm.FMTransform;

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            /*
            vm.FMTransform = compositeTransform;
            if (compositeTransform.TranslateX < -85 || compositeTransform.TranslateX > this.ActualWidth || compositeTransform.TranslateY < -85 + FM.Children.Count * -100 || compositeTransform.TranslateY > this.ActualHeight)
            {
              //  FM.Visibility = Visibility.Collapsed;
                e.Complete();
            }
            */
            e.Handled = true;
        }

        #endregion Floating Menu Handlers

        private void OnModeChange(Options mode)
        {
            switch (mode)
            {
                case Options.SELECT:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new FloatingMenuMode(this)));
                    break;
                case Options.GLOBAL_INK:
                    SetViewMode(new MultiMode(this, new GlobalInkMode(this),  new FloatingMenuMode(this)));
                    break;
                case Options.ADD_TEXT_NODE:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new AddNodeMode(this, NodeType.TEXT), new FloatingMenuMode(this)));
                    break;
                case Options.ADD_INK_NODE:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new AddNodeMode(this, NodeType.INK), new FloatingMenuMode(this)));
                    break;
                case Options.DOCUMENT:
                    SetViewMode(new MultiMode(this, new PanZoomMode(this), new SelectMode(this), new AddNodeMode(this, NodeType.DOCUMENT), new FloatingMenuMode(this)));
                    break;
            }
        }
    }
}