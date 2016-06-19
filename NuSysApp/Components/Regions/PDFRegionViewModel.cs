using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class PdfRegionViewModel : RegionViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public PdfRegion Model { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        private LibraryElementController _elementController;

        public PdfRegionViewModel(PdfRegion model, LibraryElementController elementController, Sizeable sizeable) : base(model, elementController, sizeable)
        {
            Model = model;
            _elementController = elementController;
            ContainerSizeChanged += BaseSizeChanged;
            Height = (model.BottomRightPoint.Y*sizeable.GetHeight()) - (model.TopLeftPoint.Y*sizeable.GetHeight());
            Width = (model.BottomRightPoint.X * sizeable.GetWidth()) - (model.TopLeftPoint.X * sizeable.GetWidth());

        }

        private void BaseSizeChanged(object sender, double width, double height)
        {
            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }
            Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y) * height;
            Width = (model.BottomRightPoint.X - model.BottomRightPoint.X) * width;
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }

    }
}
