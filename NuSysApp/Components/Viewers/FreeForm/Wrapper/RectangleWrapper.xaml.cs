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

        //public RectangleGeometry Rect { get { return xClippingRectangle;} }
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
                var regionController = Controller as RectangleRegionLibraryElementController;
                Debug.Assert(regionController != null);
                var regionModel = regionController.LibraryElementModel as RectangleRegion;
                Debug.Assert(regionModel != null);
                // creates a clipping rectangle using parameters topleftX, topleftY, width, height
                // the regionModel Width and points are all normalized
                var topLeftX = regionModel.TopLeftPoint.X * contentView.ActualWidth;
                var topLeftY = regionModel.TopLeftPoint.Y * contentView.ActualHeight;
                var rectWidth = regionModel.Width * contentView.ActualWidth;
                var rectHeight = regionModel.Height * contentView.ActualWidth;
                
                var rect = new Rect(topLeftX, topLeftY, rectWidth, rectHeight); 
                
                xClippingRectangle.Rect = rect;

                //var scaleX = 1 / regionModel.Width;
                //var scaleY = 1 / regionModel.Height;

                //// shifts the clipped rectangle so its upper left corner is in the upper left corner of the node
                //var compositeTransform = xClippingCompositeTransform;
                //compositeTransform.TranslateX = -regionModel.TopLeftPoint.X* contentView.ActualWidth * 1/regionModel.Width;
                //compositeTransform.TranslateY = -regionModel.TopLeftPoint.Y * contentView.ActualHeight * 1/regionModel.Height;
                //compositeTransform.ScaleX = scaleX;
                //compositeTransform.ScaleY = scaleY;
                //xClippingCompositeTransform = compositeTransform;
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
            }

            // clear the items control
            xClippingCanvas.Items.Clear();

            // get the region ids for the wrapper
            var regionsLibraryElementIds =
                SessionController.Instance.RegionsController.GetContentDataModelRegionLibraryElementIds(
                    Controller.LibraryElementModel.ContentDataModelId);
            Debug.Assert(regionsLibraryElementIds != null);

            // for each region id create a new view and put it into the canvas
            foreach (var regionId in regionsLibraryElementIds)
            {
                AddRegionView(regionId);
            }

            // Add the OnRegionAdded and OnRegionRemoved events so the view is updated
            var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(Controller.LibraryElementModel.ContentDataModelId);
            contentDataModel.OnRegionAdded += AddRegionView;
            contentDataModel.OnRegionRemoved += RemoveRegionView;

        }

        /// <summary>
        /// Adds a new region view to the wrapper
        /// </summary>
        public void AddRegionView(string regionLibraryElementId)
        {
            UITask.Run(async delegate {
                // used to check if the wrapper is in an editable detailhometabviewmodel
                var ParentDC = DataContext as DetailHomeTabViewModel;

                // get the region from the id
                var regionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementId) as RectangleRegionLibraryElementController;
                Debug.Assert(regionLibraryElementController != null);
                Debug.Assert(regionLibraryElementController.LibraryElementModel is RectangleRegion);
                if (regionLibraryElementController.LibraryElementModel.LibraryElementId == Controller.LibraryElementModel.LibraryElementId)
                {
                    return;
                }
                // create the view and vm based on the region type
                // todo add pdf, and video functionality
                FrameworkElement view = null;
                ImageRegionViewModel vm = null;
                switch (regionLibraryElementController.LibraryElementModel.Type)
                {
                    case ElementType.ImageRegion:
                        vm = new ImageRegionViewModel(regionLibraryElementController.LibraryElementModel as RectangleRegion,
                                regionLibraryElementController, this);
                        view = new ImageRegionView(vm);
                        break;
                    default:
                        vm = null;
                        view = null;
                        break;
                }

                // set editable based on the parent data context
                vm.Editable = false;
                if (ParentDC != null)
                {
                    vm.Editable = ParentDC.Editable;
                }

                // add the region to thew view

                xClippingCanvas.Items.Add(view);
            });
        }

        public void RemoveRegionView(string regionLibraryElementId)
        {


                foreach (var item in xClippingCanvas.Items)
                {
                    var region = (item as FrameworkElement).DataContext as RegionViewModel;
                    Debug.Assert(region != null);

                    if (region.Model.LibraryElementId == regionLibraryElementId)
                    {
                        xClippingCanvas.Items.Remove(item);
                        return;
                    }
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

    }


}
