using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class PdfRegionViewModel : BaseINPC
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public PdfRegion Model { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        private LibraryElementController _elementController;

        public PdfRegionViewModel(PdfRegion model, LibraryElementController elementController)
        {
            Model = model;
            _elementController = elementController;
        }

        public void ResizeRegion(Point p1, Point p2)
        {
            _elementController.AddRegion(new PdfRegion("unNamedRegion", p1, p2, Model.PageLocation));
            _elementController.RemoveRegion(Model);
        }

    }
}
