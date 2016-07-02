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
    /// <summary>
    /// View for region on image content.
    /// </summary>
    public sealed partial class ImageRegionView : UserControl
    {

        public Point _topLeft;
        public Point _bottomRight;
        private double _tx;
        private double _ty;

        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;

        public bool Selected { set; get; }

        public ImageRegionView(ImageRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.Deselect();

            CompositeTransform composite = new CompositeTransform();
            this.RenderTransform = composite;
            OnSelected?.Invoke(this, true);

            //vm.PropertyChanged += PropertyChanged;
            vm.SizeChanged += ChangeSize;
            vm.LocationChanged += ChangeLocation;
            var model = vm.Model as RectangleRegion;
            if (model == null)
            {
                return;
            }

            //vm.RegionChanged += RegionVM_RegionChanged;

            var parentWidth = vm.ContainerViewModel.GetWidth();
            var parentHeight = vm.ContainerViewModel.GetHeight();
            composite.TranslateX = model.TopLeftPoint.X * parentWidth;
            composite.TranslateY = model.TopLeftPoint.Y * parentHeight;

            _tx = composite.TranslateX;
            _ty = composite.TranslateY;

            vm.Width = (model.Width) * parentWidth;
            vm.Height = (model.Height) * parentHeight;
            Loaded += delegate (object sender, RoutedEventArgs args)
            {

            };
        }


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
        }

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
            if (xMainRectangle.Width >= vm.ContainerWidth - rt.TranslateX && xMainRectangle.Height >= vm.ContainerHeight - rt.TranslateY)
            {
                return;
            }
            else if (xMainRectangle.Width >= vm.ContainerWidth - rt.TranslateX && xMainRectangle.Height < vm.ContainerHeight - rt.TranslateY)
            {
                xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
                vm.Height = xMainRectangle.Height;

            }   
            else if (xMainRectangle.Width < vm.ContainerWidth - rt.TranslateX &&
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

            //UpdateViewModel();
            vm.SetNewSize(xMainRectangle.Width, xMainRectangle.Height);

        }

        private void xResizingTriangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

            //OnSelected?.Invoke(this, true);

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

            _tx += e.Delta.Translation.X;
            _ty += e.Delta.Translation.Y;

            if (_tx < 0)
            {
                rt.TranslateX = 0;
            }
            else if (_tx > vm.ContainerWidth - vm.OriginalWidth)
            {
                rt.TranslateX = vm.ContainerWidth - vm.OriginalWidth;
            }
            else
            {
                rt.TranslateX = _tx;
                vm.Width = vm.OriginalWidth;
            }

            if (_ty < 0)
            {
                rt.TranslateY = 0;
            }
            else if (_ty > vm.ContainerHeight - vm.OriginalHeight)
            {
                rt.TranslateY = vm.ContainerHeight - vm.OriginalHeight;
            }
            else
            {
                rt.TranslateY = _ty;
                vm.Height = vm.OriginalHeight;
            }

            //UpdateViewModel();
            var composite = RenderTransform as CompositeTransform;
            var topLeft = new Point(composite.TranslateX, composite.TranslateY);
            vm.SetNewLocation(topLeft);
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
            var vm = DataContext as ImageRegionViewModel;
            if (vm == null)
            {
                return;
            }
            var tx = ((CompositeTransform)this.RenderTransform).TranslateX;
            var ty = ((CompositeTransform)this.RenderTransform).TranslateY;
            if (tx < 0 || tx + vm.Width > vm.ContainerWidth)
                return;
            if (ty < 0 || ty + vm.Height > vm.ContainerHeight)
                return;


            vm.OriginalHeight = vm.Height;
            vm.OriginalWidth = vm.Width;
            //OnSelected?.Invoke(this, true);
            this.Select();
            e.Handled = true;

        }

        public void Deselect()
        {
            xMainRectangle.StrokeThickness = 3;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.Blue);
            xResizingTriangle.Visibility = Visibility.Collapsed;
            xDelete.Visibility = Visibility.Collapsed;
            xNameTextBox.Visibility = Visibility.Collapsed;
            Selected = false;


        }

        public void Select()
        {
            xMainRectangle.StrokeThickness = 6;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.Red);
            xResizingTriangle.Visibility = Visibility.Visible;
            xDelete.Visibility = Visibility.Visible;
            xNameTextBox.Visibility = Visibility.Visible;
            Selected = true;

        }


        
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
            libraryElementController.RemoveRegion(vm.RegionController.Model);


        }
        
    }
}