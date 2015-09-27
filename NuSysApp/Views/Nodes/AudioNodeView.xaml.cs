using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

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
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_SelectionChanged);
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

        private async void OnRecord_Click(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await ToggleRecording(FileName + ".mp3");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Text = "No microphone detected";
                await Task.Delay(500);
                MessageBox.Text = "";
                Debug.WriteLine("Record failed");
            }
        }

        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            if (_recording)
            {
                ToggleRecording(CurrentAudioFile.Name);
            }
            playbackElement.Stop();
            _stopped = true;
            play.Opacity = 1;
            e.Handled = true;
        }


        private void OnRewind_Click(object sender, TappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (_recording)
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
                };
                playbackElement.Play();
            }
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {
            playbackElement.Pause();
        //    pause.Opacity = .3;
        }

        private void OnFastforward_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnDelete_Click(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (NodeViewModel) this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void Node_SelectionChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName.Equals("IsSelected"))
            {
                var vm = (NodeViewModel)this.DataContext;

                if (vm.IsSelected)
                {
                    //var slideout = (Storyboard)Application.Current.Resources["slideout"];
                    slideout.Begin();
                }
                else
                {
                    //var slidein = (Storyboard)Application.Current.Resources["slidein"];
                    slidein.Begin();
                }
            }
        }
    }
}
