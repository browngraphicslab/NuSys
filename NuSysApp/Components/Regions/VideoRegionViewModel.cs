using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Nodes.AudioNode;

namespace NuSysApp
{
    public class VideoRegionViewModel : RegionViewModel
    {/*
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void DoubleChanged(object sender, double e);
        public event DoubleChanged WidthChanged;
        public event DoubleChanged HeightChanged;*/

        #region PrivateVariables
        private double _width;
        private double _height;
        private double _intervalRegionWidth;
        private double _intervalStart;
        private double _intervalEnd;
        private Point _topLeftPoint;
        private bool _editable;
        private double _intervalRegionTranslateY;
        #endregion PrivateVariables
        public bool Editable {
            get { return _editable; }
            set
            {
                _editable = value;
                RaisePropertyChanged("Editable");
            }
        }

        public double RectangleHeight {
            get
            {
                return _height * ContainerViewModel.GetHeight();
            }
            set
            {
                _height = value;
                RaisePropertyChanged("RectangleHeight");
            }
        }
        public double RectangleWidth {
            get { return _width*ContainerViewModel.GetWidth(); }
            set
            {
                _width = value;
                RaisePropertyChanged("RectangleWidth");
            }
        }


        public double IntervalRegionWidth
        {
            get
            {
                return (_intervalEnd - _intervalStart) * ContainerViewModel.GetWidth();
            }
            set
            {
                _intervalRegionWidth = value;
                RaisePropertyChanged("IntervalRegionWidth");
            }
        }
        public double IntervalRegionTranslateY
        {
            get
            {
                return _intervalRegionTranslateY * ContainerViewModel.GetHeight();
            }
            set
            {
                _intervalRegionTranslateY = value;
                RaisePropertyChanged("IntervalRegionTranslateY");
            }
        }
        public double IntervalStart
        {
            get { return _intervalStart * ContainerViewModel.GetWidth(); }
            set
            {
                Debug.Assert(!Double.IsNaN(value));
                if (double.IsNaN(value))
                {
                    
                }
                _intervalStart = value; 
                RaisePropertyChanged("IntervalStart");
            }
        }
        public double IntervalEnd
        {
            get { return _intervalEnd * ContainerViewModel.GetWidth(); }
            set
            {
                _intervalEnd = value;
                RaisePropertyChanged("IntervalEnd");
            } 
        }
        public Point TopLeft 
        {
            get
            {
                return new Point(_topLeftPoint.X * ContainerViewModel.GetWidth(), _topLeftPoint.Y * ContainerViewModel.GetHeight()); 
            }
            set
            {
                _topLeftPoint = new Point(value.X, value.Y);
                RaisePropertyChanged("TopLeft");
            }
        }
        public VideoRegionViewModel(VideoRegionModel model, LibraryElementController controller, VideoRegionController regionController,Sizeable sizeable) : base(model,controller, regionController,sizeable)
        {
            ContainerSizeChanged += BaseSizeChanged;
            regionController.SizeChanged += SizeChanged;
            regionController.LocationChanged += LocationChanged;
            regionController.IntervalChanged += IntervalChanged;
            _height = (model.Height);
            _width = (model.Width);
            _topLeftPoint = new Point(model.TopLeftPoint.X , model.TopLeftPoint.Y );
            _intervalStart = model.Start;
            _intervalEnd = model.End;
            _intervalRegionWidth = _intervalEnd - _intervalStart;
            _intervalRegionTranslateY = 1;

            Editable = true;
        }

        private void LocationChanged(object sender, Point topLeft)
        {
            TopLeft = new Point(topLeft.X, topLeft.Y);
        }

        private void IntervalChanged(object sender, double start, double end)
        {
            IntervalStart = start;
            IntervalEnd = end;
        }

        private void SizeChanged(object sender, double width, double height)
        {
            RectangleWidth = width;
            RectangleHeight = height;
        }

        private void BaseSizeChanged(object sender, double width, double height)
        {
            RaisePropertyChanged("RectangleWidth");
            RaisePropertyChanged("RectangleHeight");
            RaisePropertyChanged("IntervalRegionWidth");
            RaisePropertyChanged("IntervalStart");
            RaisePropertyChanged("IntervalEnd");
            RaisePropertyChanged("TopLeft");
            RaisePropertyChanged("BottomRight");
            RaisePropertyChanged("IntervalRegionTranslateY");
        }

        public void SetIntervalStart(double start)
        {
            var controller = RegionController as VideoRegionController;
            controller?.SetStartTime(start / ContainerViewModel.GetWidth());
        }
        public void SetIntervalEnd(double end)
        {
            var controller = RegionController as VideoRegionController;
            controller?.SetEndTime(end / ContainerViewModel.GetWidth());
        }
        
        public void SetRegionSize(double width, double height)
        {
            var h = height;
            var controller = RegionController as VideoRegionController;
            controller?.SetSize(width / ContainerViewModel.GetWidth(), height / ContainerViewModel.GetHeight());
        }

        public void SetRegionLocation(Point topLeft)
        {
            var controller = RegionController as VideoRegionController;
            controller?.SetLocation(new Point(topLeft.X / ContainerViewModel.GetWidth(), topLeft.Y / ContainerViewModel.GetHeight()));
        }
    }

}