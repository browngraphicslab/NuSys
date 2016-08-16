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
        /// <summary>
        /// Without a model we need to store these temporary normalized height(between 0 and 1) so that if we create
        /// an actual library element model we can call upon the height     
        /// </summary>
        public double NormalizedHeight { get; set; }
        /// <summary>
        /// Without a model we need to store these temporary normalized width(between 0 and 1) so that if we create
        /// an actual library element model we can call upon the width
        /// </summary>
        public double NormalizedWidth { get; set; }
        /// <summary>
        /// Without a model we need to store the normalized top left point (both values are between 0 and 1) 
        /// so that if we create a real library element we can call upon it otherwise it can just be disposed of
        /// </summary>
        public Point NormalizedTopLeftPoint { get; set; }
        /// <summary>
        /// This is needed so that we can keeo track of handlers that involve the size changed and location changed handlers so 
        /// the position and size of the region can be properly updated when the size of the wrapper changes
        /// </summary>
        public RectangleWrapper RectangleWrapper { get; set; }
        /// <summary>
        /// This is also necessary so that we can get the new image region args and then shoot off a request to the server to create the 
        /// actual library element model
        /// </summary>
        public DetailHomeTabViewModel HomeTabViewModel { get; set; }
        public bool Editable { get; private set; }

        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler SizeChanged;

        public delegate void LocationChangedEventHandler(object sender, Point topLeft);
        public event LocationChangedEventHandler LocationChanged;

        private RectangleRegionLibraryElementController _regionLibraryElementController;


        public TemporaryImageRegionViewModel( Point topLeftPoint, double width, double height, RectangleWrapper rectangleWrapper, DetailHomeTabViewModel hometabViewModel)
        {
            NormalizedTopLeftPoint = topLeftPoint;
            NormalizedWidth = width;
            NormalizedHeight = height;
            Editable = false;
            RectangleWrapper = rectangleWrapper;
            rectangleWrapper.SizeChanged += RectangleWrapper_SizeChanged;

            RectangleWrapper.Disposed += Dispose;
            HomeTabViewModel = hometabViewModel;
        }


        private void RectangleWrapper_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            var containerHeight = RectangleWrapper.GetHeight();
            var containerWidth = RectangleWrapper.GetWidth();
            Height = NormalizedHeight * containerHeight;
            Width = NormalizedWidth * containerWidth;
            var denormalizedTopLeft = NormalizedTopLeftPoint;
            // do not remove this location changed, it breaks everything if you do
            SizeChanged?.Invoke(this, Width,Height);
            LocationChanged?.Invoke(this, new Point(NormalizedTopLeftPoint.X * containerWidth, NormalizedTopLeftPoint.Y * containerHeight));
        }


        private void RegionController_SizeChanged(object sender, double width, double height)
        {
            Height = NormalizedHeight * RectangleWrapper.GetHeight();
            Width = NormalizedWidth * RectangleWrapper.GetWidth();
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
