using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoNodeView : AnimatableUserControl, IThumbnailable
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        private VideoNodeViewModel _vm;
        private List<LinkedTimeBlockViewModel> _timeBlocks;

        public VideoNodeView(VideoNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            if (SessionController.Instance.ContentController.ContainsAndLoaded(vm.Model.ContentId))
            {
                LoadVideo();
            }
            else
            {
                vm.Controller.ContentLoaded += LoadVideo;
            }


            _isRecording = false;
            vm.LinkedTimeModels.CollectionChanged += LinkedTimeBlocks_CollectionChanged;
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            scrubBar.SetValue(Canvas.ZIndexProperty, 1);
            //  playbackElement.Play();
        }

        private void LoadVideo(object sender = null, object data = null)
        {
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            var byteArray = Convert.FromBase64String(SessionController.Instance.ContentController.Get((((VideoNodeViewModel)DataContext).Model as VideoNodeModel).ContentId).Data);
            memoryStream.AsStreamForWrite().Write(byteArray, 0, byteArray.Length);
            memoryStream.Seek(0);
            playbackElement.SetSource(memoryStream, "video/mp4");
            ((VideoNodeViewModel)DataContext).Controller.ContentLoaded -= LoadVideo;
        }

        private void LinkedTimeBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var timeBlockVM = new LinkedTimeBlockViewModel((DataContext as VideoNodeViewModel).LinkedTimeModels.Last(), playbackElement.NaturalDuration.TimeSpan, scrubBar);
            LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
            Grid.SetRow(line, 1);
            _timeBlocks.Add(timeBlockVM);
            grid.Children.Add(line);
        }
        private void SrubBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            double ratio = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
            double millliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            playbackElement.Position = time;
        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                double ratio = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                double seconds = playbackElement.NaturalDuration.TimeSpan.TotalSeconds * ratio;

                TimeSpan time = new TimeSpan(0, 0, (int)seconds);
                playbackElement.Position = time;
            }
            e.Handled = true;
        }



        private void ScrubBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var element in _timeBlocks)
            {
                element.ResizeLine1();
            }
        }


        private async void OnRecord_Click(object sender, TappedRoutedEventArgs e)
        {
            var vm = (VideoNodeViewModel)this.DataContext;
            var model = (VideoNodeModel)vm.Model;

            if (!_isRecording)
            {
                model.Recording = new InMemoryRandomAccessStream();
                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
                await _mediaCapture.StartRecordToStreamAsync(encodingProfile, model.Recording);
                playbackElement.Visibility = Visibility.Collapsed;
                preview.Visibility = Visibility.Visible;

            }
            else
            {
                await _mediaCapture.StopRecordAsync();
                playbackElement.SetSource(model.Recording, "video/mp4");
                playbackElement.Visibility = Visibility.Visible;
                preview.Visibility = Visibility.Collapsed;
                playbackElement.Play();
            }


            _isRecording = !_isRecording;
        }


        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            /*     if (_recording)
                 {
                     ToggleRecording(CurrentAudioFile.Name);
                 }*/
            playbackElement.Stop();
            //       _stopped = true;
            e.Handled = true;
        }


        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {
            /*   if (_recording)
               {
                  ToggleRecording(CurrentAudioFile.Name);
               }
               else
               {
                  // pause.Opacity = 1;
                   play.Opacity = .3;
                   if (_stopped)
                   {
                       _stopped = false;
                       if (CurrentAudioFile == null) return;
                       var stream = await CurrentAudioFile.OpenAsync(FileAccessMode.Read);
                       playbackElement.SetSource(stream, CurrentAudioFile.FileType);
                   }MediaType.Video
                   playbackElement.MediaEnded += delegate(object o, RoutedEventArgs e2)
                   {
                       play.Opacity = 1;
                   };*/
            playbackElement.Play();
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {

            playbackElement.Pause();
            //    pause.Opacity = .3;
        }
        /*private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (ElementViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }*/
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)this.DataContext;
            //vm.Remove();
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();//TODO implement

            return r;
        }




        private void PlaybackElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = (VideoNodeViewModel)this.DataContext;
            var model = (VideoNodeModel)vm.Model;
            model.ResolutionX = playbackElement.AspectRatioWidth;
            model.ResolutionY = playbackElement.AspectRatioHeight;

            double width = this.Width;
            double height = this.Height;
            vm.SetSize(width, height);

        }
        public int AspectHeight { get { return playbackElement.AspectRatioHeight; } }
        public int AspectWidth { get { return playbackElement.AspectRatioWidth; } }

    }
}

