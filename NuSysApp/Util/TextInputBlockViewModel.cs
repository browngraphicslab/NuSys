using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TextInputBlockViewModel: BaseINPC
    {

        private double  _height, _width;

        protected virtual void OnSizeChanged(object source, double width, double height)
        {
            _width = width;
            _height = height;
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }

        public double Width
        {
            get { return _width; }
            set
            {
                if (value < Constants.MinNodeSize) 
                    return;
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                if (value < Constants.MinNodeSize)
                    return;

                _height = value;
                RaisePropertyChanged("Height");
            }
        }
    }
}
