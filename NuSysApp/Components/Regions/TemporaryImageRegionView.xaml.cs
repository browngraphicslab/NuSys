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
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    /// <summary>
    /// View for region on image content.
    /// </summary>
    public sealed partial class TemporaryImageRegionView : UserControl
    {

        public Point _topLeft;
        private double _tx;
        private double _ty;

        public bool Selected { private set; get; }

        public delegate void RegionSelectedDeselectedEventHandler(object sender, bool selected);
        public event RegionSelectedDeselectedEventHandler OnSelectedOrDeselected;

        public TemporaryImageRegionView(TemporaryImageRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.Deselect();


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

            _tx = composite.TranslateX;
            _ty = composite.TranslateY;
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



        public void Deselect()
        {
            xMainRectangleBorder.BorderThickness = new Thickness(3 * GridTranform.ScaleY, 3 * GridTranform.ScaleX, 3 * GridTranform.ScaleY, 3 * GridTranform.ScaleX);

            Selected = false;
        }

        public void Select()
        {
            xMainRectangleBorder.BorderThickness = new Thickness(6 * GridTranform.ScaleY, 6 * GridTranform.ScaleX, 6 * GridTranform.ScaleY, 6 * GridTranform.ScaleX);
            Selected = true;

        }
        /// <summary>
        ///         If not already selected, shows selection and fires event listened to by RectangleWrapper
        /// </summary>
        public void FireSelection()
        {
            if (!Selected)
            {
                Select();
                OnSelectedOrDeselected?.Invoke(this, true);
            }
        }

        /// <summary>
        /// If selected, shows deselection and fires event listened to by RectangleWrapper.
        /// </summary>
        public void FireDeselection()
        {
            if (Selected)
            {
                Deselect();
                OnSelectedOrDeselected?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Calls FireDeselection or FireSelection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DataContext as TemporaryImageRegionViewModel;

            if (!vm.Editable)
                return;

            if (Selected)
            {
                FireDeselection();
            }
            else
            {
                FireSelection();
            }

        }


        public void RescaleComponents(double scaleX, double scaleY)
        {
            /// How this works
            /// We scale the entire region based on the image being scaled. But we then want to invert the scaling on the visual components, 
            /// but not on the size of the region as a whole. To revert the scale, we divide the transforms by their current scale. using scaleX = 1/scaleX etc.
            /// we then shift the transforms over by certain margins. The math is simple even though the numbers look like "magic numbers."
            /// 
            /// The width of the rectangle borders is 3. The size of the delete button and resizing triangle is 25. So these magic numbers are simply
            /// the result of shifting things over by values relative to 25 and 3.

            //xMainRectangle.StrokeThickness = 3 / scaleX;
            xMainRectangleBorder.BorderThickness = new Thickness(3 / scaleX, 3 / scaleY, 3 / scaleX, 3 / scaleY);
        }

        public void Dispose(object sender, EventArgs e)
        {
            var vm = DataContext as TemporaryImageRegionViewModel;
            vm.Disposed -= Dispose;
            vm.LocationChanged -= ChangeLocation;
            xMainRectangleBorder.Tapped -= xMainRectangleBorder_Tapped;
        }

        private async void xMainRectangleBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //We create a request using the detail view's create args function
            var vm = this.DataContext as TemporaryImageRegionViewModel;
            var regionRequestArgs = vm.HomeTabViewModel?.GetNewCreateLibraryElementRequestArgs() as CreateNewRectangleRegionLibraryElementRequestArgs;
            var type = NusysConstants.ElementType.ImageRegion;
            // We need to then populate our new figures into it
            regionRequestArgs.RegionHeight = vm.NormalizedHeight;
            regionRequestArgs.RegionWidth = vm.NormalizedWidth;
            regionRequestArgs.TopLeftPoint = new PointModel(vm.NormalizedTopLeftPoint.X,vm.NormalizedTopLeftPoint.Y);
            // this is the rest of what's left to do to make this
            regionRequestArgs.ContentId = vm.HomeTabViewModel.LibraryElementController.LibraryElementModel.ContentDataModelId;
            regionRequestArgs.LibraryElementType = type;
            regionRequestArgs.Title = "Region " + vm.HomeTabViewModel.LibraryElementController.Title; // TODO factor out this hard-coded string to a constant
            regionRequestArgs.ClippingParentLibraryId = vm.HomeTabViewModel.LibraryElementController.LibraryElementModel.LibraryElementId;
            //create a request and send it to the server
            var request = new CreateNewLibraryElementRequest(regionRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedLibraryElementToLibrary();
            //this then removes the temporary region
            vm.RectangleWrapper.RemoveTemporaryRegion(vm);
        }

        private void XGrid_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }

        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

        }
    }
}