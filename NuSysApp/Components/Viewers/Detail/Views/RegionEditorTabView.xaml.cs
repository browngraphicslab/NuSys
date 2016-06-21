
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

        //public DetailViewerViewModel DetailViewerViewModel { set; get { return ((DetailViewerViewModel)DetailViewerView.DataContext).View} }
        private bool _edgeCaseButtonExited;
        public  RegionEditorTabView()

        {
            this.InitializeComponent();
            _edgeCaseButtonExited = true;
        }

        private void XListViewItemGrid_OnPointerEnterExit(object sender, PointerRoutedEventArgs e)
        {

        }

        private void KeyTextBox_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {

        }

        private void AddRegion_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = DetailViewerView.DataContext as DetailViewerViewModel;
            if (vm == null)
            {
                return;
            }
            Region region = null;
            switch (vm.CurrentElementController.LibraryElementModel.Type)
            {
                case ElementType.Image:
                    region = new RectangleRegion(new Point(.25,.25), new Point(.75,.75));
                    break;
                case ElementType.Audio:
                    region = new TimeRegionModel("name",0,1);
                    break;
                case ElementType.Video:
                    region = new VideoRegionModel(new Point(0.25,0.25),new Point(0.75,0.75),.25,.75  );
                    break;
                case ElementType.Collection:

                    break;
                case ElementType.Text:

                    break;
                case ElementType.PDF:
                    region = new PdfRegion(new Point(.25, .25), new Point(.75, .75), 1);
                    break;
                default:
                    region = null;
                    break;
            }
            
            vm.CurrentElementController.AddRegion(region);
        }

        private void DeleteButton_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.Visibility = Visibility.Collapsed;
            _edgeCaseButtonExited = true;

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
