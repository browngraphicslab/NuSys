﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    /// <summary>
    /// 
    ///  xaml - wrap your content like this
    /// 
    ///             <local:RectangleWrapper x:Name="xClippingWrapper">
    ///                 <local:RectangleWrapper.Content>
    ///                    <Image x:Name="xImage" Source="{Binding Image}" Stretch="Fill" />
    ///                 </local:RectangleWrapper.Content>
    ///             </local:RectangleWrapper>
    /// 
    ///  code behind -  place this in on loaded
    ///     xClippingWrapper.Controller = _vm.LibraryElementController;
    /// 
    /// </summary>
    public sealed partial class RectangleWrapper : UserControl
    {

        /// <summary>
        /// Registers the ContentProperty as a value called contet which you can access through xaml
        /// 
        /// Content - the name of the property in xaml ie.  <Wrapper Content="">
        ///                                                     or
        ///                                                     <Wrapper.Content></Wrapper.Content>
        /// typeof(FrameWorkElement) - what the type of the content is
        /// typeof(object) - the sender that calls the ContentProperty, dont worry about it
        /// new PropertyMetaData() - where you would put methods which are called if the content were bound using {binding: name} 
        ///                         if the binding changes, default is null, but you can add your own so use (null, callbacks)
        /// </summary>
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                "Content", typeof(FrameworkElement), typeof(object), new PropertyMetadata(null));

        private LibraryElementController _contentController;


        /// <summary>
        /// The content of the wrapper, basically any rectangle based format, ideally images
        /// </summary>
        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set
            {
                SetValue(ContentProperty, value);
                xClippingContent.Content = value;
            }
        }

        /// <summary>
        /// The library element controller of the node this is on, calls processlibraryelementController when it is set, should only happen once
        /// </summary>
        public LibraryElementController Controller
        {
            get { return _contentController; }
            set
            {
                _contentController = value;
                ProcessLibraryElementController();
            }
        }

        public RectangleWrapper()
        {
            this.InitializeComponent();
             
        }

        private void XClippingGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // if the controller hasn't been set yet don't try to resize
            if (Controller == null)
            {
                return;
            }
            var type = Controller.LibraryElementModel.Type;
            var contentView = xClippingContent;
            Debug.Assert(contentView != null);
            if (Constants.IsRegionType(type))
            {
                
                var regionModel = (Controller as RectangleRegionLibraryElementController).LibraryElementModel as RectangleRegion;
                // creates a clipping rectangle using parameters topleftX, topleftY, width, height
                // the regionModel Width and points are all normalized
                var rect = new Rect(regionModel.TopLeftPoint.X * contentView.ActualWidth, regionModel.TopLeftPoint.Y * contentView.ActualHeight,
                    regionModel.Width * contentView.ActualWidth, regionModel.Height * contentView.ActualHeight);
                xClippingRectangle.Rect = rect;

                var scaleX = 1 / (Controller.LibraryElementModel as RectangleRegion).Width;
                var scaleY = 1 / (Controller.LibraryElementModel as RectangleRegion).Height;

                // shifts the clipped rectangle so its upper left corner is in the upper left corner of the node
                var compositeTransform = xClippingCompositeTransform;
                compositeTransform.TranslateX = -regionModel.TopLeftPoint.X* contentView.ActualWidth * 1/regionModel.Width;
                compositeTransform.TranslateY = -regionModel.TopLeftPoint.Y * contentView.ActualHeight * 1/regionModel.Height;
                compositeTransform.ScaleX = scaleX;
                compositeTransform.ScaleY = scaleY;
                xClippingCompositeTransform = compositeTransform;
            }
            else
            {
                // since we aren't in a rectangle, the clipping rectangle contains the entire image
                var rect = new Rect(0, 0, contentView.ActualWidth, contentView.ActualHeight);
                xClippingRectangle.Rect = rect;
            }
        }

        /// <summary>
        /// Called once when the library element controller is set to set the scale of the region if the library elment controller
        /// represents a region
        /// </summary>
        private void ProcessLibraryElementController()
        {

            Debug.Assert(Controller != null);
            var type = Controller.LibraryElementModel.Type;

            if (Constants.IsRegionType(type))
            {
                // rectangle region width and height are normalized so this is something like scaleX = 1 / .5
                var scaleX = 1 / (Controller.LibraryElementModel as RectangleRegion).Width;
                var scaleY = 1 / (Controller.LibraryElementModel as RectangleRegion).Height;

                var compositeTransform = xClippingCompositeTransform;
                compositeTransform.ScaleX = scaleX;
                compositeTransform.ScaleY = scaleY;
                xClippingCompositeTransform = compositeTransform;
            }
            var regionsLibraryElementIds =
                SessionController.Instance.RegionsController.GetRegionLibraryElementIds(
                    Controller.LibraryElementModel.LibraryElementId);

            if (regionsLibraryElementIds == null)
            {
                return;
            }
            foreach (var regionId in regionsLibraryElementIds)
            {
                var regionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(regionId) as RectangleRegionLibraryElementController;
                Debug.Assert(regionLibraryElementController != null);
                Debug.Assert(regionLibraryElementController.LibraryElementModel is RectangleRegion);
                var vm = new ImageRegionViewModel(regionLibraryElementController.LibraryElementModel as RectangleRegion,
                    regionLibraryElementController, this);
            //    if (!Editable)
            //        vm.Editable = false;
                var view = new ImageRegionView(vm);
                view.SizeChanged += XClippingGrid_OnSizeChanged;
                xClippingGrid.Children.Add(view);
            }
        }

        public double GetWidth()
        {
            return xClippingGrid.ActualWidth;
        }
        public double GetHeight()
        {
            return xClippingGrid.ActualHeight;
        }
        public double GetViewWidth()
        {
            return xClippingContent.ActualWidth;
        }
        public double GetViewHeight()
        {
            return xClippingContent.ActualHeight;
        }
        // My code is slick yo - Sahil, July 2016

        // Why is this so broken - everybody else

        // But its slick - sahil "slick" mishra

        // Your code is actually slick - Luke "literally crying" Murray
        private void XClippingContent_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Debug.Assert(Controller != null);
            SessionController.Instance.SessionView.DetailViewerView.ShowElement(Controller);
        }
    }


}