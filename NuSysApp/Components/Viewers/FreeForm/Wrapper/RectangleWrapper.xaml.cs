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
using NusysIntermediate;

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
    public sealed partial class RectangleWrapper : UserControl, INuSysDisposable, IRegionHideable
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

        private FrameworkElement _selectedRegion;


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
            ApplyTransforms();

        }
        /// <summary>
        /// This method transforms the wrapper so that it fits the outer lying container(node/detail view)
        /// </summary>
        public void ApplyTransforms()
        {
            var type = Controller.LibraryElementModel.Type;
            var contentView = xClippingContent;
            Debug.Assert(contentView != null);
            if (NusysConstants.IsRegionType(type))
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
                if (DataContext is DetailHomeTabViewModel)
                {
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
                        case NusysConstants.ElementType.ImageRegion:
                            region = item as ImageRegionView;
                            (region as ImageRegionView).RescaleComponents(WrapperTransform.ScaleX, WrapperTransform.ScaleY);
                            break;
                        case NusysConstants.ElementType.PdfRegion:
                            region = item as PDFRegionView;
                            (region as PDFRegionView).RescaleComponents(WrapperTransform.ScaleX, WrapperTransform.ScaleY);
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

            if (NusysConstants.IsRegionType(type))
            {
                // This adds the handlers to update the size and the position of the region when it changes
                var regionController = Controller as RectangleRegionLibraryElementController;
                regionController.LocationChanged += RegionController_LocationChanged;
                regionController.SizeChanged += RegionController_SizeChanged;
            }

            // clear the items control
            xClippingCanvas.Items.Clear();

            //clear our reference to the selected region
            _selectedRegion = null;

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

        private void RegionController_SizeChanged(object sender, double width, double height)
        {
            var tempRect = xClippingRectangle.Rect;
            tempRect.Width = Content.ActualWidth * width;
            tempRect.Height = Content.ActualHeight * height;
            xClippingRectangle.Rect = tempRect;
            if (DataContext is ElementViewModel)
            {
                var vm = DataContext as ElementViewModel;
                if(vm.Width > vm.Height)
                {
                    vm.Controller.SetSize(vm.Height/vm.GetRatio(), vm.Height);
                }
                else
                {
                    vm.Controller.SetSize(vm.Width, vm.Width*vm.GetRatio());
                }
            }
            ApplyTransforms();
        }

        private void RegionController_LocationChanged(object sender, Point topLeft)
        {
            var tempRect = xClippingRectangle.Rect;
            tempRect.X = topLeft.X * this.ActualWidth;
            tempRect.Y = topLeft.Y * this.ActualHeight;
            xClippingRectangle.Rect = tempRect;
            ApplyTransforms();

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
                    case NusysConstants.ElementType.ImageRegion:
                        vm = new ImageRegionViewModel(regionLibraryElementController.LibraryElementModel as RectangleRegion,
                                regionLibraryElementController, this);
                        view = new ImageRegionView(vm as ImageRegionViewModel);
                        (view as ImageRegionView).OnSelectedOrDeselected += Region_OnSelectedOrDeselected;
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
                    case NusysConstants.ElementType.PdfRegion:
                        vm = new PdfRegionViewModel(regionLibraryElementController.LibraryElementModel as PdfRegionModel, 
                                regionLibraryElementController as PdfRegionLibraryElementController, this);
                        view = new PDFRegionView(vm as PdfRegionViewModel);
                        (view as PDFRegionView).OnSelectedOrDeselected += Region_OnSelectedOrDeselected;

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

        private void Region_OnSelectedOrDeselected(object sender, bool selected)
        {
            if (selected)
            {
                var region = sender as FrameworkElement;
                Debug.Assert(region != null);
                DeselectRegion(_selectedRegion);
                _selectedRegion = region;                
            }
            else
            {
                var region = sender as FrameworkElement;
                Debug.Assert(region != null);
                _selectedRegion = null;
            }
        }
        public void AddTemporaryRegion(TemporaryImageRegionView view)
        {
            xTemporaryClippingCanvas.Items.Add(view);
        }
        public void ClearTemporaryRegions()
        {
            foreach (var view in xTemporaryClippingCanvas.Items)
            {
                (view as TemporaryImageRegionView).Dispose(this, EventArgs.Empty);
            }
            xTemporaryClippingCanvas.Items.Clear();
            
        }
        public void RemoveTemporaryRegion(TemporaryImageRegionViewModel vm)
        {
            foreach(FrameworkElement view in new HashSet<FrameworkElement>(xTemporaryClippingCanvas.Items.Select(e => e as FrameworkElement)))
            {
                var dc = view.DataContext as TemporaryImageRegionViewModel;
                if (dc.NormalizedHeight == vm.NormalizedHeight &&
                    dc.NormalizedWidth  == vm.NormalizedWidth &&
                    dc.NormalizedTopLeftPoint == vm.NormalizedTopLeftPoint)
                {
                    (view as TemporaryImageRegionView).Dispose(this, EventArgs.Empty);
                    xTemporaryClippingCanvas.Items.Remove(view);
                } 
            }
        }

        public void RemoveRegionView(string regionLibraryElementId)
        {
                foreach (var item in xClippingCanvas.Items)
                {
                    var regionVM = (item as FrameworkElement).DataContext as RegionViewModel;
                    Debug.Assert(regionVM != null);


                    if (regionVM.Model.LibraryElementId == regionLibraryElementId)
                   {
                    regionVM.Dispose(null, EventArgs.Empty);
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
            if (Controller != null)
            {
                var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(Controller.LibraryElementModel.ContentDataModelId);
                contentDataModel.OnRegionAdded -= AddRegionView;
                contentDataModel.OnRegionRemoved -= RemoveRegionView;
            }
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        private void xClippingContent_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            foreach (var item in xClippingCanvas.Items)
            {
                DeselectRegion(item as FrameworkElement);
            }
            _selectedRegion = null;
        }
        /// <summary>
        /// Helper method for selecting regions based on type
        /// </summary>
        private void SelectRegion(FrameworkElement item)
        {
            if(item == null)
            {
                return;
            }
            var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
            switch (regionViewModel?.Model.Type)
            {
                case NusysConstants.ElementType.ImageRegion:
                    var imageRegionView = item as ImageRegionView;
                    imageRegionView.FireSelection();
                    break;
                case NusysConstants.ElementType.PdfRegion:
                    var pdfRegionView = item as PDFRegionView;
                    pdfRegionView.FireSelection();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Helper method for deselecting regoins based on type
        /// </summary>
        private void DeselectRegion(FrameworkElement item)
        {
            if(item == null)
            {
                return;
            }
            var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
            switch (regionViewModel?.Model.Type)
            {
                case NusysConstants.ElementType.ImageRegion:
                    var imageRegionView = item as ImageRegionView;
                    imageRegionView.FireDeselection();
                    break;
                case NusysConstants.ElementType.PdfRegion:
                    var pdfRegionView = item as PDFRegionView;
                    pdfRegionView.FireDeselection();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Makes every single region in the wrapper visible
        /// </summary>
        public void ShowAllRegions()
        {
            foreach (var item in xClippingCanvas.Items)
            {
                var regionView = item as FrameworkElement;
                regionView.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// Makes every region in this wrapper invisible
        /// </summary>
        public void HideAllRegions()
        {
            foreach (var item in xClippingCanvas.Items)
            {
                var regionView = item as FrameworkElement;
                regionView.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowOnlyChildrenRegions()
        {
            foreach (var item in xClippingCanvas.Items)
            {
                var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
                var regionModel = regionViewModel.Model as Region;
                var region = item as FrameworkElement;

                if (regionViewModel.Model.ClippingParentId == Controller.LibraryElementModel.LibraryElementId)
                {
                    region.Visibility = Visibility.Visible;
                }
                else
                {
                    region.Visibility = Visibility.Collapsed;
                }

            }
        }


        // My code is slick yo - Sahil, July 2016

        // Why is this so broken - everybody else

        // But its slick - sahil "slick" mishra

        // Your code is actually slick - Luke "literally crying" Murray

    }


}
