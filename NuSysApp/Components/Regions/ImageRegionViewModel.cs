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
        public double Height;
        public double Width;
        public bool Editable {
            set
            {
                _editable = value;
            }
            get
            {
                RaisePropertyChanged("Editable");
                return _editable;
            }
        }

        private bool _editable;
        public ImageRegionViewModel(RectangleRegion model, LibraryElementController controller) : base(model,controller)
        {
            ContainerSizeChanged += BaseSizeChanged;
            Editable = false;

        }
        private void BaseSizeChanged(object sender, double width, double height)
        {

            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y)*height;
            Width = (model.BottomRightPoint.X - model.BottomRightPoint.X)*width;
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }
    }
}
