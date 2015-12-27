using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfNodeView : AnimatableNodeView
    {
        public PdfNodeView(PdfNodeViewModel vm)
        {
            InitializeComponent();
            

            DataContextChanged += async delegate(FrameworkElement sender, DataContextChangedEventArgs args)
            {
                await vm.InitPdfViewer();
            };

            DataContext = vm;

            Loaded += async delegate(object sender, RoutedEventArgs e)
            {
            //    if (nodeTpl.inkCanvas != null) { 
            //        nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //        nodeTpl.inkCanvas.ReRenderLines();
            //    }

                var animX = new Storyboard();
                var animXAnim = new DoubleAnimation();
                animXAnim.Duration = TimeSpan.FromMilliseconds(300);
                animXAnim.EasingFunction = new ExponentialEase();
                animXAnim.From = 0.0;
                animXAnim.To = 1.0;
                animX.Children.Add(animXAnim);
                Storyboard.SetTarget(animX, this);
                Storyboard.SetTargetProperty(animX, "Opacity");
                animX.Begin();
            };

        }


        private void OnEditInk(object sender, RoutedEventArgs e)
        {
          //  nodeTpl.ToggleInkMode();

        }

        private void OnPageLeftClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.FlipLeft();

           // nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
//nodeTpl.inkCanvas.ReRenderLines();

        }

        private void OnPageRightClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.FlipRight();

         //   nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
       //     nodeTpl.inkCanvas.ReRenderLines();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }

  
    }


}
