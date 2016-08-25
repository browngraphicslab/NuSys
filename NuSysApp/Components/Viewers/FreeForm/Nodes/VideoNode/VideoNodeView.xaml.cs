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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoNodeView : AnimatableUserControl, IThumbnailable
    {

        public VideoNodeView(VideoNodeViewModel vm)
        {
            this.DataContext = vm; // DataContext has to be set before init component so xaml elements have access to it
            this.InitializeComponent();
            if (!vm.Controller.LibraryElementController.ContentLoaded)
            {
                UITask.Run(async delegate
                {
                    await vm.Controller.LibraryElementController.LoadContentDataModelAsync();
                    LoadVideo(this);
                });
            }
            else
            {
                LoadVideo(this);
            }
            
            vm.Controller.Disposed += ControllerOnDisposed;
        }


        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (VideoNodeViewModel) DataContext;
            nodeTpl.Dispose();
            DataContext = null;
            if (vm.Controller != null)
            {
                vm.Controller.Disposed -= ControllerOnDisposed;
            }
        }

        private void LoadVideo(object sender)
        {
            var vm = DataContext as VideoNodeViewModel;
            VideoMediaPlayer.Source = new Uri(vm.Controller.LibraryElementController.Data);
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

