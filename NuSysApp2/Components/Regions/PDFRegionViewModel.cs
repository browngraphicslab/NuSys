using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class PdfRegionViewModel : RegionViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public double ContainerHeight { get; set; }
        public double ContainerWidth { get; set; }
        private double _height;
        private double _width;

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                //Model.Name = _name;
                RaisePropertyChanged("Name");
            }
        }
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

        public double OriginalHeight { get; set; }
        public double OriginalWidth { get; set; }


        public delegate void SizeChangedEventHandler(object sender, double height, double width);
        public event SizeChangedEventHandler SizeChanged;

        public delegate void LocationChangedEventHandler(object sender, Point topLeft);
        public event LocationChangedEventHandler LocationChanged;
        public PdfRegionViewModel(PdfRegion model, PdfRegionController controller, Sizeable sizeable) : base(model, controller,sizeable)
        {
            if (model == null)
            {
                return;
            }

            ContainerSizeChanged += BaseSizeChanged;
            ContainerHeight = sizeable.GetHeight();
            ContainerWidth = sizeable.GetWidth();
            Height = model.Height * ContainerHeight;
            Width = model.Width * ContainerWidth;
            //LibraryElementController.RegionUpdated += RegionUpdated;


            controller.SizeChanged += RegionController_SizeChanged;
            controller.LocationChanged += RegionController_LocationChanged;
            controller.TitleChanged += RegionController_TitleChanged;


            Name = model.Title;

            Editable = true;

        }

        private void RegionController_TitleChanged(object source, string title)
        {
            Name = title;
        }

        private void RegionController_LocationChanged(object sender, Point topLeft)
        {
            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }


            var denormalizedTopLeft = new Point(model.TopLeftPoint.X * ContainerViewModel.GetWidth(), model.TopLeftPoint.Y * ContainerViewModel.GetHeight());

            LocationChanged?.Invoke(this, denormalizedTopLeft);
        }

        private void RegionController_SizeChanged(object sender, double width, double height)
        {
            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }

            Height = model.Height * ContainerViewModel.GetHeight();
            Width = model.Width * ContainerViewModel.GetWidth();
            SizeChanged?.Invoke(this, Width, Height);
        }
        /*
        private void RegionUpdated(object source, Region region)
        {
            if (region.LibraryId != Model.LibraryId)
            {
                return;
            }
            var model = Model as PdfRegion;
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
        */
        private void BaseSizeChanged(object sender, double width, double height)
        {

            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }


            //Width and height passed in are the width and height of PDF itself.
            Height = model.Height * height;
            Width = model.Width * width;
            ContainerHeight = height;
            ContainerWidth = width;
            var topLeft = new Point(model.TopLeftPoint.X * width, model.TopLeftPoint.Y * height);

            SizeChanged?.Invoke(this, Width, Height);
            LocationChanged?.Invoke(this, topLeft);
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
            RaisePropertyChanged("ContainerHeight");
            RaisePropertyChanged("ContainerWidth");
        }

        public void SetNewPoints(Point topLeft, Point bottomRight)
        {
            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }
            var normalTopLeftX = topLeft.X / ContainerViewModel.GetWidth();
            var normalTopLeftY = topLeft.Y / ContainerViewModel.GetHeight();
            var normalBottomRightX = bottomRight.X / ContainerViewModel.GetWidth();
            var normalBottomRightY = bottomRight.Y / ContainerViewModel.GetHeight();

            var normalWidth = normalBottomRightX - normalTopLeftX;
            var normalHeight = normalBottomRightY - normalTopLeftY;

            //model.TopLeftPoint = new Point(normalTopLeftX, normalTopLeftY);
            //model.BottomRightPoint = new Point(normalBottomRightX, normalBottomRightY);
            //LibraryElementController.UpdateRegion(Model);
            var pdfRegionController = LibraryElementController as PdfRegionController;
            pdfRegionController?.SetLocation(new Point(normalTopLeftX, normalTopLeftY));
            pdfRegionController?.SetWidth(normalWidth);
            pdfRegionController?.SetHeight(normalHeight);
        }

        public void SetNewLocation(Point topLeft)
        {
            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }
            var normalTopLeftX = topLeft.X / ContainerViewModel.GetWidth();
            var normalTopLeftY = topLeft.Y / ContainerViewModel.GetHeight();

            model.TopLeftPoint = new Point(normalTopLeftX, normalTopLeftY);
            //model.BottomRightPoint = new Point(normalBottomRightX, normalBottomRightY);


            (LibraryElementController as PdfRegionController).SetLocation(model.TopLeftPoint);
        }

        public void SetNewSize(double width, double height)
        {
            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }
            var normalWidth = width / ContainerViewModel.GetWidth();
            var normalHeight = height / ContainerViewModel.GetHeight();



            (LibraryElementController as PdfRegionController).SetWidth(normalWidth);
            (LibraryElementController as PdfRegionController).SetWidth(normalHeight);
        }

        public void SetNewName(string text)
        {
            Name = text;
            LibraryElementController.SetTitle(Name);
        }
    }
}
