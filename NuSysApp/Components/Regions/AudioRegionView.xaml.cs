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
    public sealed partial class AudioRegionView : UserControl
    {
        private bool _toggleManipulation;
        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;
        public AudioRegionView(AudioRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _toggleManipulation = false;
            Rect.RenderTransform = new CompositeTransform();
            vm.UpdateVals();
            //TODO Make Legitimate
            Bound1.X1 = vm.LeftHandleX;
            Bound1.X2 = vm.LeftHandleX;
            Bound1.Y1 = vm.LefthandleY1;
            Bound1.Y2 = vm.LefthandleY2;

            Bound2.X1 = vm.RightHandleX - 20;
            Bound2.X2 = vm.RightHandleX - 20;
            Bound2.Y1 = vm.RightHandleY1;
            Bound2.Y2 = vm.RightHandleY2;
        }

        private void Handle_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _toggleManipulation = true;
        }

        private void Bound1_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            (Bound1.RenderTransform as CompositeTransform).TranslateX += e.Delta.Translation.X;
            (Rect.RenderTransform as CompositeTransform).TranslateX += e.Delta.Translation.X;
            Rect.Width -= e.Delta.Translation.X;
        }

        private void Handle_OnManipulationDelta2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            
            (Bound2.RenderTransform as CompositeTransform).TranslateX += e.Delta.Translation.X;
            Rect.Width += e.Delta.Translation.X;
            
        }

        private void Handle_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _toggleManipulation = false;
        }
    }
}
