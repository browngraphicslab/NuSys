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
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    /// <summary>
    /// Temporary view for region on image content that is generated by cognitive services
    /// </summary>
    public sealed partial class TemporaryImageRegionView : UserControl
    {
        /// <summary>
        /// this constructs the view for the temporary region as well as sets its handlers
        /// </summary>
        /// <param name="vm"></param>
        public TemporaryImageRegionView(TemporaryImageRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;


            CompositeTransform composite = new CompositeTransform();
            this.RenderTransform = composite;

            vm.LocationChanged += ChangeLocation;

            var parentWidth = vm.RectangleWrapper.GetWidth();
            var parentHeight = vm.RectangleWrapper.GetHeight();

            composite.TranslateX = vm.NormalizedTopLeftPoint.X * parentWidth;
            composite.TranslateY = vm.NormalizedTopLeftPoint.Y * parentHeight;
            vm.Width = (vm.NormalizedWidth) * parentWidth;
            vm.Height = (vm.NormalizedHeight) * parentHeight;
            vm.Disposed += Dispose;
        }



        /// <summary>
        /// Changes location of view according to the element that contains it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="topLeft"></param>
        private void ChangeLocation(object sender, Point topLeft)
        {

            var vm = DataContext as TemporaryImageRegionViewModel;

            var composite = RenderTransform as CompositeTransform;
            if (composite == null)
            {
                return;
            }

            composite.TranslateX = topLeft.X;
            composite.TranslateY = topLeft.Y;
        }


        /// <summary>
        /// How this works
        /// We scale the entire region based on the image being scaled. But we then want to invert the scaling on the visual components, 
        /// but not on the size of the region as a whole. To revert the scale, we divide the transforms by their current scale. using scaleX = 1/scaleX etc.
        /// we then shift the transforms over by certain margins. The math is simple even though the numbers look like "magic numbers."
        /// 
        /// The width of the rectangle borders is 3. The size of the delete button and resizing triangle is 25. So these magic numbers are simply
        /// the result of shifting things over by values relative to 25 and 3. 
        /// </summary>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        public void RescaleComponents(double scaleX, double scaleY)
        {


            //xMainRectangle.StrokeThickness = 3 / scaleX;
            xMainRectangleBorder.BorderThickness = new Thickness(3 / scaleX, 3 / scaleY, 3 / scaleX, 3 / scaleY);
        }
        /// <summary>
        /// This disposes the size changed, location changes and the rectangle tapped handler from the runtime
        /// so that this doesn't cause memory leaks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Dispose(object sender, EventArgs e)
        {
            var vm = DataContext as TemporaryImageRegionViewModel;
            vm.Disposed -= Dispose;
            vm.LocationChanged -= ChangeLocation;
            xMainRectangleBorder.Tapped -= xMainRectangleBorder_Tapped;
        }
        /// <summary>
        /// When a temporary region is tapped it should replace itself with a new permanent region
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void xMainRectangleBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //We create a request using the detail view's create args function
            var vm = this.DataContext as TemporaryImageRegionViewModel;
            CreateNewRectangleRegionLibraryElementRequestArgs regionRequestArgs;
            if (vm.PageLocation != null)
            {
                regionRequestArgs = vm.HomeTabViewModel?.GetNewCreateLibraryElementRequestArgs() as CreateNewPDFRegionLibraryElementRequestArgs;
                (regionRequestArgs as CreateNewPDFRegionLibraryElementRequestArgs).PageLocation = (int)vm.PageLocation;
            }
            else {
                regionRequestArgs = vm.HomeTabViewModel?.GetNewCreateLibraryElementRequestArgs() as CreateNewRectangleRegionLibraryElementRequestArgs;
            }
            var type = (vm.PageLocation == null ) ?NusysConstants.ElementType.ImageRegion : NusysConstants.ElementType.PdfRegion;

            // We need to then populate our new figures into it
            regionRequestArgs.RegionHeight = vm.NormalizedHeight;
            regionRequestArgs.RegionWidth = vm.NormalizedWidth;
            regionRequestArgs.TopLeftPoint = new PointModel(vm.NormalizedTopLeftPoint.X,vm.NormalizedTopLeftPoint.Y);
            // this is the rest of what's left to do to make this
            regionRequestArgs.ContentId = vm.HomeTabViewModel.LibraryElementController.LibraryElementModel.ContentDataModelId;
            regionRequestArgs.LibraryElementType = type;
            regionRequestArgs.Title = "Region " + vm.HomeTabViewModel.LibraryElementController.Title; // TODO factor out this hard-coded string to a constant
            regionRequestArgs.ClippingParentLibraryId = vm.HomeTabViewModel.LibraryElementController.LibraryElementModel.LibraryElementId;
            regionRequestArgs.AccessType = 
            if (vm.MetadataToAddUponBeingFullRegion != null)
            {
                //add the metadata to the creation request
                regionRequestArgs.Metadata = vm.MetadataToAddUponBeingFullRegion;
            }

            //create a request and send it to the server
            var request = new CreateNewLibraryElementRequest(regionRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedLibraryElementToLibrary();

            //this then removes the temporary region
            vm.RectangleWrapper.RemoveTemporaryRegion(vm);
        }
    }
}