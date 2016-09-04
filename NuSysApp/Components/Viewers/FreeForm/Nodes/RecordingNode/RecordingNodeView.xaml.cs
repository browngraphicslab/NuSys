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
    public sealed partial class RecordingNodeView : AnimatableUserControl, IDisposable
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
            Unloaded += OnUnloaded;
            xMediaRecorder.RecordingStopped += XMediaRecorderOnRecordingStopped;
            xRootBorder.ManipulationStarting += XRootBorderOnManipulationStarting;
            xRootBorder.ManipulationDelta += XRootBorderOnManipulationDelta;
            xRootBorder.ManipulationCompleted += XRootBorderOnManipulationCompleted;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            SessionController.Instance.SessionView.FreeFormViewer.Unfreeze();
            Dispose();
        }

        private void XRootBorderOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var workspaceTransform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            _vm.X += e.Delta.Translation.X / workspaceTransform.ScaleX;
            _vm.Y += e.Delta.Translation.Y / workspaceTransform.ScaleY;
        }

        private void XRootBorderOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            SessionController.Instance.SessionView.FreeFormViewer.Unfreeze();
        }

        private void XRootBorderOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            SessionController.Instance.SessionView.FreeFormViewer.Freeze();
        }

        public void Dispose()
        {
            _vm = null;
            xMediaRecorder.RecordingStopped -= XMediaRecorderOnRecordingStopped;
            xMediaRecorder.RecordingStopped -= XMediaRecorderOnRecordingStopped;
            xRootBorder.ManipulationStarting -= XRootBorderOnManipulationStarting;
            xRootBorder.ManipulationDelta -= XRootBorderOnManipulationDelta;
            xRootBorder.ManipulationCompleted -= XRootBorderOnManipulationCompleted;
        }

        private void XMediaRecorderOnRecordingStopped(object source)
        {
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
        }

    
        private void btnDelete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
        }
    }
}