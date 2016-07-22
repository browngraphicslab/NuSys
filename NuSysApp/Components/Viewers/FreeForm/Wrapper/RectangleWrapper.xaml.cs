using System;
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

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set
            {
                SetValue(ContentProperty, value);
                xClippingContent.Content = value;
            }
        }

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

            if (Controller == null)
            {
                return;
            }
            var type = Controller.LibraryElementModel.Type;

            if (Constants.IsRegionType(type))
            {
                // region implementation
                var regionModel = (Controller as RectangleRegionLibraryElementController).LibraryElementModel as RectangleRegion;
                var rect = new Rect(regionModel.TopLeftPoint.X * this.ActualWidth, regionModel.TopLeftPoint.Y * this.ActualHeight,
                    regionModel.Width * this.ActualWidth, regionModel.Height * this.ActualHeight);
                xClippingRectangle.Rect = rect;

                var compositeTransform = xClippingCompositeTransform;
                compositeTransform.TranslateX = -regionModel.TopLeftPoint.X*this.ActualWidth * 1/regionModel.Width;
                compositeTransform.TranslateY = -regionModel.TopLeftPoint.Y * this.ActualHeight * 1/regionModel.Height;


            }
            else
            {
                var rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
                xClippingRectangle.Rect = rect;
            }

            

        }

        private void ProcessLibraryElementController()
        {

            Debug.Assert(Controller != null);
            var type = Controller.LibraryElementModel.Type;

            if (Constants.IsRegionType(type))
            {

                var scaleX = 1 / (Controller.LibraryElementModel as RectangleRegion).Width;
                var scaleY = 1 / (Controller.LibraryElementModel as RectangleRegion).Height;

                var compositeTransform = xClippingCompositeTransform;
                compositeTransform.ScaleX = scaleX;
                compositeTransform.ScaleY = scaleY;
                xClippingCompositeTransform = compositeTransform;
            }


        }


        // My code is slick yo - Sahil, July 2016

        // Why is this so broken - everybody else

        // But its slick - sahil "slick" mishra
    }


}
