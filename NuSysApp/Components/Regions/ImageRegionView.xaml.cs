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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageRegionView : UserControl
    {
        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;

        public ImageRegionView(ImageRegionViewModel viewModel)
        {
            this.InitializeComponent();

            this.Selected();
            this.RenderTransform = new CompositeTransform();
            xResizingRectangle.RenderTransform = new CompositeTransform();
            OnSelected?.Invoke(this, true);
            DataContext = viewModel;
            viewModel.PropertyChanged += PropertyChanged;
            xMainRectangle.Width = 50;
            xMainRectangle.Height = 50;
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Width": case "Height":
                    var vm = DataContext as ImageRegionViewModel;
                    if (vm == null)
                    {
                        break;
                    }
                    xMainRectangle.Width = vm.Width;
                    xMainRectangle.Height = vm.Height;
                    break;
            }
        }

        private void XResizingRectangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            
        }

        private void XResizingRectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as ImageRegionViewModel;
            if (vm == null)
            {
                return;
            }
            var oldHeight = xMainRectangle.Height;
            var oldWidth = xMainRectangle.Width;
              
            xMainRectangle.Width += e.Delta.Translation.X;
            xMainRectangle.Height += e.Delta.Translation.Y;
            
            vm.Resize(xMainRectangle.Width/oldWidth, xMainRectangle.Height/oldHeight);
        }

        private void XResizingRectangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

            OnSelected?.Invoke(this, true);
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var composite = RenderTransform as CompositeTransform;
            var vm = DataContext as ImageRegionViewModel;
            if (vm == null || composite == null)
            {
                return;
            }
            var oldX = composite.TranslateX;
            var oldY = composite.TranslateX;
            composite.TranslateX += e.Delta.Translation.X;
            composite.TranslateY += e.Delta.Translation.Y;
            
            vm.Translate(composite.TranslateX/oldX,composite.TranslateY/oldY);

            e.Handled = true;
        }
        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);

            e.Handled = true;

        }

        public void Deselected()
        {
            xMainRectangle.StrokeThickness = 3;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.Blue);
            xResizingRectangle.Visibility = Visibility.Collapsed;

        }

        public void Selected()
        {
            xMainRectangle.StrokeThickness = 6;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.DarkBlue);
            xResizingRectangle.Visibility = Visibility.Visible;

        }
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
           OnSelected?.Invoke(this, true);

        }
    }
}
