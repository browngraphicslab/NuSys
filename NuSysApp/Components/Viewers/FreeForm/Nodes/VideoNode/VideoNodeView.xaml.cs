using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoNodeView : AnimatableUserControl, IThumbnailable
    {

        public VideoNodeView(VideoNodeViewModel vm)
        {
            this.DataContext = vm; // DataContext has to be set before init component so xaml elements have access to it
            this.InitializeComponent();
            if (SessionController.Instance.ContentController.ContainsAndLoaded(vm.Model.LibraryId))
            {
                LoadVideo(this);
            }
            else
            {
                vm.Controller.LibraryElementController.Loaded += LoadVideo;
            }
            
            vm.Controller.Disposed += ControllerOnDisposed;

            VideoMediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            VideoMediaPlayer.ScrubBar.ValueChanged += vm.ScrubBarOnValueChanged;
            vm.OnRegionSeekPassing += VideoMediaPlayer.onSeekedTo;
        }

        private async void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as VideoNodeViewModel;
            vm.VideoDuration = VideoMediaPlayer.MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (VideoNodeViewModel) DataContext;
            vm.Controller.LibraryElementController.Loaded -= LoadVideo;
            vm.Controller.Disposed -= ControllerOnDisposed;
            nodeTpl.Dispose();
            DataContext = null;
        }

        private void LoadVideo(object sender)
        {
            var vm = DataContext as VideoNodeViewModel;
            VideoMediaPlayer.Source = vm.Controller.LibraryElementController.GetSource();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.RequestDelete();
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(grid, width, height);
            return r;
        }
    }
}

