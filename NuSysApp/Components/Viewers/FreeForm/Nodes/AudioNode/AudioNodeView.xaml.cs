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
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioNodeView : AnimatableUserControl, IThumbnailable
    {
        private bool  _stopped; 

        public AudioNodeView(AudioNodeViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
   
            _stopped = true;
        }

        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            playbackElement.Stop();
            _stopped = true;
            play.Opacity = 1;
            e.Handled = true;
        }
        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (playbackElement.Source == null)
                playbackElement.SetSource((DataContext as AudioNodeViewModel).AudioSource, "audio/mp3");
            play.Opacity = .3;
            playbackElement.MediaEnded += delegate(object o, RoutedEventArgs e2)
            {
                play.Opacity = 1;
            };
            playbackElement.Play();
        }



        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            return new RenderTargetBitmap();//TODO implement
        }
    }
}
