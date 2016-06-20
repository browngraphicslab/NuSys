﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class PdfRegionViewModel : RegionViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public double Height { get; set; }
        public double Width { get; set; }
        private LibraryElementController _elementController;

        public delegate void SizeChangedEventHandler(object sender, Point topLeft, Point bottomRight);
        public event SizeChangedEventHandler SizeChanged;

        public PdfRegionViewModel(PdfRegion model, LibraryElementController elementController, Sizeable sizeable) : base(model, elementController, sizeable)
        {
            if (model == null)
            {
                return;
            }
            _elementController = elementController;
            ContainerSizeChanged += BaseSizeChanged;
            Height = (model.BottomRightPoint.Y*sizeable.GetHeight()) - (model.TopLeftPoint.Y*sizeable.GetHeight());
            Width = (model.BottomRightPoint.X * sizeable.GetWidth()) - (model.TopLeftPoint.X * sizeable.GetWidth());

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

            var topLeft = new Point(model.TopLeftPoint.X * width, model.TopLeftPoint.Y * height);
            var bottomRight = new Point(model.BottomRightPoint.X * width, model.BottomRightPoint.Y * height);
            SizeChanged?.Invoke(this, topLeft, bottomRight);

            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
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
            Controller.UpdateRegion(Model);
        }

    }
}
