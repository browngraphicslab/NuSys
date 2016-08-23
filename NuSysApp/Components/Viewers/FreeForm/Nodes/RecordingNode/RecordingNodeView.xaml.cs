using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class RecordingNodeView : AnimatableUserControl
    {
        /// <summary>
        /// The view model of the recording node, so we don't have to check the data context every time
        /// </summary>
        private RecordingNodeViewModel _vm;

        public RecordingNodeView(RecordingNodeViewModel vm)
        {
            _vm = vm;
            DataContext = _vm;
            InitializeComponent();
            xMediaRecorder.RecordingStopped += delegate(object source)
            {
                SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
            };

        }

        /// <summary>
        /// Updates the view model's X and Y coordinates when user attempts to move the recording node on the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XRootBorder_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var workspaceTransform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            _vm.X += e.Delta.Translation.X / workspaceTransform.ScaleX;
            _vm.Y += e.Delta.Translation.Y / workspaceTransform.ScaleY;
            e.Handled = true;
        }

        private void btnDelete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
        }
    }
}