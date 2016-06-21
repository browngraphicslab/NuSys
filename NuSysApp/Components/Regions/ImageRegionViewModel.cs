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
        public bool Editable
        {
            set
            {

                _editable = value;

                RaisePropertyChanged("Editable");
            }
            get
            {
                return _editable;
            }
        }

        private bool _editable;
        public delegate void SizeChangedEventHandler(object sender, Point topLeft, Point bottomRight);
        public event SizeChangedEventHandler SizeChanged;

        public delegate void RegionUpdatedEventHandler(object sender, double height, double width);

        public event RegionUpdatedEventHandler RegionChanged;

        public ImageRegionViewModel(RectangleRegion model, LibraryElementController controller, Sizeable sizeable) : base(model,controller,sizeable)
        {
            ContainerSizeChanged += BaseSizeChanged;
            Height = (model.BottomRightPoint.Y * sizeable.GetHeight()) - (model.TopLeftPoint.Y * sizeable.GetHeight());
            Width = (model.BottomRightPoint.X * sizeable.GetWidth()) - (model.TopLeftPoint.X * sizeable.GetWidth());
            Editable = true;
            controller.RegionUpdated += Controller_RegionUpdated;

        }

        private void Controller_RegionUpdated(object source, Region region)
        {
            if (region.Id != Model.Id)
            {
                return;
            }
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            Height = model.BottomRightPoint.Y * ContainerViewModel.GetHeight() - model.TopLeftPoint.Y * ContainerViewModel.GetHeight();
            Width = model.BottomRightPoint.X * ContainerViewModel.GetWidth() - model.TopLeftPoint.X * ContainerViewModel.GetWidth();


            RegionChanged?.Invoke(this, Height, Width);
            var topLeft = new Point(model.TopLeftPoint.X * ContainerViewModel.GetWidth(), model.TopLeftPoint.Y * ContainerViewModel.GetHeight());
            var bottomRight = new Point(model.BottomRightPoint.X * ContainerViewModel.GetWidth(), model.BottomRightPoint.Y * ContainerViewModel.GetHeight());
            SizeChanged?.Invoke(this, topLeft, bottomRight);


            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");

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

            var topLeft = new Point(model.TopLeftPoint.X * width, model.TopLeftPoint.Y*height);
            var bottomRight = new Point(model.BottomRightPoint.X * width, model.BottomRightPoint.Y * height);
            SizeChanged?.Invoke(this,topLeft,bottomRight);

            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }
        
        public void SetNewPoints(Point topLeft, Point bottomRight)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var normalTopLeftX = topLeft.X / ContainerViewModel.GetWidth();
            var normalTopLeftY = topLeft.Y / ContainerViewModel.GetHeight();
            var normalBottomRightX = bottomRight.X / ContainerViewModel.GetWidth();
            var normalBottomRightY = bottomRight.Y / ContainerViewModel.GetHeight();

            model.TopLeftPoint = new Point(normalTopLeftX, normalTopLeftY);
            model.BottomRightPoint = new Point(normalBottomRightX, normalBottomRightY);
            Controller.UpdateRegion(Model);
        }


    }
}
