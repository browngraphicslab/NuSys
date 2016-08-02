using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    ///     xClippingWrapper.LibraryElementController = _vm.LibraryElementController;
    /// 
    /// </summary>
    public sealed partial class RectangleWrapper : UserControl, INuSysDisposable
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

        public event EventHandler Disposed;

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
                var rectHeight = regionModel.Height * contentView.ActualHeight;
                
                var rect = new Rect(topLeftX, topLeftY, rectWidth, rectHeight); 
                
                xClippingRectangle.Rect = rect;
                //This section onwards is for resizing 


                var scaleX = 1 / regionModel.Width;
                var scaleY = 1 / regionModel.Height;
                var lesserScale = scaleX < scaleY ? scaleX : scaleY;
                // shifts the clipped rectangle so its upper left corner is in the upper left corner of the node
                var compositeTransform = WrapperTransform;
                if (DataContext is DetailHomeTabViewModel) {
                    var regionHalfWidth = regionModel.Width * xClippingContent.ActualWidth / 2.0;
                    var regionHalfHeight = regionModel.Height * xClippingContent.ActualHeight / 2.0;

                    var detailViewHalfWidth = xClippingContent.ActualWidth / 2.0;
                    var detailViewHalfHeight = xClippingContent.ActualHeight / 2.0;

                    var regionTopLeftX = xClippingContent.ActualWidth * regionModel.TopLeftPoint.X;
                    var regionTopLeftY = xClippingContent.ActualHeight * regionModel.TopLeftPoint.Y;

                    var regionCenterX = -(regionTopLeftX + regionHalfWidth - detailViewHalfWidth);
                    var regionCenterY = -(regionTopLeftY + regionHalfHeight - detailViewHalfHeight);



                    compositeTransform.TranslateX = regionCenterX;
                    compositeTransform.TranslateY = regionCenterY;

                    compositeTransform.ScaleX = lesserScale;
                    compositeTransform.ScaleY = lesserScale;

                    compositeTransform.CenterX = regionTopLeftX + regionHalfWidth;
                    compositeTransform.CenterY = regionTopLeftY + regionHalfHeight;
                }
                else
                {
                    // shifts the clipped rectangle so its upper left corner is in the upper left corner of the node
                    compositeTransform.TranslateX = -regionModel.TopLeftPoint.X * xClippingContent.ActualWidth * scaleX;
                    compositeTransform.TranslateY = -regionModel.TopLeftPoint.Y * xClippingContent.ActualHeight * scaleY;
                    compositeTransform.ScaleX = scaleX;
                    compositeTransform.ScaleY = scaleY;
//                    compositeTransform.CenterX = regionModel.TopLeftPoint.X * xClippingContent.ActualWidth * scaleX;
//                    compositeTransform.CenterY = regionModel.TopLeftPoint.Y * xClippingContent.ActualHeight * scaleY;
                }
                WrapperTransform = compositeTransform;

                //now we want to resize the components of the imageregionview so that it isn't too big
                foreach (var item in xClippingCanvas.Items)
                {
                    var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
                    FrameworkElement region;
                    switch (regionViewModel.Model.Type)
                    {
                        case ElementType.ImageRegion:
                            region = item as ImageRegionView;
                            (region as ImageRegionView).RescaleComponents(WrapperTransform.ScaleX, WrapperTransform.ScaleY);
                            break;
                        case ElementType.PdfRegion:
                            region = item as PDFRegionView;
                            (region as PDFRegionView).RescaleComponents(WrapperTransform.ScaleX,WrapperTransform.ScaleY);
                            break;
                        default:
                            break;
                    }
                }
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
        public async Task ProcessLibraryElementController()
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
                Debug.Assert(DataContext != null);
                await AddRegionView(regionId);
            }

            // Add the OnRegionAdded and OnRegionRemoved events so the view is updated
            var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(Controller.LibraryElementModel.ContentDataModelId);
            contentDataModel.OnRegionAdded += AddRegionView;
            contentDataModel.OnRegionRemoved += RemoveRegionView;

        }

        /// <summary>
        /// Adds a new region view to the wrapper
        /// </summary>
        public async Task AddRegionView(string regionLibraryElementId)
        {
            await UITask.Run(delegate {

                // get the region from the id
                var regionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementId) as RectangleRegionLibraryElementController;
                Debug.Assert(regionLibraryElementController != null);
                Debug.Assert(regionLibraryElementController.LibraryElementModel is RectangleRegion);

                // if we are in the region itself then don't create a region view
                if (regionLibraryElementController.LibraryElementModel.LibraryElementId == Controller.LibraryElementModel.LibraryElementId)
                {
                    return;
                }

                // todo video functionality
                // used to check if the wrapper is in an editable detailhometabviewmodel
                DetailHomeTabViewModel ParentDetailDC = null;
                // create the view and vm based on the region type
                FrameworkElement view = null;
                RegionViewModel vm = null;
                switch (regionLibraryElementController.LibraryElementModel.Type)
                {
                    case ElementType.ImageRegion:
                        vm = new ImageRegionViewModel(regionLibraryElementController.LibraryElementModel as RectangleRegion,
                                regionLibraryElementController, this);
                        view = new ImageRegionView(vm as ImageRegionViewModel);
                        Disposed += (view as ImageRegionView).Dispose;
                        // get all the data context stuff in a view.loaded delegate, because it comes from xaml and must be loaded to be accessed in a ui thread
                        view.Loaded += delegate
                        {
                            ParentDetailDC = DataContext as DetailHomeTabViewModel;
                            (view as ImageRegionView).RescaleComponents(WrapperTransform.ScaleX, WrapperTransform.ScaleY);

                            // set editable based on the parent data context
                            vm.Editable = false;
                            if (ParentDetailDC != null)
                            {
                                vm.Editable = ParentDetailDC.Editable;
                            }
                        };



                        break;
                    case ElementType.PdfRegion:
                        vm = new PdfRegionViewModel(regionLibraryElementController.LibraryElementModel as PdfRegionModel, 
                                regionLibraryElementController as PdfRegionLibraryElementController, this);
                        view = new PDFRegionView(vm as PdfRegionViewModel);
                        Disposed += (view as PDFRegionView).Dispose;

                        // get all the data context stuff in a view.loaded delegate, because it comes from xaml and must be loaded to be accessed in a ui thread
                        view.Loaded += delegate
                        {
                            (view as PDFRegionView).RescaleComponents(WrapperTransform.ScaleX, WrapperTransform.ScaleY);
                            // check the page number of detail view parent data context and node view parent data context and set visibility
                            var ParentNodeDC = DataContext as PdfNodeViewModel;
                            ParentDetailDC = DataContext as DetailHomeTabViewModel;
                            if (ParentNodeDC != null)
                            {
                                view.Visibility = ParentNodeDC.CurrentPageNumber == (vm.Model as PdfRegionModel).PageLocation ? Visibility.Visible : Visibility.Collapsed;
                            }
                            else if (ParentDetailDC != null)
                            {
                                var PdfParentDetailDC = ParentDetailDC as PdfDetailHomeTabViewModel;
                                view.Visibility = PdfParentDetailDC.CurrentPageNumber == (vm.Model as PdfRegionModel).PageLocation ? Visibility.Visible : Visibility.Collapsed;
                            }
                            else
                            {
                                Debug.Fail("the parent data context should always be a detail view or node view, if not the visibility should be taken care of here");
                            }

                            // set editable based on the parent data context
                            vm.Editable = false;
                            if (ParentDetailDC != null)
                            {
                                vm.Editable = ParentDetailDC.Editable;
                            }

                        };
                        break;

                    default:
                        vm = null;
                        view = null;
                        break;
                }

                //// set editable based on the parent data context
                //vm.Editable = false;
                //if (ParentDetailDC != null)
                //{
                //    vm.Editable = ParentDetailDC.Editable;
                //}

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
            return this.ActualWidth;
        }
        public double GetHeight()
        {
            return this.ActualHeight;
        }
        public double GetViewWidth()
        {
            return xClippingContent.ActualWidth;
        }
        public double GetViewHeight()
        {
            return xClippingContent.ActualHeight;
        }

        public ItemCollection GetRegionItems()
        {
            return xClippingCanvas.Items;
        }

        public void Dispose()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        // My code is slick yo - Sahil, July 2016

        // Why is this so broken - everybody else

        // But its slick - sahil "slick" mishra

        // Your code is actually slick - Luke "literally crying" Murray

    }


}
