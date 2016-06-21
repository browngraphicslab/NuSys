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
using Windows.Foundation;
using System.ComponentModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PDFRegionView : UserControl
    { 
        public Point _topLeft;
        public Point _bottomRight;

        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;

        public PDFRegionView(PdfRegionViewModel regionVM)
        {
            
            this.InitializeComponent();
            this.DataContext = regionVM;
            this.Selected();
          
            regionVM.PropertyChanged += RegionVM_PropertyChanged;
            OnSelected?.Invoke(this, true);
            
            CompositeTransform composite = new CompositeTransform();
            this.RenderTransform = composite;
           
            OnSelected?.Invoke(this, true);
            
            regionVM.SizeChanged += ChangeSize;
            var model = regionVM.Model as PdfRegion;
            if (model == null)
            {
                return;
            }
            var parentWidth = regionVM.ContainerViewModel.GetWidth();
            var parentHeight = regionVM.ContainerViewModel.GetHeight();
            composite.TranslateX = model.TopLeftPoint.X * parentWidth;
            composite.TranslateY = model.TopLeftPoint.Y * parentHeight;
            xMainRectangle.Width = (model.BottomRightPoint.X - model.TopLeftPoint.X) * parentWidth;
            xMainRectangle.Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y) * parentHeight;

        }

        private void RegionVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = DataContext as PdfRegionViewModel;
            switch (e.PropertyName)
            {
                case "Width":
                    if (vm == null)
                    {
                        break;
                    }
                    xMainRectangle.Width = vm.Width;
                    break;

                case "Height":
                    if (vm == null)
                    {
                        break;
                    }
                    xMainRectangle.Height = vm.Height;
                    break;
            }
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
        private void UpdateViewModel()
        {
            var composite = RenderTransform as CompositeTransform;
            var vm = DataContext as PdfRegionViewModel;
            if (vm == null || composite == null)
            {
                return;
            }
            var topLeft = new Point(composite.TranslateX, composite.TranslateY);
            var bottomRight = new Point(topLeft.X + xMainRectangle.Width, topLeft.Y + xMainRectangle.Height);
            vm.SetNewPoints(topLeft, bottomRight);
        }

        private void XResizingRectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            xMainRectangle.Width += e.Delta.Translation.X;
            xMainRectangle.Height += e.Delta.Translation.Y;
            RectangleTranform.CenterX += e.Delta.Translation.X;
            RectangleTranform.CenterY += e.Delta.Translation.Y;
            xGrid.Width += e.Delta.Translation.X;
            xGrid.Height += e.Delta.Translation.Y;
            GridTranform.CenterX += e.Delta.Translation.X;
            GridTranform.CenterY += e.Delta.Translation.Y;
            ResizerTransform.TranslateX += e.Delta.Translation.X;
            ResizerTransform.TranslateY += e.Delta.Translation.Y;

            UpdateViewModel();

            //var vm = DataContext as ImageRegionViewModel;
            //if (vm == null)
            //{
            //    return;
            //}

            //xMainRectangle.Width = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 0);
            //xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 0);
            //ResizerTransform.TranslateX += e.Delta.Translation.X / 2;
            //ResizerTransform.TranslateY += e.Delta.Translation.Y / 2;
            //UpdateViewModel();
        }

        private void XResizingRectangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            ((CompositeTransform)this.RenderTransform).TranslateX += e.Delta.Translation.X;
            ((CompositeTransform)this.RenderTransform).TranslateY += e.Delta.Translation.Y;
            UpdateViewModel();
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
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.Aquamarine);
            xResizingTriangle.Visibility = Visibility.Collapsed;
        }

        public void Selected()
        {
            xMainRectangle.StrokeThickness = 6;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.CadetBlue);
            xResizingTriangle.Visibility = Visibility.Visible;

        }
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);
        }


        private void XMainRectangle_OnGotFocus(object sender, RoutedEventArgs e)
        {
            Selected();
        }

        private void XMainRectangle_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Deselected();
        }
    }
}
