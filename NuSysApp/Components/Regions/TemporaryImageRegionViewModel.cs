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
        /// <summary>
        /// This is the private behind of the public height member that the temporaryregion view binds to
        /// </summary>
        private double _height;
        /// <summary>
        /// This is the private behind of the public width member that the temporaryregion view binds to
        /// </summary>
        private double _width;
        /// <summary>
        /// This is the disposed event that the children of the temp view listen to to get their handlers stripped from them
        /// </summary>
        /// 

        private int? _pageLocation;

        public event EventHandler Disposed;
        /// <summary>
        /// This is the height that the view binds to. It is denormalized based on the size of the wrapper
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }
        /// <summary>
        /// Page location of temporary region view. If null, it's an temporaryregionview for images. If not, it's a trv for PDFs.
        /// </summary>
        public int? PageLocation
        {
            get { return _pageLocation; }
            set
            {
                _pageLocation = value;
                RaisePropertyChanged("PageLocation");
            }

        }
        /// <summary>
        /// this is the width that the view binds to, it is denormalized based on the size of the wrapper
        /// </summary>
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
        /// <summary>
        /// This is the parent image's accesstype so that the temporary image can then inheret it's accesstype
        /// </summary>
        public NusysConstants.AccessType AccessType { get; set; }


        /// <summary>
        /// when the size changes we want to tell the view that the location on the actual node has changed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public delegate void LocationChangedEventHandler(object sender, Point topLeft);
        public event LocationChangedEventHandler LocationChanged;

        /// <summary>
        /// a list of metadata entries that will be applied to the new region after it has been made into a full region, if ever. 
        /// </summary>
        public List<MetadataEntry> MetadataToAddUponBeingFullRegion { get; set; }

        public TemporaryImageRegionViewModel( Point topLeftPoint, double width, double height, RectangleWrapper rectangleWrapper, DetailHomeTabViewModel hometabViewModel, int? pageLocation = null, NusysConstants.AccessType access = NusysConstants.AccessType.Private)
        {
            NormalizedTopLeftPoint = topLeftPoint;
            NormalizedWidth = width;
            NormalizedHeight = height;

            PageLocation = pageLocation;

            RectangleWrapper = rectangleWrapper;
            HomeTabViewModel = hometabViewModel;

            AccessType = access;
            rectangleWrapper.SizeChanged += RectangleWrapper_SizeChanged;
            RectangleWrapper.Disposed += Dispose;
        }

        /// <summary>
        /// When the size of the rectangle wrapper, where the temporary regions are stored in, changes then we tell the vm to 
        /// adjust it's size and location based on the new values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RectangleWrapper_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            var containerHeight = RectangleWrapper.GetHeight();
            var containerWidth = RectangleWrapper.GetWidth();
            Height = NormalizedHeight * containerHeight;
            Width = NormalizedWidth * containerWidth;
            var denormalizedTopLeft = NormalizedTopLeftPoint;

            LocationChanged?.Invoke(this, new Point(NormalizedTopLeftPoint.X * containerWidth, NormalizedTopLeftPoint.Y * containerHeight));
        }

        /// <summary>
        /// We dispose the temp region vm's handlers so that there aren't any memory leaks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Dispose(object sender, EventArgs e)
        {
            RectangleWrapper.Disposed -= Dispose;
            RectangleWrapper.SizeChanged -= RectangleWrapper_SizeChanged;
            Disposed?.Invoke(this, EventArgs.Empty);
        }

    }
}
