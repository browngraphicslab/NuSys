using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class PdfDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public LibraryElementModel Model { get; }
        public ObservableCollection<PDFRegionView> RegionViews { set; get; }
        public PdfDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;

            RegionViews = new ObservableCollection<PDFRegionView>();

            if (Model.Regions.Count > 0)
            {
                foreach (var region in Model.Regions)
                {
                    RegionViews.Add(new PDFRegionView(new PdfRegionViewModel(region as PdfRegion, Controller)));
                }
            }
            
        }
    }
}
