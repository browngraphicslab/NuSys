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
using System.Diagnostics;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PDFRegionView : UserControl
    { 
        public Point _topLeft;
        public Point _bottomRight;
        private double _tx;
        private double _ty;

        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;

        public PDFRegionView(PdfRegionViewModel regionVM)
        {
            
            this.InitializeComponent();
            this.DataContext = regionVM;
            this.Selected();

            regionVM.RegionChanged += RegionVM_RegionChanged;
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
            _tx = composite.TranslateX;
            _ty = composite.TranslateY;
            regionVM.Width = (model.BottomRightPoint.X - model.TopLeftPoint.X) * parentWidth;
            regionVM.Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y) * parentHeight;

        }

        private void RegionVM_RegionChanged(object sender, double height, double width)
        {
            return;
            var vm = (PdfRegionViewModel) DataContext;
            Debug.WriteLine("oooooooooooooo");
            vm.Width = width;
            vm.Height = height;
        }

        

        private void ChangeSize(object sender, Point topLeft, Point bottomRight)
        {
            return;
            var composite = RenderTransform as CompositeTransform;
            if (composite == null)
            {
                return;
            }
            composite.TranslateX = topLeft.X;
            composite.TranslateY = topLeft.Y;
           // _tx = composite.TranslateX;
           // _ty = composite.TranslateY;
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
            var bottomRight = new Point(topLeft.X + xMainRectangle.ActualWidth, topLeft.Y + xMainRectangle.ActualHeight);
            vm.SetNewPoints(topLeft, bottomRight);
        }

        private void XResizingRectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
//            xMainRectangle.Width += e.Delta.Translation.X;
 //           xMainRectangle.Height += e.Delta.Translation.Y;
            RectangleTranform.CenterX += e.Delta.Translation.X;
            RectangleTranform.CenterY += e.Delta.Translation.Y;
            xGrid.Width += e.Delta.Translation.X;
            xGrid.Height += e.Delta.Translation.Y;
            GridTranform.CenterX += e.Delta.Translation.X;
            GridTranform.CenterY += e.Delta.Translation.Y;
            
            
            //ResizerTransform.TranslateX += e.Delta.Translation.X;
            //ResizerTransform.TranslateY += e.Delta.Translation.Y;

            UpdateViewModel();

            
        }

        private void XResizingRectangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as PdfRegionViewModel;
            if (vm == null)
            {
                return;
            }

            var rt = ((CompositeTransform)this.RenderTransform);

            _tx += e.Delta.Translation.X;
            _ty += e.Delta.Translation.Y;

            if (_tx < 0)
            {
                Debug.WriteLine(vm.OriginalWidth);
                Debug.WriteLine(_tx);
                Debug.WriteLine("-------");

                vm.Width = vm.OriginalWidth + _tx;
                rt.TranslateX = 0;
            }
            else
            {
                rt.TranslateX = _tx;
                vm.Width = vm.OriginalWidth;
            }
            
            
      
              //  ((CompositeTransform)this.RenderTransform).TranslateY += e.Delta.Translation.Y;
            
                
            UpdateViewModel();
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var vm = DataContext as PdfRegionViewModel;
            if (vm == null)
            {
                return;
            }
            var tx = ((CompositeTransform) this.RenderTransform).TranslateX;
            var ty = ((CompositeTransform) this.RenderTransform).TranslateY;
            if (tx < 0 || tx + vm.Width > vm.ContainerWidth)
                return;
            if (ty < 0 || ty + vm.Height > vm.ContainerHeight)
                return;


            vm.OriginalHeight = vm.Height;
            vm.OriginalWidth = vm.Width;
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
