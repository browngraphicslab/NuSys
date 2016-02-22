using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoDetailView : UserControl
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;

        public VideoDetailView(VideoNodeViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;

            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            var byteArray = Convert.FromBase64String(SessionController.Instance.ContentController.Get((vm.Model as VideoNodeModel).ContentId).Data);
            memoryStream.AsStreamForWrite().Write(byteArray, 0, byteArray.Length);
            memoryStream.Seek(0);
            playbackElement.SetSource(memoryStream, "video/mp4");
            _isRecording = false;

            //Loaded += delegate (object sender, RoutedEventArgs args)
            //{
            //    var sw = SessionController.Instance.SessionView.ActualWidth / 2;
            //    var sh = SessionController.Instance.SessionView.ActualHeight / 2;

            //    var ratio = playbackElement.ActualWidth > playbackElement.ActualHeight ? playbackElement.ActualWidth / sw : playbackElement.ActualHeight / sh;
            //    playbackElement.Width = playbackElement.ActualWidth / ratio;
            //    playbackElement.Height = playbackElement.ActualHeight / ratio;


            //};
        }
    }
}
