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
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_SelectionChanged);
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
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (NodeViewModel) this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void Node_SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            /*
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
            }*/
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            return new RenderTargetBitmap();//TODO implement
        }
    }
}
