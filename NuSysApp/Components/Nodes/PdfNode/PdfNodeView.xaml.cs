﻿using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfNodeView : AnimatableUserControl
    {
        public PdfNodeView(PdfNodeViewModel vm)
        {
            InitializeComponent();
          //  IsDoubleTapEnabled = true;

            Loaded += async delegate
            {
                await vm.InitPdfViewer();
            };

            DataContext = vm;
            
        }


        private void OnEditInk(object sender, RoutedEventArgs e)
        {
          //  nodeTpl.ToggleInkMode();

        }

        private async void OnPageLeftClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            await vm.FlipLeft();
            (nodeTpl.inkCanvas.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            e.Handled = true;

            // nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
//nodeTpl.inkCanvas.ReRenderLines();

        }

        private async void OnPageRightClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            await vm.FlipRight();
            (nodeTpl.inkCanvas.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            e.Handled = true;

            //   nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //     nodeTpl.inkCanvas.ReRenderLines();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }


        private void PageRight_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }


}
