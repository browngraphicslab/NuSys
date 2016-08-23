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

namespace NuSysApp2
{
    /// <summary>
    /// View for region on image content.
    /// </summary>
    public sealed partial class ImageRegionView : UserControl
    {

        public Point _topLeft;
        private double _tx;
        private double _ty;

        public bool Selected {private set; get; }

        public ImageRegionView(ImageRegionViewModel vm, ClippedGridWrapper grid)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.Deselect();
            var model = vm.Model as RectangleRegion;
            if (model == null)
            {
                return;
            }

            CompositeTransform composite = new CompositeTransform();
            this.RenderTransform = composite;

            vm.PropertyChanged += PropertyChanged;
            vm.SizeChanged += ChangeSize;
            vm.LocationChanged += ChangeLocation;


            var parentWidth = vm.ContainerViewModel.GetWidth();
            var parentHeight = vm.ContainerViewModel.GetHeight();
            
            composite.TranslateX = model.TopLeftPoint.X * parentWidth;
            composite.TranslateY = model.TopLeftPoint.Y * parentHeight;
            vm.Width = (model.Width) * parentWidth;
            vm.Height = (model.Height) * parentHeight;

            //If in detail view, adjust to the right to account for difference between view and actual image.
            if (vm.ContainerViewModel is ImageDetailHomeTabViewModel)
            {
                var ivm = vm.ContainerViewModel as ImageDetailHomeTabViewModel;

                var horizontalMargin = (ivm.GetViewWidth() - parentWidth)/2;
                var verticalMargin = (ivm.GetViewHeight() - parentHeight)/2;
                composite.TranslateX += horizontalMargin;
                composite.TranslateY += verticalMargin;
            }

            _tx = composite.TranslateX;
            _ty = composite.TranslateY;
            grid.XClippedGrid.Children.Add(this);
            //grid.XClippedGrid.SizeChanged += ChangeSize;

        }

        /// <summary>
        /// Changes location of view according to the element that contains it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="topLeft"></param>
        private void ChangeLocation(object sender, Point topLeft)
        {

            var vm = DataContext as ImageRegionViewModel;

            var composite = RenderTransform as CompositeTransform;
            if (composite == null)
            {
                return;
            }

            composite.TranslateX = topLeft.X;
            composite.TranslateY = topLeft.Y;

            //If in detail view, adjust to the right to account for difference between view and actual image.
            if (vm.ContainerViewModel is ImageDetailHomeTabViewModel)
            {
                var ivm = vm.ContainerViewModel as ImageDetailHomeTabViewModel;
                var horizontalMargin = (ivm.GetViewWidth() - ivm.GetWidth())/ 2;
                var verticalMargin = (ivm.GetViewHeight() - ivm.GetHeight()) / 2;
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
            var vm = DataContext as ImageRegionViewModel;

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
        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Selected":
                    var vm = DataContext as ImageRegionViewModel;
                    if (vm.Selected)
                    {
                        this.Select();
                    }
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Updates the width and height of the region relative to the position of the resizing triangle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xResizingTriangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as ImageRegionViewModel;
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
            var ivm = vm.ContainerViewModel as ImageDetailHomeTabViewModel;
            if (ivm == null)
            {
                return;
            }

            var horizontalMargin = (ivm.GetViewWidth() - ivm.GetWidth()) / 2;
            var verticalMargin = (ivm.GetViewHeight() - ivm.GetHeight())/ 2;

            var leftXBound = horizontalMargin;
            var rightXBound = horizontalMargin + ivm.GetWidth();

            var upYBound = verticalMargin;
            var downYBound = verticalMargin + ivm.GetHeight();


            //CHANGE IN WIDTH
            if (xMainRectangle.Width + rt.TranslateX + e.Delta.Translation.X <= rightXBound)
            {
                xMainRectangle.Width = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 25);
                vm.Width = xMainRectangle.Width;

            }
            //CHANGE IN HEIGHT
            
            if (xMainRectangle.Height + rt.TranslateY + e.Delta.Translation.Y <= downYBound)
            {
                xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
                vm.Height = xMainRectangle.Height;
            }

            //Updates viewmodel
            vm.SetNewSize(xMainRectangle.Width, xMainRectangle.Height);

    
        }

        private void xResizingTriangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

            this.Select();
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            var vm = DataContext as ImageRegionViewModel;
            if (vm == null)
            {
                return;
            }

            var rt = ((CompositeTransform)this.RenderTransform);
            if (rt == null)
            {
                return;
            }

            var ivm = vm.ContainerViewModel as ImageDetailHomeTabViewModel;
            var horizontalMargin = (ivm.GetViewWidth() - ivm.GetWidth())/2;
            var verticalMargin = (ivm.GetViewHeight() - ivm.GetHeight())/2;

            var leftXBound = horizontalMargin;
            var rightXBound = horizontalMargin + ivm.GetWidth() - vm.Width;


            var upYBound = verticalMargin;
            var downYBound = verticalMargin + ivm.GetHeight() - vm.Height;

            _tx += e.Delta.Translation.X;
            _ty += e.Delta.Translation.Y;

            //Translating X
            if (_tx < leftXBound)
            {
                rt.TranslateX = leftXBound;
            }
            else if(_tx > rightXBound)
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
                rt.TranslateY = vm.ContainerHeight - vm.OriginalHeight;
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
            var vm = DataContext as ImageRegionViewModel;
            if (vm == null)
            {
                return;
            }

            _tx = ((CompositeTransform)this.RenderTransform).TranslateX;
            _ty = ((CompositeTransform)this.RenderTransform).TranslateY;

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


        //Selection is currently very primitive.
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DataContext as ImageRegionViewModel;

            if (!vm.Editable)
                return;

            if (Selected)
                this.Deselect();
            else
                this.Select();
                
        }

        private void xDelete_PointerPressed(object sender, PointerRoutedEventArgs e)
        {


            var vm = this.DataContext as ImageRegionViewModel;
            if (vm == null)
            {
                return;
            }

            var libraryElementController = vm.LibraryElementController;
            libraryElementController.RemoveRegion(vm.LibraryElementController.LibraryElementModel as RectangleRegion);


        }

        private void XGrid_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = DataContext as RegionViewModel;
            SessionController.Instance.SessionView.ShowDetailView(vm?.LibraryElementController);
            var regionController = vm?.LibraryElementController;
            SessionController.Instance.SessionView.ShowDetailView(regionController);
        }

        private void xNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = DataContext as ImageRegionViewModel;
            vm.SetNewName((sender as TextBox).Text);

        }
    }
}