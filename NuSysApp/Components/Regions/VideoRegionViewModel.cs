﻿using System;
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
using NusysIntermediate;
using WinRTXamlToolkit.Tools;

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
        private string _name;
        private VideoRegionLibraryElementController _regionLibraryElementController;
        #endregion PrivateVariables

        public event EventHandler Disposed;


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
                return Math.Max(0, _height * AudioWrapper.GetHeight());
            }
            set
            {
                _height = value;
                RaisePropertyChanged("RectangleHeight");
            }
        }
        public double RectangleWidth {
            get { return Math.Max(0, _width* AudioWrapper.GetWidth()); }
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
                return Math.Max(0, (_intervalEnd - _intervalStart) * (AudioWrapper.GetWidth()));
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
                return Math.Max(0, _intervalRegionTranslateY * AudioWrapper.GetHeight());
            }
            set
            {
                _intervalRegionTranslateY = value;
                RaisePropertyChanged("IntervalRegionTranslateY");
            }
        }
        public double IntervalStart
        {
            get { return Math.Max(0, _intervalStart * AudioWrapper.GetWidth()); }
            set
            {
                Debug.Assert(!Double.IsNaN(value));
                _intervalStart = value;
                RaisePropertyChanged("IntervalStart");
                RaisePropertyChanged("IntervalRegionWidth");
            }
        }
        public double IntervalEnd
        {
            get { return Math.Max(0, _intervalEnd * AudioWrapper.GetWidth()); }
            set
            {
                _intervalEnd = value;
                RaisePropertyChanged("IntervalEnd");
                RaisePropertyChanged("IntervalRegionWidth");
            }
        }
        public Point TopLeft 
        {
            get
            {
                return new Point(_topLeftPoint.X * AudioWrapper.GetWidth(), _topLeftPoint.Y * AudioWrapper.GetHeight()); 
            }
            set
            {
                _topLeftPoint = new Point(value.X, value.Y);
                RaisePropertyChanged("TopLeft");
            }
        }

        public AudioWrapper AudioWrapper { get; set; }

        public VideoRegionViewModel(VideoRegionModel model, VideoRegionLibraryElementController regionLibraryElementController, AudioWrapper audioWrapper) : base(model, regionLibraryElementController)
        {
            _regionLibraryElementController = regionLibraryElementController;
            audioWrapper.SizeChanged += AudioWrapper_SizeChanged;
            _regionLibraryElementController.SizeChanged += SizeChanged;
            _regionLibraryElementController.LocationChanged += LocationChanged;
            _regionLibraryElementController.IntervalChanged += IntervalChanged;
            _regionLibraryElementController.TitleChanged += TitleChanged;
            _regionLibraryElementController.IntervalChanged += audioWrapper.AudioWrapper_TimeChanged;
            _height = (model.Height);
            _width = (model.Width);
            _topLeftPoint = new Point(model.TopLeftPoint.X , model.TopLeftPoint.Y );
            _intervalStart = model.Start;
            _intervalEnd = model.End;
            _intervalRegionWidth = _intervalEnd - _intervalStart;
            _intervalRegionTranslateY = 1;

            Name = Model.Title;
            AudioWrapper = audioWrapper;
            
            Editable = true;
        }

        private void AudioWrapper_SizeChanged(object sender, SizeChangedEventArgs e)
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

        private void TitleChanged(object source, string title)
        {
            Name = title;
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
            var newstart = Math.Max(0, start);
            var controller = RegionLibraryElementController as VideoRegionLibraryElementController;
            controller?.SetStartTime(newstart / (AudioWrapper.GetWidth()));
        }
        public void SetIntervalEnd(double end)
        {
            var newEnd = Math.Max(0, end);
            var controller = RegionLibraryElementController as VideoRegionLibraryElementController;
            controller?.SetEndTime(newEnd / (AudioWrapper.GetWidth()));
        }
        
        public void SetRegionSize(double width, double height)
        {
            var h = Math.Max(0,height);
            var w = Math.Max(0, width);
            var controller = RegionLibraryElementController as VideoRegionLibraryElementController;
            controller?.SetHeight(h / AudioWrapper.GetHeight());
            controller?.SetWidth(w / AudioWrapper.GetWidth());
        }

        public void SetRegionLocation(Point topLeft)
        {
            var controller = RegionLibraryElementController as VideoRegionLibraryElementController;
            controller?.SetLocation(new Point(topLeft.X / AudioWrapper.GetWidth(), topLeft.Y / AudioWrapper.GetHeight()));
        }

        public override void Dispose(object sender, EventArgs e)
        {
            AudioWrapper.Disposed -= Dispose;
            AudioWrapper.SizeChanged -= AudioWrapper_SizeChanged;
            _regionLibraryElementController.IntervalChanged -= IntervalChanged;
            _regionLibraryElementController.TitleChanged -= TitleChanged;
            _regionLibraryElementController.IntervalChanged -= AudioWrapper.AudioWrapper_TimeChanged;
            _regionLibraryElementController.Disposed -= Dispose;
            Disposed?.Invoke(sender, e);
        }
    }

}