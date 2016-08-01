using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NuSysApp
{

    public sealed partial class AudioWrapper : Page
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
        /// The content of the wrapper, a media element in this case
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
        //denormalized start of audio
        public double AudioStart { set; get; }

        //denormalized end of audio
        public double AudioEnd { set; get; }

        public AudioWrapper()
        {
            this.InitializeComponent();
        }

        public void ProcessLibraryElementController()
        {
            Debug.Assert(Controller != null);
            var type = Controller.LibraryElementModel.Type;

            if (Constants.IsRegionType(type))
            {
                var regionController = Controller as AudioRegionLibraryElementController;
                AudioStart = regionController.AudioRegionModel.Start;
                AudioEnd = regionController.AudioRegionModel.End;



            }
            else
            {
                AudioStart = 0;
                AudioEnd = 1;
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

            var compositeTransform = new CompositeTransform();
            compositeTransform.ScaleX = 1 / (AudioEnd - AudioStart);
            compositeTransform.CenterX = this.ActualWidth * (AudioStart + (AudioEnd - AudioStart) / 2.0);
            RenderTransform = compositeTransform;
        }

        /// <summary>
        /// Adds a new region view to the wrapper
        /// </summary>
        public Task AddRegionView(string regionLibraryElementId)
        {
            UITask.Run(async delegate
            {
                // used to check if the wrapper is in an editable detailhometabviewmodel
                var ParentDC = DataContext as DetailHomeTabViewModel;

                // get the region from the id
                var regionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementId);
                if (regionLibraryElementController.LibraryElementModel.Type == ElementType.VideoRegion)
                {
                    return;
                }
                Debug.Assert(regionLibraryElementController != null);
       //         Debug.Assert(regionLibraryElementController.LibraryElementModel is RectangleRegion);
                if (regionLibraryElementController.LibraryElementModel.LibraryElementId == Controller.LibraryElementModel.LibraryElementId)
                {
                    return;
                }
                // create the view and vm based on the region type
                FrameworkElement view = null;
                RegionViewModel vm = null;
                switch (regionLibraryElementController.LibraryElementModel.Type)
                {
                    case ElementType.AudioRegion:
                        vm = new AudioRegionViewModel(regionLibraryElementController.LibraryElementModel as AudioRegionModel,
                                regionLibraryElementController as AudioRegionLibraryElementController, this);
                        view = new AudioRegionView(vm as AudioRegionViewModel);
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
            return null;
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

        private void xClippingGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (AudioEnd != 0 || AudioStart != 0)
            {
                var compositeTransform = new CompositeTransform();
                compositeTransform.ScaleX = 1 / (AudioEnd - AudioStart);
                compositeTransform.CenterX = this.ActualWidth * (AudioStart + (AudioEnd - AudioStart) / 2.0);
                RenderTransform = compositeTransform;
            }

            //// if the controller hasn't been set yet don't try to resize
            //if (Controller == null)
            //{
            //    return;
            //}
            //var type = Controller.LibraryElementModel.Type;
            //var contentView = xClippingContent;
            //Debug.Assert(contentView != null);
            //if (Constants.IsRegionType(type))
            //{
            //    var regionController = Controller as AudioRegionLibraryElementController;
            //    Debug.Assert(regionController != null);
            //    var regionModel = regionController.LibraryElementModel as AudioRegionModel;
            //    Debug.Assert(regionModel != null);

            //    var startX = regionModel.Start 


            //    // creates a clipping rectangle using parameters topleftX, topleftY, width, height
            //    // the regionModel Width and points are all normalized
            //    var topLeftX = regionModel.TopLeftPoint.X * contentView.ActualWidth;
            //    var topLeftY = regionModel.TopLeftPoint.Y * contentView.ActualHeight;
            //    var rectWidth = regionModel.Width * contentView.ActualWidth;
            //    var rectHeight = regionModel.Height * contentView.ActualHeight;

            //    var rect = new Rect(topLeftX, topLeftY, rectWidth, rectHeight);

            //    xClippingRectangle.Rect = rect;
            //    //This section onwards is for resizing 


            //    var scaleX = 1 / regionModel.Width;
            //    var scaleY = 1 / regionModel.Height;
            //    var lesserScale = scaleX < scaleY ? scaleX : scaleY;
            //    // shifts the clipped rectangle so its upper left corner is in the upper left corner of the node
            //    var compositeTransform = WrapperTransform;
            //    if (DataContext is DetailHomeTabViewModel)
            //    {
            //        var regionHalfWidth = regionModel.Width * xClippingContent.ActualWidth / 2.0;
            //        var regionHalfHeight = regionModel.Height * xClippingContent.ActualHeight / 2.0;

            //        var detailViewHalfWidth = xClippingContent.ActualWidth / 2.0;
            //        var detailViewHalfHeight = xClippingContent.ActualHeight / 2.0;

            //        var regionTopLeftX = xClippingContent.ActualWidth * regionModel.TopLeftPoint.X;
            //        var regionTopLeftY = xClippingContent.ActualHeight * regionModel.TopLeftPoint.Y;

            //        var regionCenterX = -(regionTopLeftX + regionHalfWidth - detailViewHalfWidth);
            //        var regionCenterY = -(regionTopLeftY + regionHalfHeight - detailViewHalfHeight);



            //        compositeTransform.TranslateX = regionCenterX;
            //        compositeTransform.TranslateY = regionCenterY;

            //        compositeTransform.ScaleX = lesserScale;
            //        compositeTransform.ScaleY = lesserScale;

            //        compositeTransform.CenterX = regionTopLeftX + regionHalfWidth;
            //        compositeTransform.CenterY = regionTopLeftY + regionHalfHeight;
            //    }
            //    else
            //    {
            //        // shifts the clipped rectangle so its upper left corner is in the upper left corner of the node
            //        compositeTransform.TranslateX = -regionModel.TopLeftPoint.X * xClippingContent.ActualWidth * scaleX;
            //        compositeTransform.TranslateY = -regionModel.TopLeftPoint.Y * xClippingContent.ActualHeight * scaleY;
            //        compositeTransform.ScaleX = scaleX;
            //        compositeTransform.ScaleY = scaleY;
            //        //                    compositeTransform.CenterX = regionModel.TopLeftPoint.X * xClippingContent.ActualWidth * scaleX;
            //        //                    compositeTransform.CenterY = regionModel.TopLeftPoint.Y * xClippingContent.ActualHeight * scaleY;
            //    }
            //    WrapperTransform = compositeTransform;

            //    //now we want to resize the components of the imageregionview so that it isn't too big
            //    foreach (var item in xClippingCanvas.Items)
            //    {
            //        var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
            //        FrameworkElement region;
            //        switch (regionViewModel.Model.Type)
            //        {
            //            case ElementType.ImageRegion:
            //                region = item as ImageRegionView;
            //                (region as ImageRegionView).RescaleComponents(WrapperTransform.ScaleX, WrapperTransform.ScaleY);
            //                break;
            //            case ElementType.PdfRegion:
            //                region = item as PDFRegionView;
            //                (region as PDFRegionView).RescaleComponents(WrapperTransform.ScaleX, WrapperTransform.ScaleY);
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
            //else
            //{
            //    // since we aren't in a rectangle, the clipping rectangle contains the entire image
            //    var rect = new Rect(0, 0, contentView.ActualWidth, contentView.ActualHeight);
            //    xClippingRectangle.Rect = rect;
            //}
        }
    }
}
