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

        //useless for now
        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;

        public bool Selected {private set; get; }



        public PDFRegionView(PdfRegionViewModel regionVM)
        {
            
            this.InitializeComponent();
            this.DataContext = regionVM;
            this.Deselect();

            //regionVM.RegionChanged += RegionVM_RegionChanged;
            OnSelected?.Invoke(this, true);
            
            CompositeTransform composite = new CompositeTransform();
            this.RenderTransform = composite;
           
            OnSelected?.Invoke(this, true);
            //regionVM.PropertyChanged += PropertyChanged;
            regionVM.SizeChanged += ChangeSize;
            regionVM.LocationChanged += ChangeLocation;
            var model = regionVM.Model as PdfRegion;
            if (model == null)
            {
                return;
            }
            var parentWidth = regionVM.ContainerViewModel.GetWidth();
            var parentHeight = regionVM.ContainerViewModel.GetHeight();
            composite.TranslateX = model.TopLeftPoint.X * parentWidth;
            composite.TranslateY = model.TopLeftPoint.Y * parentHeight;
      

            //If in detail view, adjust to the right to account for difference between view and actual image.
            if (regionVM.ContainerViewModel is PdfDetailHomeTabViewModel)
            {
                var pvm = regionVM.ContainerViewModel as PdfDetailHomeTabViewModel;
                var diffWidth = pvm.GetViewWidth() - parentWidth;
                var diffHeight = pvm.GetViewHeight() - parentHeight;
                composite.TranslateX += diffWidth / 2;
                composite.TranslateY += diffHeight / 2;
            }


            _tx = composite.TranslateX;
            _ty = composite.TranslateY;


            regionVM.Width = (model.Width) * parentWidth;
            regionVM.Height = (model.Height) * parentHeight;
        }


        private void ChangeLocation(object sender, Point topLeft)
        {

            var vm = DataContext as PdfRegionViewModel;

            var composite = RenderTransform as CompositeTransform;
            if (composite == null)
            {
                return;
            }
            composite.TranslateX = topLeft.X;
            composite.TranslateY = topLeft.Y;

            //If in detail view, adjust to the right to account for difference between view and actual image.
            if (vm.ContainerViewModel is PdfDetailHomeTabViewModel)
            {
                var pvm = vm.ContainerViewModel as PdfDetailHomeTabViewModel;
                var diffWidth = pvm.GetViewWidth() - pvm.GetWidth();
                var diffHeight = pvm.GetViewHeight() - pvm.GetHeight();
                composite.TranslateX += diffWidth / 2;
                composite.TranslateY += diffHeight / 2;
            }
        }

        private void ChangeSize(object sender, double width, double height)
        {
            var vm = DataContext as PdfRegionViewModel;

            var composite = RenderTransform as CompositeTransform;
            if (composite == null)
            {
                return;
            }
            xMainRectangle.Width = width;
            xMainRectangle.Height = height;
            vm.Width = width;
            vm.Height = height;
        }

        private void RegionVM_RegionChanged(object sender, double height, double width)
        {
            var vm = (PdfRegionViewModel) DataContext;
            vm.Width = width;
            vm.Height = height;

            // TODO Refactor to Controller
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

            //TODO For shrinking behavior, this needs to be changed
           
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

        private void XResizingTriangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            var vm = DataContext as PdfRegionViewModel;
            if (vm == null)
            {
                return;
            }
            var rt = ((CompositeTransform)this.RenderTransform);
            if (rt == null)
            {
                return;
            }

            var ivm = vm.ContainerViewModel as PdfDetailHomeTabViewModel;
            var diffWidth = ivm.GetViewWidth() - ivm.GetWidth();
            var diffHeight = ivm.GetViewHeight() - ivm.GetHeight();

            var leftXBound = diffWidth / 2;
            var rightXBound = diffHeight / 2 + ivm.GetWidth();

            var upYBound = diffHeight / 2;
            var downYBound = diffHeight / 2 + ivm.GetHeight();

            if (xMainRectangle.Width + rt.TranslateX + e.Delta.Translation.X - diffWidth/2 <= rightXBound)
            {
                xMainRectangle.Width = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 25);
                vm.Width = xMainRectangle.Width;


            }

            if (xMainRectangle.Height + rt.TranslateY + e.Delta.Translation.Y - diffHeight/2 <= downYBound)
            {
                xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
                vm.Height = xMainRectangle.Height;

            }
            /*

            if (xMainRectangle.Width >= vm.ContainerWidth - rt.TranslateX && xMainRectangle.Height >= vm.ContainerHeight - rt.TranslateY)
            {
                return;
            } else if (xMainRectangle.Width >= vm.ContainerWidth - rt.TranslateX && xMainRectangle.Height < vm.ContainerHeight - rt.TranslateY)
            {
                xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
                vm.Height = xMainRectangle.Height;

            } else if (xMainRectangle.Width < vm.ContainerWidth - rt.TranslateX &&
                       xMainRectangle.Height >= vm.ContainerHeight - rt.TranslateY)
            {
                xMainRectangle.Width = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 25);
                vm.Width = xMainRectangle.Width;
            }
            else
            {
                xMainRectangle.Width = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 25);
                xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
                vm.Width = xMainRectangle.Width;
                vm.Height = xMainRectangle.Height;
            }
            */

            vm.SetNewSize(xMainRectangle.Width, xMainRectangle.Height);
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Width": case "Height":
                    var vm = DataContext as PdfRegionViewModel;
                    if (vm == null)
                    {
                        break;
                    }
                    xMainRectangle.Width = vm.Width;
                    xMainRectangle.Height = vm.Height;
                    break;
            }
        }

        private void XResizingTriangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            this.Select();
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
            if (rt == null)
            {
                return;
            }
            var ivm = vm.ContainerViewModel as PdfDetailHomeTabViewModel;
            var diffWidth = ivm.GetViewWidth() - ivm.GetWidth();
            var diffHeight = ivm.GetViewHeight() - ivm.GetHeight();

            var leftXBound = diffWidth / 2;
            var rightXBound = diffWidth / 2 + ivm.GetWidth() - vm.Width;


            var upYBound = diffHeight / 2;
            var downYBound = diffHeight / 2 + ivm.GetHeight() - vm.Height;

            _tx += e.Delta.Translation.X;
            _ty += e.Delta.Translation.Y;

            //Translating X
            if (_tx < leftXBound)
            {
                rt.TranslateX = leftXBound;
            } else if (_tx > rightXBound)
            {
                rt.TranslateX = rightXBound;
            }
            else
            {
                rt.TranslateX = _tx;
            }

            //Translating Y
            if (_ty < upYBound)
            {
                rt.TranslateY = upYBound;
            }
            else if (_ty > downYBound)
            {
                rt.TranslateY = downYBound;
            }
            else
            {
                rt.TranslateY = _ty;
            }

            var composite = RenderTransform as CompositeTransform;
            var topLeft = new Point(composite.TranslateX - leftXBound, composite.TranslateY - upYBound);
            vm.SetNewLocation(topLeft);
            e.Handled = true; 
        }

        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var vm = DataContext as PdfRegionViewModel;
            if (vm == null)
            {
                return;
            }
            _tx = ((CompositeTransform) this.RenderTransform).TranslateX;
            _ty = ((CompositeTransform) this.RenderTransform).TranslateY;


            vm.OriginalHeight = vm.Height;
            vm.OriginalWidth = vm.Width;
            this.Select();
            e.Handled = true;
        }

        public void Deselect()
        {
            xMainRectangle.StrokeThickness = 3;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.CadetBlue);
            xResizingTriangle.Visibility = Visibility.Collapsed;
            xDelete.Visibility = Visibility.Collapsed;
            xNameTextBox.Visibility = Visibility.Collapsed;
            Selected = false;

        }

        public void Select()
        {
            xMainRectangle.StrokeThickness = 6;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.CadetBlue);
            xResizingTriangle.Visibility = Visibility.Visible;
            xDelete.Visibility = Visibility.Visible;
            xNameTextBox.Visibility = Visibility.Visible;
            Selected = true;


        }
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            
                var vm = DataContext as PdfRegionViewModel;

                if (!vm.Editable)
                    return;

                if (Selected)
                    this.Deselect();
                else
                    this.Select();
        }


        private void XMainRectangle_OnGotFocus(object sender, RoutedEventArgs e)
        {
            Select();
        }

        private void XMainRectangle_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Deselect();
        }

        private void XGrid_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = DataContext as RegionViewModel;
            SessionController.Instance.SessionView.ShowDetailView(vm?.LibraryElementController);
            var regionController = vm?.RegionController;
            SessionController.Instance.SessionView.ShowDetailView(regionController);
        }


        private void xDelete_PointerPressed(object sender, PointerRoutedEventArgs e)
        {


            var vm = this.DataContext as PdfRegionViewModel;
            if (vm == null)
            {
                return;
            }

            var libraryElementController = vm.LibraryElementController;
            libraryElementController.RemoveRegion(vm.RegionController.Model);


        }
    }
}
