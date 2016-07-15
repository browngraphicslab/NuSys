using System;
using System.Collections.Generic;
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
        private bool _toggleManipulation;
        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;
        public delegate void OnRegionSeekHandler(double time);
        public event OnRegionSeekHandler OnRegionSeek;
        public bool Selected { get; set; }


        private bool _isSingleTap;

        public AudioRegionView(AudioRegionViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.Deselect();
            _toggleManipulation = false;


        }
        private void Handle_OnPointerPressed(object sender, PointerRoutedEventArgs e)
       {
            _toggleManipulation = true;
        }

        private void Bound1_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = this.DataContext as AudioRegionViewModel;
            if (Rect.Width + e.Delta.Translation.X > 0 && vm.LeftHandleX + e.Delta.Translation.X > 0 && vm.LeftHandleX + e.Delta.Translation.X < vm.RightHandleX)
            {
                vm.SetNewPoints(e.Delta.Translation.X,0);
            }
        }

        private void Handle_OnManipulationDelta2(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = this.DataContext as AudioRegionViewModel;
            if (Rect.Width + e.Delta.Translation.X > 0 && vm.RightHandleX + e.Delta.Translation.X < vm.ContainerViewModel.GetWidth())
            {
                //            (Bound2.RenderTransform as CompositeTransform).TranslateX += e.Delta.Translation.X;
                //         Bound2.X2 += e.Delta.Translation.X;
                //        Bound2.X1 += e.Delta.Translation.X;
                //        Rect.Width += e.Delta.Translation.X;
                vm.SetNewPoints(0,e.Delta.Translation.X);
            }
        }

 
        private void Handle_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _toggleManipulation = false;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
           
        }


        public void Deselect()
        {
            Rect.Fill = new SolidColorBrush(Color.FromArgb(255, 219, 151, 179));
            xNameTextBox.Visibility = Visibility.Collapsed;
            Rect.IsHitTestVisible = true;
            xDelete.Visibility = Visibility.Collapsed;
            Selected = false;

        }

        public void Select()
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

        private void Rect_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DataContext as AudioRegionViewModel;
            /*
            if (!vm.Editable)
                return;

            if (Selected)
                this.Deselect();
            else
                this.Select();
                */
            e.Handled = true;

        }
        private void XGrid_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _isSingleTap = false;

            var vm = DataContext as RegionViewModel;
            var regionController = vm?.RegionController;
            SessionController.Instance.SessionView.ShowDetailView(regionController);
        }

        private void Rect_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = this.DataContext as AudioRegionViewModel;
            if (Rect.Width + e.Delta.Translation.X > 0 && vm.RightHandleX + e.Delta.Translation.X < vm.ContainerViewModel.GetWidth() && vm.LeftHandleX + e.Delta.Translation.X > 0)
            {

                vm.SetNewPoints(e.Delta.Translation.X, e.Delta.Translation.X);
            }
            e.Handled = true;
        }
        private void xNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = DataContext as AudioRegionViewModel;
            vm.Name = (sender as TextBox).Text;
            vm.RegionController.SetTitle(vm.Name);
        }

        private void xDelete_PointerPressed(object sender, PointerRoutedEventArgs e)
        {


            var vm = this.DataContext as AudioRegionViewModel;
            if (vm == null)
            {
                return;
            }

            var libraryElementController = vm.LibraryElementController;
            libraryElementController.RemoveRegion(vm.RegionController.Model);


        }
        private async void Rect_OnTapped(object sender, TappedRoutedEventArgs e)
        {

            // check to see if double tap gets called
            _isSingleTap = true;
            await Task.Delay(200);
            if (!_isSingleTap) return;


            if (!Selected)
            {
                OnRegionSeek?.Invoke(((DataContext as AudioRegionViewModel).RegionController.Model as TimeRegionModel).Start + 0.01);
            }

            e.Handled = true;
        }

    }
}
