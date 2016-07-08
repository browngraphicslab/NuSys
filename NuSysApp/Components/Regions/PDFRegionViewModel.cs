using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
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
        public PdfRegionViewModel(PdfRegion model, LibraryElementController elementController, PdfRegionController regionController, Sizeable sizeable) : base(model, elementController, regionController, sizeable)
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
            //RegionController.RegionUpdated += RegionUpdated;


            regionController.SizeChanged += RegionController_SizeChanged;
            regionController.LocationChanged += RegionController_LocationChanged;
            regionController.TitleChanged += RegionController_TitleChanged;


            Name = Model.Name;

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
            if (region.ContentId != Model.ContentId)
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

            var model = Model as PdfRegion ;
            if (model == null)
            {
                return;
            }

            Point topLeft;
            Point bottomRight;
            if (ContainerViewModel is PdfDetailHomeTabViewModel)
            {

                var detailVM = ContainerViewModel as PdfDetailHomeTabViewModel;


                ContainerHeight = detailVM.GetHeight();
                ContainerWidth = detailVM.GetWidth();

                Height = model.Height * ContainerHeight;
                Width = model.Width * ContainerWidth;
                topLeft = new Point(model.TopLeftPoint.X * ContainerWidth, model.TopLeftPoint.Y * ContainerHeight);

            }
            else {

                Height = model.Height * height;
                Width = model.Width * width;
                ContainerHeight = height;
                ContainerWidth = width;
                topLeft = new Point(model.TopLeftPoint.X * width, model.TopLeftPoint.Y * height);

            }



           

            //TODO: HOOK THIS UP
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
            //RegionController.UpdateRegion(Model);
            var pdfRegionController = RegionController as PdfRegionController;
            pdfRegionController?.SetLocation(new Point(normalTopLeftX, normalTopLeftY));
            pdfRegionController?.SetSize(normalWidth, normalHeight);
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


            (RegionController as PdfRegionController).SetLocation(model.TopLeftPoint);
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



            (RegionController as PdfRegionController).SetSize(normalWidth, normalHeight);
        }

    }
}
