using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class PinViewModel : BaseINPC
    {
        private MatrixTransform _transform;
        private UserControl _view;
        private PinModel _model;
        private string _text = string.Empty;
        
        public PinViewModel(PinModel model) : base()
        {
            Model = model;
            Transform = new MatrixTransform();   
            View = new PinView(this);
          //  this.Model.PropertyChanged += (s, e) => { Update(e); };
            Text = "<Enter Pin Name>";

            var transMat = ((MatrixTransform)View.RenderTransform).Matrix;
            transMat.OffsetX = model.X;
            transMat.OffsetY = model.Y;
            Transform = new MatrixTransform
            {
                Matrix = transMat
            };

            model.OnLocationUpdate += OnLocationUpdate;
        }

        private void OnLocationUpdate(object source, PositionChangeEventArgs args)
        {
            var transMat = ((MatrixTransform)View.RenderTransform).Matrix;
            transMat.OffsetX = args.X;
            transMat.OffsetY = args.Y;
            Transform = new MatrixTransform
            {
                Matrix = transMat
            };
        }

        private void Update(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Model_Text":
                    this.Text = ((PinModel)Model).Text;
                    break;
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
                ((PinModel)Model).Text = value;
                RaisePropertyChanged("Text");
            }
        }

        public PinModel Model
        {
            get { return _model; }
            set
            {
                if (_model == value)
                {
                    return;
                }
                _model = value;
                RaisePropertyChanged("Model");
            }
        }        
    }
}
