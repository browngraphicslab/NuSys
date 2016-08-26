using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using SharpDX.Direct3D11;
using System.Threading.Tasks;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoRegionView : UserControl
    {
        private bool _isSingleTap;


        public delegate void RegionSelectedDeselectedEventHandler(object sender, bool selected);
        public event RegionSelectedDeselectedEventHandler OnSelectedOrDeselected;

        public delegate void OnRegionSeekHandler(double time);
        public event OnRegionSeekHandler OnRegionSeek;

        public Grid RegionRectangle
        {
            get { return xGrid; }
        }

        public bool Selected { get; set; }


        public VideoRegionView(VideoRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.Deselect();
        }
        private void Bound1_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var composite = IntervalRectangle.RenderTransform as CompositeTransform;
            var vm = DataContext as VideoRegionViewModel;
            if (composite != null &&  vm != null)
            {
                var newStart = composite.TranslateX + e.Delta.Translation.X * Bound1Transform.ScaleX;
                if (newStart < 0)
                {
                    newStart = 0;
                }
                if (newStart > vm.IntervalEnd)
                {
                    newStart = vm.IntervalEnd;
                }
                vm.SetIntervalStart(newStart);
            }
        }

        private void Bound2_OnManipulationDelta2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var composite = IntervalRectangle.RenderTransform as CompositeTransform;
            var vm = DataContext as VideoRegionViewModel;
            if (composite != null &&  vm != null)
            {
                var newEnd = composite.TranslateX + IntervalRectangle.Width + e.Delta.Translation.X * Bound2Transform.ScaleX;
                if (newEnd > vm.AudioWrapper.GetWidth())
                {
                    newEnd = vm.AudioWrapper.GetWidth();
                }
                if (newEnd < vm.IntervalStart)
                {
                    newEnd = vm.IntervalStart;
                }
                if (Double.IsNaN(newEnd))
                {
                    return;
                }
                vm.SetIntervalEnd(newEnd);
            }
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

            var vvm = vm.AudioWrapper;

            //double newWidth = xMainRectangle.Width;
            //double newHeight = xMainRectangle.Height;

            ////CHANGE IN WIDTH
            //if (xMainRectangle.Width + vm.TopLeft.X + e.Delta.Translation.X <= vvm.GetWidth())
            //{
            //    newWidth = Math.Max(xMainRectangle.Width + e.Delta.Translation.X, 25);

            //}
            ////CHANGE IN HEIGHT

            //if (xMainRectangle.Height + vm.TopLeft.Y + e.Delta.Translation.Y <= vvm.GetHeight())
            //{
            //    newHeight = Math.Max(xMainRectangle.Height + e.Delta.Translation.Y, 25);
            //}

            //vm.SetRegionSize(newWidth, newHeight);
            e.Handled = true;
        }
        private void xResizingTriangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }

        private void xResizingTriangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as VideoRegionViewModel;
            if (vm == null || GridTransform == null)
            {
                return;
            }

            // check that manipulation delta remains in the bounds
            if (
                
                vm.TopLeft.X + e.Delta.Translation.X < 0 ||
                vm.TopLeft.X + e.Delta.Translation.X + vm.RectangleWidth >
                vm.AudioWrapper.GetWidth() ||
                vm.TopLeft.Y + e.Delta.Translation.Y < 0 ||
                vm.TopLeft.Y + e.Delta.Translation.Y + vm.RectangleHeight >
                vm.AudioWrapper.GetHeight())
            {
                return;
            }
            


            vm.SetRegionLocation(new Point(GridTransform.TranslateX + e.Delta.Translation.X, GridTransform.TranslateY + e.Delta.Translation.Y));
            e.Handled = true;
        }


        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (!Selected)
            {
                this.Select();
                OnRegionSeek?.Invoke(((this.DataContext as VideoRegionViewModel).RegionLibraryElementController.LibraryElementModel as VideoRegionModel).Start);

            }
            e.Handled = true;
        }
        private void Deselect()
        {
            var vm = DataContext as VideoRegionViewModel;
            IntervalRectangle.Fill = new SolidColorBrush(Color.FromArgb(255, 219, 151, 179));
            //xResizingTriangle.Visibility = Visibility.Collapsed;
            xNameTextBox.Visibility = Visibility.Collapsed;
            xDelete.Visibility = Visibility.Collapsed;
            IntervalRectangle.IsHitTestVisible = true;

            Selected = false;


        }

        private void Select()
        {
            var vm = DataContext as VideoRegionViewModel;
            //xMainRectangle.StrokeThickness = 6;
            //xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.CadetBlue);
            IntervalRectangle.Fill = new SolidColorBrush(Color.FromArgb(255, 152, 26, 77));
            //xResizingTriangle.Visibility = Visibility.Visible;
            xNameTextBox.Visibility = Visibility.Visible;
            if (vm.Editable)
            {
                xDelete.Visibility = Visibility.Visible;
            }
            IntervalRectangle.IsHitTestVisible = false;


            Selected = true;

        }

        public void FireSelection()
        {
            if (!Selected)
            {
                Select();
                OnSelectedOrDeselected?.Invoke(this, true);
            }
        }

        public void FireDeselection()
        {
            if (Selected)
            {
                Deselect();
                OnSelectedOrDeselected?.Invoke(this, false);
            }
        }

        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DataContext as VideoRegionViewModel;
            /*
            if (!vm.Editable)
                return;

            if (Selected)
                this.Deselect();
            else
                this.Select();
                
            e.Handled = true;
            */
        }
        private void IntervalRectangle_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Bound1_OnManipulationDelta(sender,e);
            Bound2_OnManipulationDelta2(sender, e);
        }
        
        private void XGrid_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _isSingleTap = false;

            var vm = DataContext as RegionViewModel;
            var regionController = vm?.RegionLibraryElementController;
            SessionController.Instance.SessionView.ShowDetailView(regionController);
        }

        private void xNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = DataContext as VideoRegionViewModel;
            vm.Name = (sender as TextBox).Text;
            vm.RegionLibraryElementController.SetTitle(vm.Name);
        }

        private async void IntervalRectangle_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // check to see if double tap gets called
            _isSingleTap = true;
            await Task.Delay(200);
            if (!_isSingleTap) return;

            if (!Selected)
            {
                OnRegionSeek?.Invoke(((this.DataContext as VideoRegionViewModel).RegionLibraryElementController.LibraryElementModel as VideoRegionModel).Start);
            }

            FireSelection();
            e.Handled = true;

        }

        private void xDelete_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = this.DataContext as VideoRegionViewModel;
            if (vm == null)
            {
                return;
            }
           
            // delete all the references to this region from the library
            var removeRequest = new DeleteLibraryElementRequest(vm.RegionLibraryElementController.LibraryElementModel.LibraryElementId);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(removeRequest);

        }

        public void RescaleComponents(double scaleX)
        {
            Bound1Transform.ScaleX = 1.0 / scaleX;
            Bound2Transform.ScaleX = 1.0 / scaleX;
            xToolBarTransform.ScaleX = 1.0 / scaleX;
        }
    }
}
