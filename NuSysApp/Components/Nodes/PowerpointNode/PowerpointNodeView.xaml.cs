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
    public sealed partial class PowerpointNodeView : AnimatableUserControl, IThumbnailable
    {
        public PowerpointNodeView(PowerpointNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                vm.Init();
                //nodeTpl.inkCanvas.ViewModel.CanvasSize = new Size(vm.Width, vm.Height);
            };
        }

        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)DataContext;
            vm.Remove();
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(xImage, width, height);
            return r;
        }
    }
}
