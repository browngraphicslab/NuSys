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
    public class TemporaryImageRegionViewModel : BaseINPC
    {

        private double _height;
        private double _width;
        private string _name;
        public event EventHandler Disposed;

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
        private double _normailzedHeight;
        private double _normalizedWidth;
        private Point _topLeftPoint;
        public RectangleWrapper RectangleWrapper { get; set; }
        public bool Editable { get; private set; }

        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler SizeChanged;

        public delegate void LocationChangedEventHandler(object sender, Point topLeft);
        public event LocationChangedEventHandler LocationChanged;

        private RectangleRegionLibraryElementController _regionLibraryElementController;


        public TemporaryImageRegionViewModel( Point topLeftPoint, double width, double height, RectangleWrapper rectangleWrapper)
        {
            _topLeftPoint = topLeftPoint;
            _width = width;
            _height = height;
            Editable = true;
            RectangleWrapper = rectangleWrapper;
            rectangleWrapper.SizeChanged += RectangleWrapper_SizeChanged;
            RectangleWrapper.Disposed += Dispose;
        }


        private void RectangleWrapper_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            var containerHeight = RectangleWrapper.GetHeight();
            var containerWidth = RectangleWrapper.GetWidth();
            Height = _height * containerHeight;
            Width = _width * containerWidth;

            // do not remove this location changed, it breaks everything if you do
            LocationChanged?.Invoke(this, new Point(Width,Height));
        }
        private void RegionController_LocationChanged(object sender, Point topLeft)
        { 

            var denormalizedTopLeft = _topLeftPoint;
            LocationChanged?.Invoke(this, denormalizedTopLeft);

        }


        private void RegionController_SizeChanged(object sender, double width, double height)
        {

            Height = _height * RectangleWrapper.GetHeight();
            Width = _width * RectangleWrapper.GetWidth();
            SizeChanged?.Invoke(this, Width, Height);

        }

        public void Dispose(object sender, EventArgs e)
        {
            RectangleWrapper.Disposed -= Dispose;
            RectangleWrapper.SizeChanged -= RectangleWrapper_SizeChanged;
            Disposed?.Invoke(this, EventArgs.Empty);
        }

    }
}
