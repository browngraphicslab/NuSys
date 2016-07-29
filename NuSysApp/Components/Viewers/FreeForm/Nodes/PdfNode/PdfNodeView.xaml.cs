using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfNodeView : AnimatableUserControl, IThumbnailable, Sizeable
    {
        private PdfNodeViewModel _vm;

        public PdfNodeView(PdfNodeViewModel vm)
        {
            _vm = vm;
            vm.View = this;
            InitializeComponent();
            //  IsDoubleTapEnabled = true;
            DataContext = vm;

            vm.Controller.Disposed += ControllerOnDisposed;
            SizeChanged += PdfNodeView_SizeChanged;

            Loaded += PdfNodeView_Loaded;

        }

        private async void UpdateRegionViews(int currentPageNumber)
        {
            foreach (var item in xClippingWrapper.GetRegionItems())
            {
                var regionView = item as PDFRegionView;
                var model = (regionView?.DataContext as PdfRegionViewModel)?.Model as PdfRegionModel;
                Debug.Assert(regionView != null);
                await UITask.Run(() =>
                {
                    regionView.Visibility = model?.PageLocation == currentPageNumber ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }

        private async void PdfNodeView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PdfNodeViewModel;
            xClippingWrapper.Controller = vm?.Controller.LibraryElementController;
            await xClippingWrapper.ProcessLibraryElementController();
            UpdateRegionViews(vm.CurrentPageNumber);


            //vm?.CreatePdfRegionViews();
        }

        private void PdfNodeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // ?
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (PdfNodeViewModel) DataContext;
            nodeTpl.Dispose();
            vm.Controller.Disposed -= ControllerOnDisposed;
        }


        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            //  nodeTpl.ToggleInkMode();

        }

        private async void OnPageLeftClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel) this.DataContext;
            await vm.FlipLeft();
            UpdateRegionViews(vm.CurrentPageNumber);


            //(nodeTpl.inkCanvas.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            e.Handled = true;

            // nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //nodeTpl.inkCanvas.ReRenderLines();

        }

        private async void OnPageRightClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel) this.DataContext;
            await vm.FlipRight();
            UpdateRegionViews(vm.CurrentPageNumber);
            //(nodeTpl.inkCanvas.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            e.Handled = true;

            //   nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //     nodeTpl.inkCanvas.ReRenderLines();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel) this.DataContext;
            vm.Controller.RequestDelete();
        }

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel) DataContext;
            vm.Controller.RequestDuplicate(vm.Model.X, vm.Model.Y);
        }

        private void PageRight_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(xRenderedPdf, width, height);
            return r;
        }
       
        public double GetWidth()
        {
            return xRenderedPdf.ActualWidth;
        }

        public double GetHeight()
        {
            return xRenderedPdf.ActualHeight;
        }

    }
}
