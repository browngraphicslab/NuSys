using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SharpDX.Direct3D11;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoRegionView : UserControl
    {
        public VideoRegionView(VideoRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }

        private void Bound1_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var composite = IntervalRectangle.RenderTransform as CompositeTransform;
            var vm = DataContext as VideoRegionViewModel;
            if (composite != null && composite.TranslateX > 0  && vm != null)
            {
                vm.SetIntervalStart(composite.TranslateX);
            }
        }

        private void Bound2_OnManipulationDelta2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var composite = IntervalRectangle.RenderTransform as CompositeTransform;
            var vm = DataContext as VideoRegionViewModel;
            if (composite != null && composite.TranslateX > 0 &&  vm != null)
            {
                vm.SetIntervalEnd(composite.TranslateX + IntervalRectangle.Width);
            }
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }
        private void xResizingTriangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as VideoRegionViewModel;
            if (vm == null)
            {
                return;
            }
            vm.SetRegionSize(xMainRectangle.Width + e.Delta.Translation.X, xMainRectangle.Height + e.Delta.Translation.Y);
            e.Handled = true;
        }
        private void xResizingTriangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }

        private void xResizingTriangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as VideoRegionViewModel;
            if (vm == null || RectangleTranform == null)
            {
                return;
            }

            // check that manipulation delta remains in the bounds
            if (
                
                vm.TopLeft.X + e.Delta.Translation.X < 0 ||
                vm.TopLeft.X + e.Delta.Translation.X + vm.RectangleWidth >
                vm.ContainerViewModel.GetWidth() ||
                vm.TopLeft.Y + e.Delta.Translation.Y < 0 ||
                vm.TopLeft.Y + e.Delta.Translation.Y + vm.RectangleHeight >
                vm.ContainerViewModel.GetHeight())
            {
                return;
            }

            RectangleTranform.TranslateX += e.Delta.Translation.X;
            RectangleTranform.TranslateY += e.Delta.Translation.Y;
            

            vm.SetRegionLocation(new Point(RectangleTranform.TranslateX, RectangleTranform.TranslateY));
            e.Handled = true;
        }


        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            
        }
    }
}
