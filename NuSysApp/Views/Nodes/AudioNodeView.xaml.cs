using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
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
    public sealed partial class AudioNodeView : UserControl
    {
        private readonly AudioNodeViewModel _anvm; 
        private bool _recording, _stopped; 

        public AudioNodeView(AudioNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _anvm = vm;
            _recording = false;
   
            _stopped = true;
            CurrentAudioFile = null;
        }
        private string FileName => _anvm.FileName;

        private StorageFile CurrentAudioFile
        {
            get { return _anvm.CurrentAudioFile; }
            set { _anvm.CurrentAudioFile = value; }
        }

        private async Task ToggleRecording(string fileName)
        {
            if (!_recording)
            {              
                await _anvm.AudioRecorder.InitializeAudioRecording();
                CurrentAudioFile = await _anvm.AudioRecorder.CaptureAudio(fileName);
                _recording = true;
                record.Opacity = .3;
            }
            else
            {
                await _anvm.AudioRecorder.StopCapture();
                _recording = false;
                record.Opacity = 1;
            }
        }

        private async void OnRecord_Click(object sender, RoutedEventArgs e)
        {
            await ToggleRecording(FileName + ".mp3");
        }

        private void OnStop_Click(object sender, RoutedEventArgs e)
        {
            playbackElement.Stop();
            _stopped = true;
            play.Opacity = 1;
        }


        private void OnRewind_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {
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
            };
            playbackElement.Play();
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {
            playbackElement.Pause();
        }

        private void OnFastforward_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }
    }
}
