using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NAudio.Wave;
using NuSysApp.Util;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : AnimatableUserControl, IThumbnailable
    {

        private ImageElementViewModel _vm;

        public ImageNodeView(ImageElementViewModel vm)
        {
            _vm = vm;           
            InitializeComponent();            
            DataContext = vm;

            Loaded += ViewLoaded;

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            xClippingWrapper.Controller = _vm.Controller.LibraryElementController;
            xClippingWrapper.ProcessLibraryElementController();
        }

        private void ControllerOnDisposed(object source, object args)
        {
            _vm.Controller.Disposed -= ControllerOnDisposed;
            nodeTpl.Dispose();
            DataContext = null;
        }
        

        

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(xImage, width, height);
            return r;
        }
    }
}
