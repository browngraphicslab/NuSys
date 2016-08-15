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

        public TemporaryImageRegionView(ImageRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.Deselect();
            var model = vm.Model as RectangleRegion;
            if (model == null)
            {
                return;
            }

            CompositeTransform composite = new CompositeTransform();
            this.RenderTransform = composite;

            vm.LocationChanged += ChangeLocation;

            var parentWidth = vm.RectangleWrapper.GetWidth();
            var parentHeight = vm.RectangleWrapper.GetHeight();

            composite.TranslateX = model.TopLeftPoint.X * parentWidth;
            composite.TranslateY = model.TopLeftPoint.Y * parentHeight;
            vm.Width = (model.Width) * parentWidth;
            vm.Height = (model.Height) * parentHeight;
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

            var vm = DataContext as ImageRegionViewModel;

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
            var vm = DataContext as ImageRegionViewModel;

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
            var vm = DataContext as ImageRegionViewModel;
            vm.Disposed -= Dispose;
            vm.LocationChanged -= ChangeLocation;
        }
    }
}