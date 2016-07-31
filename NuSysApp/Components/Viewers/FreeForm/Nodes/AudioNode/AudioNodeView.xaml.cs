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
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;
using NuSysApp.Components.Nodes;
using NuSysApp.Controller;
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioNodeView : AnimatableUserControl, IThumbnailable
    {
        private bool _loaded;
        


        public AudioNodeView(AudioNodeViewModel vm)
        {
            this.DataContext = vm; // has to be set before initComponent so child xaml elements inherit it
            InitializeComponent();
            _loaded = false;
            
            ((AudioNodeModel)vm.Model).OnJump += AudioNodeView_OnJump;

            //I'm sorry for the stupid name. I don't think it was me, but I'm too lazy to fix it.
            MediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.ScrubBar.ValueChanged += vm.ScrubBarOnValueChanged;

            MediaPlayer.AudioSource = vm.AudioSource;
            vm.OnRegionSeekPassing += MediaPlayer.onSeekedTo;
            //playbackElement.MediaEnded += MediaEnded;
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AudioNodeViewModel;
            vm.AudioDuration = MediaPlayer.MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            AudioNodeView_OnJump(new TimeSpan(0));
        }

        public void AudioNodeView_OnJump(TimeSpan time)
        {
            //(((DataContext as AudioNodeViewModel).Controller).ScrubJump(time);
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(MediaPlayer, width, height);
            return r;
        }

        private async void LoadPlaybackElement()
        {
            await MediaPlayer.RenderImageSource((DataContext as AudioNodeViewModel).VisualGrid);
            if (MediaPlayer.AudioSource == null && _loaded == false)
            {
                MediaPlayer.AudioSource = (DataContext as AudioNodeViewModel).AudioSource;
                _loaded = true;
            }
                        
        }
    }
}
