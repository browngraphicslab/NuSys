using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NuSysApp
{
    class SessionViewPopupViewModel : BaseINPC
    {

        // the width of the popup
        private double _width;
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        // the height of the popup
        private double _height;
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        private Visibility _isOpen;

        public Visibility IsOpen
        {
            get { return _isOpen; }
            set
            {
                _isOpen = value;
                RaisePropertyChanged("IsOpen");
            }
        }

        public void Init()
        {
            Width = 400;
            Height = 400;
            IsOpen = Visibility.Collapsed;
            
        }

        public void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
