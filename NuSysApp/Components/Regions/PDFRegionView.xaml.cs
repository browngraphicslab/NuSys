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
        private double _tx;
        private double _ty;


        public bool Selected {private set; get; }


        public PDFRegionView(PdfRegionViewModel regionVM)
        {
            
            this.InitializeComponent();
            this.DataContext = regionVM;
            this.Deselect();
            var model = regionVM.Model as PdfRegion;
            if (model == null)
            {
                return;
            }

            CompositeTransform composite = new CompositeTransform();
            this.RenderTransform = composite;
           
            regionVM.SizeChanged += ChangeSize;
            regionVM.LocationChanged += ChangeLocation;

            var parentWidth = regionVM.ContainerViewModel.GetWidth();
            var parentHeight = regionVM.ContainerViewModel.GetHeight();

            composite.TranslateX = model.TopLeftPoint.X * parentWidth;
            composite.TranslateY = model.TopLeftPoint.Y * parentHeight;
            regionVM.Width = (model.Width) * parentWidth;
            regionVM.Height = (model.Height) * parentHeight;


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
                var horizontalMargin = (pvm.GetViewWidth() - pvm.GetWidth()) / 2;
                var verticalMargin = (pvm.GetViewHeight() - pvm.GetHeight()) / 2;
                composite.TranslateX += horizontalMargin;
                composite.TranslateY += verticalMargin;
            }
        }
        /// <summary>
        /// Changes size of view according to element that contains it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
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
            //Because editing is done only in region editor tab, this is probably safe to cast.
            var pvm = vm.ContainerViewModel as PdfDetailHomeTabViewModel;
            var horizontalMargin = (pvm.GetViewWidth() - pvm.GetWidth()) / 2;
            var verticalMargin = (pvm.GetViewHeight() - pvm.GetHeight()) / 2;

            var leftXBound = horizontalMargin;
            var rightXBound = horizontalMargin + pvm.GetWidth();

            var upYBound = verticalMargin;
            var downYBound = verticalMargin + pvm.GetHeight();

            if (xMainRectangle.Width + rt.TranslateX + e.Delta.Translation.X <= rightXBound)
            {
                xMainRectangle.Width = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 25);
                vm.Width = xMainRectangle.Width;
            }

            if (xMainRectangle.Height + rt.TranslateY + e.Delta.Translation.Y <= downYBound)
            {
                xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
                vm.Height = xMainRectangle.Height;

            }

            vm.SetNewSize(xMainRectangle.Width, xMainRectangle.Height);
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
            var pvm = vm.ContainerViewModel as PdfDetailHomeTabViewModel;
            var horizontalMargin = (pvm.GetViewWidth() - pvm.GetWidth()) / 2;
            var verticalMargin = (pvm.GetViewHeight() - pvm.GetHeight()) / 2;

            var leftXBound = horizontalMargin;
            var rightXBound = horizontalMargin + pvm.GetWidth() - vm.Width;


            var upYBound = verticalMargin;
            var downYBound = verticalMargin + pvm.GetHeight() - vm.Height;


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
            //Makes sure the location of the point is generalized -- not relative to the margined container.
            var topLeft = new Point(composite.TranslateX - leftXBound, composite.TranslateY - upYBound);
            //Updates the viewmodel

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
            //SessionController.Instance.SessionView.ShowDetailView(vm?.LibraryElementController);
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

        private void xNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = DataContext as PdfRegionViewModel;
            vm.SetNewName((sender as TextBox).Text);

        }
    }
}
