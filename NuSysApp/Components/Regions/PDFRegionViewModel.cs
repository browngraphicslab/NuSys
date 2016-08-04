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
    public class PdfRegionViewModel : RegionViewModel, INuSysDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Disposed;
        public RectangleWrapper RectangleWrapper { get; private set; }
        private double _height;
        private double _width;

        private RectangleRegionLibraryElementController _regionLibraryElementController;

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
                RaisePropertyChanged("Name");
            }
        }


        public delegate void SizeChangedEventHandler(object sender, double height, double width);
        public event SizeChangedEventHandler SizeChanged;

        public delegate void LocationChangedEventHandler(object sender, Point topLeft);
        public event LocationChangedEventHandler LocationChanged;
        public PdfRegionViewModel(PdfRegionModel model, PdfRegionLibraryElementController regionLibraryElementController, RectangleWrapper wrapper) : base(model, regionLibraryElementController)
        {
            if (model == null)
            {
                return;
            }

            _regionLibraryElementController = regionLibraryElementController;
            RectangleWrapper = wrapper;
            RectangleWrapper.Disposed += Dispose;
            RectangleWrapper.SizeChanged += RectangleWrapper_SizeChanged;

            regionLibraryElementController.SizeChanged += RegionController_SizeChanged;
            regionLibraryElementController.LocationChanged += RegionController_LocationChanged;
            regionLibraryElementController.TitleChanged += RegionController_TitleChanged;
            regionLibraryElementController.Disposed += Dispose;


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

        public override void Dispose(object sender, EventArgs e)
        {
            RectangleWrapper.Disposed -= Dispose;
            RectangleWrapper.SizeChanged -= RectangleWrapper_SizeChanged;
            _regionLibraryElementController.SizeChanged -= RegionController_SizeChanged;
            _regionLibraryElementController.LocationChanged -= RegionController_LocationChanged;
            _regionLibraryElementController.TitleChanged -= RegionController_TitleChanged;
            _regionLibraryElementController.Disposed -= Dispose;
            Disposed?.Invoke(this, EventArgs.Empty);
        }

    }
}
