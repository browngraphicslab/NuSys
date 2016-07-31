
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
using NusysIntermediate;

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

            // get appropriate new region message based on the type of library element currently
            // loaded in the detail view. i.e. for images top left point, width, and height.
            var detailHomeTabViewModel = vm.RegionView.DataContext as DetailHomeTabViewModel;
            Message message = null;
            NusysConstants.ElementType type;
            switch (vm.CurrentElementController.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.ImageRegion:
                case NusysConstants.ElementType.Image:

                    message = detailHomeTabViewModel?.GetNewRegionMessage();
                    type = NusysConstants.ElementType.ImageRegion;
                    break;
                case NusysConstants.ElementType.AudioRegion:
                case NusysConstants.ElementType.Audio:

                    message = detailHomeTabViewModel?.GetNewRegionMessage();
                    type = NusysConstants.ElementType.AudioRegion;
                    break;
                case NusysConstants.ElementType.VideoRegion:
                case NusysConstants.ElementType.Video:
                    message = detailHomeTabViewModel?.GetNewRegionMessage();
                    type  = NusysConstants.ElementType.VideoRegion;
                    break;
                case NusysConstants.ElementType.PdfRegion:
                case NusysConstants.ElementType.PDF:
                    message = detailHomeTabViewModel?.GetNewRegionMessage();
                    type = NusysConstants.ElementType.PdfRegion;
                    break;
                default:
                    Debug.Fail("This should never occur, if it does we just return to be safe but this is a massive bug");
                    return;
            }
            Debug.Assert(message != null);

            // Add universal data to the message, should be self explanatory, unpacked in the rectangleRegionController
            message["id"] = SessionController.Instance.GenerateId();
            message["title"] = "Region " + vm.CurrentElementController.Title;
            message["content__id"] = vm.CurrentElementController.LibraryElementModel.ContentDataModelId;
            message["type"] = type.ToString();
            message["clipping_parent_library_id"] = vm.CurrentElementController.LibraryElementModel.LibraryElementId;
            if (vm.CurrentElementController.LibraryElementModel.ServerUrl != null)
            {
                message["server_url"] = vm.CurrentElementController.LibraryElementModel.ServerUrl;
            }
            var request = new CreateNewLibraryElementRequest(message);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }

        public void ShowListView(bool visible, NusysConstants.ElementType type)
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
                if (type == NusysConstants.ElementType.PDF)
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
