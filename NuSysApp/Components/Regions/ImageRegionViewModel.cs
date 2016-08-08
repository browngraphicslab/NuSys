using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageRegionViewModel : RegionViewModel, INuSysDisposable
    {

        private double _height;
        private double _width;
        private string _name;
        public event EventHandler Disposed;

        public string Name {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

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

        public RectangleWrapper RectangleWrapper { get; set; } 

        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler SizeChanged;

        public delegate void LocationChangedEventHandler(object sender, Point topLeft);
        public event LocationChangedEventHandler LocationChanged;

        private RectangleRegionLibraryElementController _regionLibraryElementController;


        public ImageRegionViewModel(RectangleRegion model, RectangleRegionLibraryElementController regionLibraryElementController, RectangleWrapper rectangleWrapper) : base(model, regionLibraryElementController)
        {
            if (model == null)
            {
                return;
            }
            _regionLibraryElementController = regionLibraryElementController;

            regionLibraryElementController.SizeChanged += RegionController_SizeChanged;
            regionLibraryElementController.LocationChanged += RegionController_LocationChanged;
            regionLibraryElementController.TitleChanged += RegionController_TitleChanged;
            regionLibraryElementController.Disposed += Dispose;
            Name = Model.Title;
            Editable = true;
            RectangleWrapper = rectangleWrapper;
            rectangleWrapper.SizeChanged += RectangleWrapper_SizeChanged;
            RectangleWrapper.Disposed += Dispose;
        }


        private void RectangleWrapper_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            var model = this.RegionLibraryElementController.LibraryElementModel as RectangleRegion;
            var containerHeight = RectangleWrapper.GetHeight();
            var containerWidth = RectangleWrapper.GetWidth();
            Height = model.Height * containerHeight;
            Width = model.Width * containerWidth;

            // do not remove this location changed, it breaks everything if you do
            LocationChanged?.Invoke(this,new Point(model.TopLeftPoint.X * containerWidth,model.TopLeftPoint.Y * containerHeight));
        }

        private void RegionController_TitleChanged(object source, string title)
        {
            Name = title;
        }

        private void RegionController_LocationChanged(object sender, Point topLeft)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }

            var denormalizedTopLeft = new Point(model.TopLeftPoint.X * RectangleWrapper.GetWidth(), model.TopLeftPoint.Y * RectangleWrapper.GetHeight());
            LocationChanged?.Invoke(this, denormalizedTopLeft);

        }

        
        private void RegionController_SizeChanged(object sender, double width, double height)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            
            Height = model.Height * RectangleWrapper.GetHeight();
            Width = model.Width * RectangleWrapper.GetWidth();
            SizeChanged?.Invoke(this, Width, Height);

        }

        public void SetNewLocation(Point topLeft)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var normalTopLeftX = topLeft.X / RectangleWrapper.GetWidth();
            var normalTopLeftY = topLeft.Y / RectangleWrapper.GetHeight();

            var tlp = new Point(normalTopLeftX, normalTopLeftY);

            (RegionLibraryElementController as RectangleRegionLibraryElementController).SetLocation(tlp);
        }

        public void SetNewSize(double width, double height)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var normalWidth = width / RectangleWrapper.GetWidth();
            var normalHeight = height / RectangleWrapper.GetHeight();

            (RegionLibraryElementController as RectangleRegionLibraryElementController).SetHeight(normalHeight);
            (RegionLibraryElementController as RectangleRegionLibraryElementController).SetWidth(normalWidth);
        }

        public void SetNewName(string text)
        {
            Name = text;
            RegionLibraryElementController.SetTitle(Name);
        }

        public override void Dispose(object sender, EventArgs e)
        {
            RectangleWrapper.Disposed -= Dispose;
            _regionLibraryElementController.SizeChanged -= RegionController_SizeChanged;
            _regionLibraryElementController.LocationChanged -= RegionController_LocationChanged;
            _regionLibraryElementController.TitleChanged -= RegionController_TitleChanged;
            _regionLibraryElementController.Disposed -= Dispose;
            RectangleWrapper.SizeChanged -= RectangleWrapper_SizeChanged;
            Disposed?.Invoke(this, EventArgs.Empty);
        }

    }
}
