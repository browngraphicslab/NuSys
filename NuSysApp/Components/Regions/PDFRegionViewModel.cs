using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class PdfRegionViewModel : RegionViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public double ContainerHeight { get; set; }
        public double ContainerWidth { get; set; }
        public RectangleWrapper RectangleWrapper { get; private set; }
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
        public PdfRegionViewModel(PdfRegionModel model, PdfRegionLibraryElementController regionLibraryElementController, RectangleWrapper wrapper) : base(model, regionLibraryElementController, null)
        {
            if (model == null)
            {
                return;
            }

            ContainerSizeChanged += BaseSizeChanged;
            ContainerHeight = wrapper.GetHeight();
            ContainerWidth = wrapper.GetWidth();
            Height = model.Height * ContainerHeight;
            Width = model.Width * ContainerWidth;
            RectangleWrapper = wrapper;
            RectangleWrapper.SizeChanged += RectangleWrapper_SizeChanged;

            //RegionLibraryElementController.RegionUpdated += RegionUpdated;


            regionLibraryElementController.SizeChanged += RegionController_SizeChanged;
            regionLibraryElementController.LocationChanged += RegionController_LocationChanged;
            regionLibraryElementController.TitleChanged += RegionController_TitleChanged;


            Name = Model.Title;

            Editable = true;

        }

        private void RectangleWrapper_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var model = this.RegionLibraryElementController.LibraryElementModel as RectangleRegion;
            var containerHeight = RectangleWrapper.GetHeight();
            var containerWidth = RectangleWrapper.GetWidth();
            Height = model.Height * containerHeight;
            Width = model.Width * containerWidth;

            LocationChanged?.Invoke(this, new Point(model.TopLeftPoint.X * containerWidth, model.TopLeftPoint.Y * containerHeight));
        }


        private void RegionController_TitleChanged(object source, string title)
        {
            Name = title;
        }

        private void RegionController_LocationChanged(object sender, Point topLeft)
        {
            var model = Model as PdfRegionModel;
            if (model == null)
            {
                return;
            }


            var denormalizedTopLeft = new Point(model.TopLeftPoint.X * RectangleWrapper.GetWidth(), model.TopLeftPoint.Y * RectangleWrapper.GetHeight());

            LocationChanged?.Invoke(this, denormalizedTopLeft);
        }

        private void RegionController_SizeChanged(object sender, double width, double height)
        {
            var model = Model as PdfRegionModel;
            if (model == null)
            {
                return;
            }

            Height = model.Height * RectangleWrapper.GetHeight();
            Width = model.Width * RectangleWrapper.GetWidth();
            SizeChanged?.Invoke(this, Width, Height);
        }
        /*
        private void RegionUpdated(object source, Region region)
        {
            if (region.ContentId != Model.ContentDataModelId)
            {
                return;
            }
            var model = Model as PdfRegionModel;
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

            var model = Model as PdfRegionModel;
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
            var model = Model as PdfRegionModel;
            if (model == null)
            {
                return;
            }
            var normalTopLeftX = topLeft.X / RectangleWrapper.GetWidth();
            var normalTopLeftY = topLeft.Y / RectangleWrapper.GetHeight();
            var normalBottomRightX = bottomRight.X / RectangleWrapper.GetWidth();
            var normalBottomRightY = bottomRight.Y / RectangleWrapper.GetHeight();

            var normalWidth = normalBottomRightX - normalTopLeftX;
            var normalHeight = normalBottomRightY - normalTopLeftY;

            //model.TopLeftPoint = new Point(normalTopLeftX, normalTopLeftY);
            //model.BottomRightPoint = new Point(normalBottomRightX, normalBottomRightY);
            //RegionLibraryElementController.UpdateRegion(Model);
            var pdfRegionController = RegionLibraryElementController as PdfRegionLibraryElementController;
            pdfRegionController?.SetLocation(new Point(normalTopLeftX, normalTopLeftY));
            pdfRegionController?.SetSize(normalWidth, normalHeight);
        }

        public void SetNewLocation(Point topLeft)
        {
            var model = Model as PdfRegionModel;
            if (model == null)
            {
                return;
            }
            var normalTopLeftX = topLeft.X / RectangleWrapper.GetWidth();
            var normalTopLeftY = topLeft.Y / RectangleWrapper.GetHeight();

            model.TopLeftPoint = new Point(normalTopLeftX, normalTopLeftY);
            //model.BottomRightPoint = new Point(normalBottomRightX, normalBottomRightY);


            (RegionLibraryElementController as PdfRegionLibraryElementController).SetLocation(model.TopLeftPoint);
        }

        public void SetNewSize(double width, double height)
        {
            var model = Model as PdfRegionModel;
            if (model == null)
            {
                return;
            }
            var normalWidth = width / RectangleWrapper.GetWidth();
            var normalHeight = height / RectangleWrapper.GetHeight();



            (RegionLibraryElementController as PdfRegionLibraryElementController).SetSize(normalWidth, normalHeight);
        }

        public void SetNewName(string text)
        {
            Name = text;
            RegionLibraryElementController.SetTitle(Name);
        }
    }
}
