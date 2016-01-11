using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class AudioDetailView : UserControl
    {
        private bool _stopped;

        public AudioDetailView(AudioNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            
        }

        private void Play_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (playbackElement.Source == null)
                playbackElement.SetSource((DataContext as AudioNodeViewModel).AudioSource, "audio/mp3");
            Play.Opacity = .3;
            playbackElement.MediaEnded += delegate (object o, RoutedEventArgs e2)
            {
                Play.Opacity = 1;
            };
            playbackElement.Play();
        }
        private void Pause_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            
        }
        private void Stop_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            playbackElement.Stop();
            Play.Opacity = 1;
            _stopped = true;
            e.Handled = true;
        }
    }
}
