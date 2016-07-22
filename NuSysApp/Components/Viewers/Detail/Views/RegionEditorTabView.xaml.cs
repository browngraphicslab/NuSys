
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
            Canvas.SetZIndex(xButtonStack, 20);
        }

        private void AddRegion_Clicked(object sender, RoutedEventArgs e)
        {
            var vm = DetailViewerView.DataContext as DetailViewerViewModel;
            if (vm == null)
            {
                return;
            }
            var detailHomeTabViewModel = vm.RegionView.DataContext as DetailHomeTabViewModel;
            Message message = null;
            ElementType type = ElementType.ImageRegion;
            switch (vm.CurrentElementController.LibraryElementModel.Type)
            {
                case ElementType.Image:
                    message = detailHomeTabViewModel?.GetNewRegionMessage();
                    type = ElementType.ImageRegion;
                    break;
                case ElementType.Audio:
                    message = detailHomeTabViewModel?.GetNewRegionMessage();
                    type = ElementType.AudioRegion;
                    break;
                case ElementType.Video:
                    message = detailHomeTabViewModel?.GetNewRegionMessage();
                    type  = ElementType.VideoRegion;
                    break;
                case ElementType.PDF:
                    message = detailHomeTabViewModel?.GetNewRegionMessage();
                    type = ElementType.PdfRegion;
                    break;
                default:
                    message = null;
                    break;
            }
            Debug.Assert(message != null);
            message["id"] = SessionController.Instance.GenerateId();
            message["title"] = "Region " + vm.CurrentElementController.Title;
            message["type"] = type.ToString();
            message["clipping_parent_library_id"] = vm.CurrentElementController.LibraryElementModel.LibraryElementId;
            message["server_url"] = vm.CurrentElementController.LibraryElementModel.ServerUrl;
            var request = new CreateNewLibraryElementRequest(message);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }

        public void ShowListView(bool visible, ElementType type)
        {
            if (!visible)
            {
                xListViewPresenter.Content = null;
                if (xMainGrid.ColumnDefinitions.Contains(xSecondColumn))
                {
                    xMainGrid.ColumnDefinitions.Remove(xSecondColumn);
                }
                return;
            }
            else
            {
                if (type == ElementType.PDF)
                {
                    xListViewPresenter.Content = new PDFRegionListView(DetailViewerView);
                    if (!xMainGrid.ColumnDefinitions.Contains(xSecondColumn))
                    {
                        xMainGrid.ColumnDefinitions.Add(xSecondColumn);
                    }
                    return;
                }
            }
        }
    }
}
