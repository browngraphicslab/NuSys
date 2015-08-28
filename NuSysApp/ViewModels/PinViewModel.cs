using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class PinViewModel : BaseINPC
    {
        private MatrixTransform _transform;
        private UserControl _view;
        private string _text;
        
        public PinViewModel() : base()
        {
            Transform = new MatrixTransform();
            View = new PinView(this);
            Text = "Nusysland";
        }
        public MatrixTransform Transform
        {
            get { return _transform; }
            set
            {
                if (_transform == value)
                {
                    return;
                }
                _transform = value;

                RaisePropertyChanged("Transform");
            }
        }

        public UserControl View
        {
            get { return _view; }
            set
            {
                if (_view == value)
                {
                    return;
                }
                _view = value;

                RaisePropertyChanged("View");
            }
        }
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                {
                    return;
                }
                _text = value;

                RaisePropertyChanged("Text");
            }
        }


    }
}
