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
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfNodeView : AnimatableUserControl, IThumbnailable
    {
        private PdfNodeViewModel _vm;

        public PdfNodeView(PdfNodeViewModel vm)
        {
            _vm = vm;
            InitializeComponent();
            DataContext = vm;
            vm.Controller.Disposed += ControllerOnDisposed;
            Loaded += PdfNodeView_Loaded;
        }

        /// <summary>
        /// Loads all the region views for the passed in page number and removes any other region views
        /// </summary>
        /// <param name="currentPageNumber"></param>
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

            // disable page left and page right buttons for pdf regions
            if (vm.Model.ElementType == NusysConstants.ElementType.PdfRegion)
            {
                pageLeft.Height = 0;
                pageLeft.Width = 0;
                pageRight.Height = 0;
                pageRight.Width = 0;

            }
        }

        private void ControllerOnDisposed(object source, object args)
        {
            nodeTpl.Dispose();
            var vm = (PdfNodeViewModel) DataContext;
            if (vm == null)
            {
                return;
            }
            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }

        private async void OnPageLeftClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel) this.DataContext;
            await vm.FlipLeft();
            UpdateRegionViews(vm.CurrentPageNumber);
            e.Handled = true;


        }

        private async void OnPageRightClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel) this.DataContext;
            await vm.FlipRight();
            UpdateRegionViews(vm.CurrentPageNumber);
            e.Handled = true;
        }



        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(xRenderedPdf, width, height);
            return r;
        }
    }
}
