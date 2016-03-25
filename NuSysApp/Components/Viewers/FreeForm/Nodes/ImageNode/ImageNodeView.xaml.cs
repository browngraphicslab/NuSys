﻿using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : AnimatableUserControl, IThumbnailable
    {
        public ImageNodeView(ImageElementViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                //nodeTpl.inkCanvas.ViewModel.CanvasSize = new Size(vm.Width, vm.Height);

                //vm.Init();
                //lets see if this 2 way binding works
                //nodeTpl.inkCanvas.ViewModel.CanvasSize = new Size(vm.Width, vm.Height);
                //nodeTpl.inkCanvas.Width = vm.Width;
                //nodeTpl.inkCanvas.Height = vm.Height;

            };
            //XamlRenderingBackgroundTask x = new RenderTask(this.xImage);

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (ImageElementViewModel) DataContext;
            vm.Controller.Disposed -= ControllerOnDisposed;
            nodeTpl.Dispose();
            DataContext = null;
        }

        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            //nodeTpl.ToggleInkMode();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.RequestDelete();
        }

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.RequestDuplicate(vm.Model.X, vm.Model.Y);
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(xImage, width, height);
            return r;
        }


    }
}
