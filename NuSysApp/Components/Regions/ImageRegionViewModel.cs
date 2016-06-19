﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class ImageRegionViewModel : RegionViewModel
    {
        public double Height { get; set; }
        public double Width{ get; set; }

        public delegate void SizeChangedEventHandler(object sender, Point topLeft, Point bottomRight);
        public event SizeChangedEventHandler SizeChanged;

        public ImageRegionViewModel(RectangleRegion model, LibraryElementController controller, Sizeable sizeable) : base(model,controller,sizeable)
        {
            ContainerSizeChanged += BaseSizeChanged;
            Height = sizeable.GetHeight();
            Width = sizeable.GetWidth();
        }
        private void BaseSizeChanged(object sender, double width, double height)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y)*height;
            Width = (model.BottomRightPoint.X - model.TopLeftPoint.X)*width;

            var topLeft = new Point(model.TopLeftPoint.X * width, model.TopLeftPoint.Y*height);
            var bottomRight = new Point(model.BottomRightPoint.X * width, model.BottomRightPoint.Y * height);
            SizeChanged?.Invoke(this,topLeft,bottomRight);

            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }
        /*
        public void Translate(double xPercent, double yPercent)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var newTopLeftX = model.TopLeftPoint.X * xPercent;
            var newTopLeftY = model.TopLeftPoint.Y * yPercent;
            var newBottomRightX = model.BottomRightPoint.X * xPercent;
            var newBottomRightY = model.BottomRightPoint.Y * yPercent;
            
            model.TopLeftPoint = new Point(newTopLeftX,newTopLeftY);
            model.BottomRightPoint = new Point(newBottomRightX, newBottomRightY);
        }
        public void Resize(double widthPercent, double heightPercent)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var newBottomRightX = model.BottomRightPoint.X * widthPercent;
            var newBottomRightY = model.BottomRightPoint.Y * heightPercent;


            model.BottomRightPoint = new Point(newBottomRightX, newBottomRightY);*/

        public void SetNewPoints(Point topLeft, Point bottomRight)
        {
            var model = Model as RectangleRegion;
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
