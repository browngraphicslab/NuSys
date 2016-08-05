using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioRegionView
    {

        public delegate void RegionSelectedDeselectedEventHandler(object sender, bool selected);
        public event RegionSelectedDeselectedEventHandler OnSelectedOrDeselected;

        public delegate void OnRegionSeekHandler(double time);
        public event OnRegionSeekHandler OnRegionSeek;
        public bool Selected { get; set; }


        private bool _isSingleTap;

        public AudioRegionView(AudioRegionViewModel vm)
        {
            this.DataContext = vm;
            this.InitializeComponent();
            this.Deselect();
            vm.Disposed += Dispose;
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

        private void Bound1_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = this.DataContext as AudioRegionViewModel;
            if (Rect.Width + e.Delta.Translation.X * Bound1Transform.ScaleX > 0 && vm.LeftHandleX + e.Delta.Translation.X * Bound1Transform.ScaleX > 0 && vm.LeftHandleX + e.Delta.Translation.X * Bound1Transform.ScaleX < vm.RightHandleX)
            {
                vm.SetNewPoints(e.Delta.Translation.X * Bound1Transform.ScaleX, 0);
            }
        }

        private void Bound2_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = this.DataContext as AudioRegionViewModel;
            if (Rect.Width + e.Delta.Translation.X * Bound2Transform.ScaleX > 0 && vm.RightHandleX + e.Delta.Translation.X * Bound2Transform.ScaleX < vm.AudioWrapper.ActualWidth)
            {
                vm.SetNewPoints(0,e.Delta.Translation.X * Bound2Transform.ScaleX);
            }
        }

        private void Deselect()
        {
            Rect.Fill = new SolidColorBrush(Color.FromArgb(255, 219, 151, 179));
            xNameTextBox.Visibility = Visibility.Collapsed;
            Rect.IsHitTestVisible = true;
            xDelete.Visibility = Visibility.Collapsed;
            Selected = false;
        }

        private void Select()
        {
            var vm = DataContext as AudioRegionViewModel;
            Rect.Fill = new SolidColorBrush(Color.FromArgb(255, 152, 26, 77));
            xNameTextBox.Visibility = Visibility.Visible;
            Rect.IsHitTestVisible = false;
            if (vm.Editable)
            {
                xDelete.Visibility = Visibility.Visible;
            }
            Selected = true;
        }

        private void XGrid_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _isSingleTap = false;

            var vm = DataContext as RegionViewModel;
            var regionController = vm?.RegionLibraryElementController;
            SessionController.Instance.SessionView.ShowDetailView(regionController);
        }

        private void Rect_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = this.DataContext as AudioRegionViewModel;
            if (Rect.Width + e.Delta.Translation.X * Bound2Transform.ScaleX > 0 && vm.RightHandleX + e.Delta.Translation.X * Bound2Transform.ScaleX < vm.AudioWrapper.ActualWidth && vm.LeftHandleX + e.Delta.Translation.X * Bound2Transform.ScaleX > 0)
            {

                vm.SetNewPoints(e.Delta.Translation.X * Bound2Transform.ScaleX, e.Delta.Translation.X * Bound2Transform.ScaleX);
            }
            e.Handled = true;
        }
        private void xNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = DataContext as AudioRegionViewModel;
            vm.Name = (sender as TextBox).Text;
            vm.RegionLibraryElementController.SetTitle(vm.Name);
        }

        private void xDelete_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = this.DataContext as AudioRegionViewModel;
            if (vm == null)
            {
                return;
            }
            // delete all the references to this region from the library
            var removeRequest = new DeleteLibraryElementRequest(vm.RegionLibraryElementController.LibraryElementModel.LibraryElementId);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(removeRequest);
            // If the region is deleted, it needs to dispose of its handlers.
            vm.Dispose(this, EventArgs.Empty);


        }
        private async void Rect_OnTapped(object sender, TappedRoutedEventArgs e)
        {

            // check to see if double tap gets called
            _isSingleTap = true;
            await Task.Delay(200);
            if (!_isSingleTap) return;

            if (!Selected)
            {
                OnRegionSeek?.Invoke(((DataContext as AudioRegionViewModel).RegionLibraryElementController.LibraryElementModel as AudioRegionModel).Start);
            }
            FireSelection();
            e.Handled = true;
        }

        public void RescaleComponents(double scaleX)
        {
            Bound1Transform.ScaleX = 1.0 / scaleX;
            Bound2Transform.ScaleX = 1.0 / scaleX;
            xToolBarTransform.ScaleX = 1.0 / scaleX;
        }

        public void Dispose(object sender, EventArgs e)
        {
            var vm = DataContext as AudioRegionViewModel;
            vm.Disposed -= Dispose;
            OnRegionSeek -= vm.AudioWrapper.AudioWrapper_OnRegionSeek;
        }
        
    }
}
