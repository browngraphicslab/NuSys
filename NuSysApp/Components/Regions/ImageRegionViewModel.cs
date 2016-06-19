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
        public double Width { get; set; }
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

        public void Translate(double xPercent, double yPercent)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var newTopLeftX = model.TopLeftPoint.X * xPercent;
            var newTopLeftY = model.TopLeftPoint.Y * yPercent;
            var newBottomRightX = model.BottomRightPoint.X * xPercent;
            var newBottomRightY = model.BottomRightPoint.Y * yPercent;
            
            model.TopLeftPoint = new Point(newTopLeftX,newTopLeftY);
            model.BottomRightPoint = new Point(newBottomRightX, newBottomRightY);
        }
        public void Resize(double widthPercent, double heightPercent)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var newBottomRightX = model.BottomRightPoint.X * widthPercent;
            var newBottomRightY = model.BottomRightPoint.Y * heightPercent;


            model.BottomRightPoint = new Point(newBottomRightX, newBottomRightY);
        }
    }
}
