using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class ImageRegionViewModel : RegionViewModel
    {
        public double Height { get; set; }
        public double Width{ get; set; }
        public ImageRegionViewModel(RectangleRegion model, LibraryElementController controller) : base(model,controller)
        {
            ContainerSizeChanged += BaseSizeChanged;
        }
        private void BaseSizeChanged(object sender, double width, double height)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y)*height;
            Width = (model.BottomRightPoint.X - model.TopLeftPoint.X)*width;
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }
    }
}
