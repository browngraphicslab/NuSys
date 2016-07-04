using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoDetailHomeTabView : UserControl
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        private bool _addTimeBlockMode;
        private bool _loaded;
        private Line _temporaryLinkVisual;
        private List<LinkedTimeBlockViewModel> _timeBlocks;



        public VideoDetailHomeTabView(VideoDetailHomeTabViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;

            VideoMediaPlayer.Source = vm.Controller.GetSource();
            VideoMediaPlayer.MediaPlayer.MediaOpened += vm.VideoMediaPlayer_Loaded;

            _isRecording = false;
            //vm.LinkedTimeModels.CollectionChanged += LinkedTimeBlocks_CollectionChanged;
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            
            vm.Controller.Disposed += ControllerOnDisposed;
            vm.View = this;
            VideoMediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            VideoMediaPlayer.ScrubBar.ValueChanged += vm.ScrubBarOnValueChanged;
            //Loaded += delegate (object sender, RoutedEventArgs args)
            //{
            //    var sw = SessionController.Instance.SessionView.ActualWidth / 2;
            //    var sh = SessionController.Instance.SessionView.ActualHeight / 2;

            //    var ratio = playbackElement.ActualWidth > playbackElement.ActualHeight ? playbackElement.ActualWidth / sw : playbackElement.ActualHeight / sh;
            //    playbackElement.Width = playbackElement.ActualWidth / ratio;
            //    playbackElement.Height = playbackElement.ActualHeight / ratio;
            //};
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as VideoDetailHomeTabViewModel;
            vm.VideoDuration = VideoMediaPlayer.MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        public void Dispose()
        {
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (VideoNodeViewModel)DataContext;
            vm.Controller.Disposed -= ControllerOnDisposed;
        }

        public double VideoWidth => VideoMediaPlayer.MediaPlayer.ActualWidth;
        public double VideoHeight => VideoMediaPlayer.MediaPlayer.ActualHeight;
    }
}
