using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

        public ObservableCollection<PdfRegionModel> RegionModelList { get; set; }

        private ContentDataModel _contentDataModel;

        public PDFRegionListView(DetailViewerView dvv)
        {
            DataContext = this;
            DetailViewerView = dvv;
            RegionModelList = new ObservableCollection<PdfRegionModel>();

            // populate the region model list with all the regions for the given content
            var contentDataModelId = (dvv.DataContext as DetailViewerViewModel).CurrentElementController.LibraryElementModel.ContentDataModelId;
            PopulateRegionModelList(contentDataModelId);

            // add events so that the list is live updated
            _contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(contentDataModelId);
            _contentDataModel.OnRegionAdded += ContentDataModel_OnRegionAdded;
            _contentDataModel.OnRegionRemoved += ContentDataModel_OnRegionRemoved;

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed;

            this.InitializeComponent();
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            _contentDataModel.OnRegionAdded -= ContentDataModel_OnRegionAdded;
            _contentDataModel.OnRegionRemoved -= ContentDataModel_OnRegionRemoved;
            _contentDataModel = null;
        }

        private async void ContentDataModel_OnRegionRemoved(string regionLibraryElementModelId)
        {
            foreach (var model in RegionModelList)
            {
                if (model.LibraryElementId == regionLibraryElementModelId)
                {
                    RegionModelList.Remove(model);

                    break;
                }
            }
        }

        private async Task ContentDataModel_OnRegionAdded(string regionLibraryElementModelId)
        {
            var region = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementModelId);
            var model = region.LibraryElementModel as PdfRegionModel;
            Debug.Assert(model != null);
            RegionModelList.Add(model);
        }

        private void PopulateRegionModelList(string contentDataModelId)
        {
            var regionIds = SessionController.Instance.RegionsController.GetContentDataModelRegionLibraryElementIds(contentDataModelId);
            foreach (var regionId in regionIds)
            {
                var region = SessionController.Instance.ContentController.GetLibraryElementController(regionId);
                var model = region.LibraryElementModel as PdfRegionModel;
                Debug.Assert(model != null);
                RegionModelList.Add(model);
            }
        }

        private async void RegionListViewItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DetailViewerView.DataContext as DetailViewerViewModel;
            if (vm == null)
            {
                return;
            }
            var detailHomeTabViewModel = vm.RegionView.DataContext as PdfDetailHomeTabViewModel;
            var pdfRegion = (sender as Grid).DataContext as PdfRegionModel;

            await detailHomeTabViewModel.Goto(pdfRegion.PageLocation, pdfRegion);


        }       
    }
}
