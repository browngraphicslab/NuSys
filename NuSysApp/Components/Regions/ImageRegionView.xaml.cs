﻿using System;
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

        public bool Selected {private set; get; }

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

            var parentWidth = vm.ContainerViewModel.GetWidth();
            var parentHeight = vm.ContainerViewModel.GetHeight();
            
            composite.TranslateX = model.TopLeftPoint.X * parentWidth;
            composite.TranslateY = model.TopLeftPoint.Y * parentHeight;

            //If in detail view, adjust to the right to account for difference between view and actual image.
            if (vm.ContainerViewModel is ImageDetailHomeTabViewModel)
            {
                var ivm = vm.ContainerViewModel as ImageDetailHomeTabViewModel;
                var diff = ivm.GetViewWidth() - parentWidth;
                composite.TranslateX += diff / 2;
            }



            _tx = composite.TranslateX;
            _ty = composite.TranslateY;

            vm.Width = (model.Width) * parentWidth;
            vm.Height = (model.Height) * parentHeight;

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

            //If in detail view, adjust to the right to account for difference between view and actual image.
            if (vm.ContainerViewModel is ImageDetailHomeTabViewModel)
            {
                var ivm = vm.ContainerViewModel as ImageDetailHomeTabViewModel;
                var diff = ivm.GetViewWidth() - ivm.GetWidth();
                composite.TranslateX += diff / 2;
            }
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


            var ivm = vm.ContainerViewModel as ImageDetailHomeTabViewModel;
            var diff = ivm.GetViewWidth() - vm.ContainerViewModel.GetWidth();

            var leftXBound = diff / 2;
            var rightXBound = diff / 2 + ivm.GetWidth();


            //CHANGE IN X
            if (xMainRectangle.Width + rt.TranslateX + e.Delta.Translation.X <= rightXBound)
            {
                xMainRectangle.Width = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 25);
                vm.Width = xMainRectangle.Width;


            }
            
            if (xMainRectangle.Height + rt.TranslateY + e.Delta.Translation.Y <= vm.ContainerHeight)
            {
                xMainRectangle.Height = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
                vm.Height = xMainRectangle.Height;

            }

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

            var ivm = vm.ContainerViewModel as ImageDetailHomeTabViewModel;
            var diff = ivm.GetViewWidth() - vm.ContainerViewModel.GetWidth();

            var leftXBound = diff / 2;
            var rightXBound = diff / 2 + ivm.GetWidth() - vm.Width;



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
            }

            var composite = RenderTransform as CompositeTransform;
            var topLeft = new Point(composite.TranslateX - leftXBound, composite.TranslateY);
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
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.DarkBlue);
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

        private void XGrid_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = DataContext as RegionViewModel;
            SessionController.Instance.SessionView.ShowDetailView(vm?.LibraryElementController);
            var regionController = vm?.RegionController;
            SessionController.Instance.SessionView.ShowDetailView(regionController);
        }
    }
}