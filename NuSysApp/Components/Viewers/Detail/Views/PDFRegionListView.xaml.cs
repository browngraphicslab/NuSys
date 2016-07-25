using System;
using System.Collections.Generic;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PDFRegionListView : UserControl
    {
        public DetailViewerView DetailViewerView { set; get; }
        public PDFRegionListView(DetailViewerView dvv)
        {
            DetailViewerView = dvv;
            this.InitializeComponent();
        }

        private void XListViewItemGrid_OnPointerEnterExit(object sender, PointerRoutedEventArgs e)
        {

        }


        private async void RegionListViewItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //((ImageFullScreenView)(((DetailViewerViewModel)DetailViewerView.DataContext).RegionView)).SelectedRegion(test);

            var vm = DetailViewerView.DataContext as DetailViewerViewModel;
            if (vm == null)
            {
                return;
            }
            var detailHomeTabViewModel = vm.RegionView.DataContext as PdfDetailHomeTabViewModel;
            var pdfRegion = (sender as Grid).DataContext as PdfRegionModel;

            await detailHomeTabViewModel.Goto(pdfRegion.PageLocation, pdfRegion);


        }

        private void DeleteButton_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.Visibility = Visibility.Collapsed;

        }
        
    }
}
