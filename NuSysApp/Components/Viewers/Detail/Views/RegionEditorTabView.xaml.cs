
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using MyToolkit.UI;
using System.Threading.Tasks;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class RegionEditorTabView : UserControl
    {
        public DetailViewerView DetailViewerView { set; get; }

        public ObservableCollection<Region> RegionCollection { set; get; }


        //public DetailViewerViewModel DetailViewerViewModel { set; get { return ((DetailViewerViewModel)DetailViewerView.DataContext).View} }
        private bool _edgeCaseButtonExited;
        public  RegionEditorTabView()

        {
            this.InitializeComponent();
            _edgeCaseButtonExited = true;
            RegionCollection = new ObservableCollection<Region>();
        }

        private void XListViewItemGrid_OnPointerEnterExit(object sender, PointerRoutedEventArgs e)
        {

        }

        private void KeyTextBox_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {

        }

        private void XDeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            xContentPresenter.Content = (((DetailViewerViewModel)DetailViewerView.DataContext).RegionView);
            Region region = null;
            if ((((DetailViewerView.DataContext as DetailViewerViewModel)?.RegionView as ImageDetailHomeTabView)) != null)
            {
                region = new RectangleRegion("new rectangle", new Point(0, 0), new Point(1, 1));
                ((ImageDetailHomeTabView) (((DetailViewerViewModel) DetailViewerView.DataContext).RegionView))
                    .DisplayRegion(region);
            }
            if ((((DetailViewerView.DataContext as DetailViewerViewModel)?.RegionView as AudioDetailHomeTabView)) != null)
            {
                region = new TimeRegionModel("new rectangle",0,1);
                ((AudioDetailHomeTabView) (((DetailViewerViewModel) DetailViewerView.DataContext).RegionView)).DisplayRegion(region);
            }
            if ((((DetailViewerView.DataContext as DetailViewerViewModel)?.RegionView as PdfDetailHomeTabView)) != null)
            {
                region = new PdfRegion("new rectangle", new Point(0, 0), new Point(1, 1), 1);
                (((DetailViewerView.DataContext as DetailViewerViewModel)?.RegionView as PdfDetailHomeTabView)).AddRegion(region as PdfRegion);
            }
            var width = ((DetailViewerViewModel)DetailViewerView.DataContext).RegionView.ActualWidth;
            var height = ((DetailViewerViewModel)DetailViewerView.DataContext).RegionView.ActualHeight;
            //TODO: make this rectangle's size dependent on the size of the region view

            region.Name = "Untitled Region";


            ((DetailViewerViewModel)DetailViewerView.DataContext).CurrentElementController.AddRegion(region);
            Update();

        }

        private void DeleteButton_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.Visibility = Visibility.Collapsed;
            _edgeCaseButtonExited = true;

        }

        public void Update()
        {
            var vm = (DetailViewerViewModel)DetailViewerView.DataContext;

            //xLeftRegionPanel.Content = vm.View;
            xContentPresenter.Content = vm.RegionView;

            RegionCollection.Clear();
            if (vm?.CurrentElementController?.LibraryElementModel?.Regions == null)
            {
                return;
            }
            foreach (var region in vm.CurrentElementController.LibraryElementModel.Regions)
            {
                RegionCollection.Add(region);
            }
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {

        }

        private void xCreateReagionButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RegionListViewItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //((ImageFullScreenView)(((DetailViewerViewModel)DetailViewerView.DataContext).RegionView)).SelectedRegion(test);

        }
    }
}
