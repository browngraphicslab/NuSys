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
using SharpDX.Direct3D11;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoRegionView : UserControl
    {
        private bool _toggleManipulation;
        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;
        public VideoRegionView(VideoRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _toggleManipulation = false;
            xMainRectangle.RenderTransform = new CompositeTransform();
            vm.PropertyChanged += PropertyChanged;
            
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Width":
                case "Height":
                    var vm = DataContext as VideoRegionViewModel;
                    if (vm == null)
                    {
                        break;
                    }
                    xMainRectangle.Width = vm.Width;
                    xMainRectangle.Height = vm.Height;
                    break;
            }

        }

        private void HeightChanged (object sender, double e)
        {
            Rect.Height = e- 100;
        }
        private void WidthChanged (object sender, double e)
        {
            Rect.Width = e ;
        }

        private void Handle_OnPointerPressed(object sender, PointerRoutedEventArgs e)
       {
            _toggleManipulation = true;
        }

        private void Bound1_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!(Rect.Width - e.Delta.Translation.X > 0)) return;
            UpdateModel(e.Delta.Translation.X,0,new Point(), new Point());
        }

        private void Handle_OnManipulationDelta2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Rect.Width + e.Delta.Translation.X > 0)
            {
                UpdateModel(0, e.Delta.Translation.X,new Point(), new Point());
            }
        }

        private void Handle_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _toggleManipulation = false;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }
        private void xResizingTriangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as VideoRegionViewModel;
            if (vm == null)
            {
                return;
            }
            if (vm.BottomRight.X-vm.TopLeft.X +e.Delta.Translation.X < 0 || e.Delta.Translation.X + vm.TopLeft.X  + vm.RectSize.X> vm.ContainerViewModel.GetWidth())
                return;
            if (vm.BottomRight.Y-vm.TopLeft.Y +e.Delta.Translation.Y < 0 || e.Delta.Translation.Y + vm.TopLeft.Y  + vm.RectSize.Y> vm.ContainerViewModel.GetHeight())
                return;
            vm.Width = Math.Max(vm.Width + e.Delta.Translation.X, 0);
            vm.Height = Math.Max(vm.Height + e.Delta.Translation.Y, 0);
            if (vm.Width * vm.Height == 0)
            {
                return;
            }
            UpdateModel(0,0,new Point(), e.Delta.Translation);

            //ResizerTransform.TranslateX += e.Delta.Translation.X/2;
            //ResizerTransform.TranslateY += e.Delta.Translation.Y/2;

        }
        private void xResizingTriangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }

        private void xResizingTriangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);
            e.Handled = true;
        }

        private void UpdateViewModel()
        {
            var composite = RenderTransform as CompositeTransform;
            var vm = DataContext as VideoRegionViewModel;
            if (vm == null || composite == null)
            {
                return;
            }
            var topLeft = new Point(composite.TranslateX, composite.TranslateY);
            var bottomRight = new Point(topLeft.X + xMainRectangle.Width, topLeft.Y + xMainRectangle.Height);
           // vm.SetNewPoints(topLeft, bottomRight);
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as VideoRegionViewModel;
            if (vm == null)
            {
                return;
            }
            if (vm.TopLeft.X +e.Delta.Translation.X < 0 || e.Delta.Translation.X + vm.TopLeft.X  + vm.RectSize.X> vm.ContainerViewModel.GetWidth())
                return;
            if (vm.TopLeft.Y +e.Delta.Translation.Y < 0 || e.Delta.Translation.Y + vm.TopLeft.Y  + vm.RectSize.Y> vm.ContainerViewModel.GetHeight())
                return;
            UpdateModel(0,0,e.Delta.Translation,e.Delta.Translation);
            e.Handled = true;
        }


        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);
            e.Handled = true;
        }
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);
        }
        private void UpdateModel(double Audd1, double Audd2,Point Imgd1, Point Imgd2)
        {
                var vm = this.DataContext as VideoRegionViewModel; 
                vm.SetNewPoints(Audd1,Audd2, Imgd1,Imgd2); 
        }

    }
}
