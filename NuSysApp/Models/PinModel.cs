using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    class PinModel : BaseINPC
    {
        private string _text;
        private MatrixTransform _transform; 
        public PinModel () : base()
        {
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

                RaisePropertyChanged("Model_Text");
            }
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

                RaisePropertyChanged("Model_Transform");
            }
        }
    }
}
