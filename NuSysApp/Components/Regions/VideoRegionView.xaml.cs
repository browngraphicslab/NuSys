using System;
using System.Collections.Generic;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoRegionView : UserControl
    {
        private bool _toggleManipulation;
        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;
        public VideoRegionView(VideoRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _toggleManipulation = false;
        }

        private void Handle_OnPointerPressed(object sender, PointerRoutedEventArgs e)
       {
            _toggleManipulation = true;
        }

        private void Bound1_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Rect.Width - e.Delta.Translation.X > 0)
            {
                (Bound1.RenderTransform as CompositeTransform).TranslateX += e.Delta.Translation.X;
                (Rect.RenderTransform as CompositeTransform).TranslateX += e.Delta.Translation.X;
                Rect.Width -= e.Delta.Translation.X;
            }
        }

        private void Handle_OnManipulationDelta2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Rect.Width + e.Delta.Translation.X > 0)
            {
                (Bound2.RenderTransform as CompositeTransform).TranslateX += e.Delta.Translation.X;
                Rect.Width += e.Delta.Translation.X;
            }
        }

        private void Handle_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _toggleManipulation = false;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var vm = this.DataContext as AudioRegionViewModel;
            var model = vm.Model as TimeRegionModel;
            model.Start = Bound1.X1 / vm.RegionWidth;
            model.End = Bound2.X1 / vm.RegionWidth;
           
        }
    }
}
