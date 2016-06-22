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

        public ImageRegionView(ImageRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.Selected();
            
            CompositeTransform composite = new CompositeTransform();
            this.RenderTransform = composite;
            OnSelected?.Invoke(this, true);
            DataContext = vm;
            vm.PropertyChanged += PropertyChanged;
            vm.SizeChanged += ChangeSize;
            var model = vm.Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var parentWidth = vm.ContainerViewModel.GetWidth();
            var parentHeight = vm.ContainerViewModel.GetHeight();
            composite.TranslateX = model.TopLeftPoint.X * parentWidth;
            composite.TranslateY = model.TopLeftPoint.Y * parentHeight;
            xMainRectangle.Width = (model.BottomRightPoint.X - model.TopLeftPoint.X)* parentWidth;
            xMainRectangle.Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y) *parentHeight;
        }

        private void ChangeSize(object sender, Point topLeft, Point bottomRight)
        {
            var composite = RenderTransform as CompositeTransform;
            if (composite == null)
            {
                return;
            }
            composite.TranslateX = topLeft.X;
            composite.TranslateY = topLeft.Y;
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

        private void xResizingTriangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

        }

        private void xResizingTriangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as ImageRegionViewModel;
            if (vm == null)
            {
                return;
            }

           
            xMainRectangle.Width = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 25);
            xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
            

            UpdateViewModel();

        }

        private void xResizingTriangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
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
            composite.TranslateX += e.Delta.Translation.X;
            composite.TranslateY += e.Delta.Translation.Y;

            UpdateViewModel();
            e.Handled = true;
        }

        private void UpdateViewModel()
        {
            var composite = RenderTransform as CompositeTransform;
            var vm = DataContext as ImageRegionViewModel;
            if (vm == null || composite == null)
            {
                return;
            }
            var topLeft = new Point(composite.TranslateX, composite.TranslateY);
            var bottomRight = new Point(topLeft.X + xMainRectangle.Width, topLeft.Y + xMainRectangle.Height);
            vm.SetNewPoints(topLeft, bottomRight);
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
            xResizingTriangle.Visibility = Visibility.Collapsed;


        }

        public void Selected()
        {
            xMainRectangle.StrokeThickness = 6;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.DarkBlue);
            //xResizingTriangle.Visibility = Visibility.Visible;

        }
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);

        }
    }
}