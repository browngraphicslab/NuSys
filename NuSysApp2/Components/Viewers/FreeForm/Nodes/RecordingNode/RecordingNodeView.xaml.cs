using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class RecordingNodeView : AnimatableUserControl, IThumbnailable
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        private bool _isopen;

        public RecordingNodeView(RecordingNodeViewModel vm)
        {
            DataContext = vm;
            vm.Controller.SetSize(vm.Width, vm.Height);
            InitializeComponent();
            xMediaRecorder.RecordingStopped += delegate(object source)
            {
                SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
            };

        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            return new RenderTargetBitmap();
        }
    }
}