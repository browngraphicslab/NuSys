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

            if (composite != null &&  vm != null)
            {
                var newStart = composite.TranslateX + e.Delta.Translation.X;
                if (newStart < 0)
                {
                    newStart = 0;
                }
                if (newStart > vm.IntervalEnd)
                {
                    newStart = vm.IntervalEnd;
                }
                vm.SetIntervalStart(newStart);
            }
        }

        private void Bound2_OnManipulationDelta2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var composite = IntervalRectangle.RenderTransform as CompositeTransform;
            var vm = DataContext as VideoRegionViewModel;
            if (composite != null &&  vm != null)
            {
                var newEnd = composite.TranslateX + IntervalRectangle.Width + e.Delta.Translation.X;
                if (newEnd > vm.ContainerViewModel.GetWidth())
                {
                    newEnd = vm.ContainerViewModel.GetWidth();
                }
                if (newEnd < vm.IntervalStart)
                {
                    newEnd = vm.IntervalStart;
                }
                if (Double.IsNaN(newEnd))
                {
                    
                }
                vm.SetIntervalEnd(newEnd);
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
            if (vm == null || GridTransform == null)
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
            
            //GridTransform.TranslateX += e.Delta.Translation.X;
            //GridTransform.TranslateY += e.Delta.Translation.Y;
            

            vm.SetRegionLocation(new Point(GridTransform.TranslateX + e.Delta.Translation.X, GridTransform.TranslateY + e.Delta.Translation.Y));
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
