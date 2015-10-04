using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class VideoNodeView : UserControl
    {
        public VideoNodeView(VideoNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            memoryStream.AsStreamForWrite().Write((vm.Model as VideoNodeModel).ByteArray, 0, (vm.Model as VideoNodeModel).ByteArray.Length);
            memoryStream.Seek(0);
            playbackElement.SetSource(memoryStream, "video/mp4");
            playbackElement.Play();
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
        private void OnRewind_Click(object sender, TappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
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
                }
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
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (NodeViewModel) this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }
    }
}
