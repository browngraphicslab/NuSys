
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

        private void DeleteRegion_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = DetailViewerView.DataContext as DetailViewerViewModel;
            if (vm == null)
            {
                return;
            }
            var button = sender as Button;
            var region = button.DataContext as Region;

            vm.CurrentElementController.RemoveRegion(region);
            
        }
        private void AddRegion_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = DetailViewerView.DataContext as DetailViewerViewModel;
            if (vm == null)
            {
                return;
            }
            var detailHomeTabViewModel = vm.RegionView.DataContext as DetailHomeTabViewModel;
            Region region = null;
            switch (vm.CurrentElementController.LibraryElementModel.Type)
            {
                case ElementType.Image:
                    region = detailHomeTabViewModel?.GetNewRegion();
                    break;
                case ElementType.Audio:
                    region = detailHomeTabViewModel?.GetNewRegion();
                    break;
                case ElementType.Video:
                    region = detailHomeTabViewModel?.GetNewRegion();
                    break;
                case ElementType.Collection:

                    break;
                case ElementType.Text:

                    break;
                case ElementType.PDF:
                    region = detailHomeTabViewModel?.GetNewRegion();
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
        
        private void RegionListViewItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //((ImageFullScreenView)(((DetailViewerViewModel)DetailViewerView.DataContext).RegionView)).SelectedRegion(test);

        }
    }
}
