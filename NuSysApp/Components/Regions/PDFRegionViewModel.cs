﻿using System;
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

        //public double Width { get; set; }
        public bool Editable { get; set; }

        public double OriginalHeight { get; set; }
        public double OriginalWidth { get; set; }

        public delegate void SizeChangedEventHandler(object sender, Point topLeft, Point bottomRight);
        public event SizeChangedEventHandler SizeChanged;

        public delegate void RegionUpdatedEventHandler(object sender, double height, double width);

        public event RegionUpdatedEventHandler RegionChanged;

        public PdfRegionViewModel(PdfRegion model, LibraryElementController elementController, RegionController regionController, Sizeable sizeable) : base(model, elementController, regionController, sizeable)
        {
            if (model == null)
            {
                return;
            }

            ContainerSizeChanged += BaseSizeChanged;
            Height = (model.BottomRightPoint.Y*sizeable.GetHeight()) - (model.TopLeftPoint.Y*sizeable.GetHeight());
            Width = (model.BottomRightPoint.X * sizeable.GetWidth()) - (model.TopLeftPoint.X * sizeable.GetWidth());
            ContainerHeight = sizeable.GetHeight();
            ContainerWidth = sizeable.GetWidth();
            elementController.RegionUpdated += RegionUpdated;
            Editable = true;

        }

        private void RegionUpdated(object source, Region region)
        {
            if (region.Id != Model.Id)
            {
                return;
            }
            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }
            //Debug.WriteLine("sdfsdfsdf");
            Height = model.BottomRightPoint.Y * ContainerViewModel.GetHeight() - model.TopLeftPoint.Y * ContainerViewModel.GetHeight();
           // Width = model.BottomRightPoint.X * ContainerViewModel.GetWidth() - model.TopLeftPoint.X * ContainerViewModel.GetWidth();
            RegionChanged?.Invoke(this, Height, Width);
            var topLeft = new Point(model.TopLeftPoint.X * ContainerViewModel.GetWidth(), model.TopLeftPoint.Y * ContainerViewModel.GetHeight());
            var bottomRight = new Point(model.BottomRightPoint.X * ContainerViewModel.GetWidth(), model.BottomRightPoint.Y * ContainerViewModel.GetHeight());
            SizeChanged?.Invoke(this, topLeft, bottomRight);
            //model.TopLeftPoint = new Point();

            RaisePropertyChanged("Height"); 
            RaisePropertyChanged("Width");
        }

        private void BaseSizeChanged(object sender, double width, double height)
        {
            var model = Model as PdfRegion;
            if (model == null)
            {
                return;
            }
            Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y) * height;
            Width = (model.BottomRightPoint.X - model.TopLeftPoint.X) * width;
            ContainerHeight = height;
            ContainerWidth = width;

            var topLeft = new Point(model.TopLeftPoint.X * width, model.TopLeftPoint.Y * height);
            var bottomRight = new Point(model.BottomRightPoint.X * width, model.BottomRightPoint.Y * height);
            SizeChanged?.Invoke(this, topLeft, bottomRight);

            RegionChanged?.Invoke(this, Height, Width);
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

            model.TopLeftPoint = new Point(normalTopLeftX, normalTopLeftY);
            model.BottomRightPoint = new Point(normalBottomRightX, normalBottomRightY);
            LibraryElementController.UpdateRegion(Model);
        }

    }
}
