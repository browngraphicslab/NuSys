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
    public sealed partial class LibraryElementView : AnimatableUserControl, IThumbnailable
    {
        private LibraryBucketViewModel _libVm;

        public LibraryElementView(LibraryElementViewModel vm)
        {
            InitializeComponent();
            _libVm = new LibraryBucketViewModel(vm.Width, vm.Height);
            xLibContainer.Children.Add(new LibraryView(_libVm, new LibraryElementPropertiesWindow()));
            vm.Controller.SizeChanged += delegate(object source, double width, double height)
            {
                _libVm.Width = width;
                _libVm.Height = height;
            };
            DataContext = vm;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {

                //LibraryView.Reload();
                //nodeTpl.inkCanvas.ViewModel.CanvasSize = new Size(vm.Width, vm.Height);

                //vm.Init();
                //lets see if this 2 way binding works
                //nodeTpl.inkCanvas.ViewModel.CanvasSize = new Size(vm.Width, vm.Height);
                //nodeTpl.inkCanvas.Width = vm.Width;
                //nodeTpl.inkCanvas.Height = vm.Height;
                

            };
            //XamlRenderingBackgroundTask x = new RenderTask(this.xImage);
        }

        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            //nodeTpl.ToggleInkMode();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.Delete();
        }

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.Duplicate(vm.Model.X, vm.Model.Y);
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();

            return r;
        }


    }
}
