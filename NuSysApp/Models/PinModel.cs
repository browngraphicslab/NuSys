using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    class PinModel : BaseINPC
    {
        private string _text;
        private MatrixTransform _transform; 
        public PinModel () : base()
        {
            this.Transform = new MatrixTransform();
            this.Text = "NusysLand";
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
