﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private BaseINPC _model;
        private string _text;
        
        public PinViewModel() : base()
        {
            Transform = new MatrixTransform();
            View = new PinView(this);
            Model = new PinModel();
            this.Model.PropertyChanged += (s, e) => { Update(e); };
            Text = "Nusysland";
        }

        private void Update(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Model_Text":
                    this.Text = ((PinModel)Model).Text;
                    break;
                case "Model_Transform":
                    this.Transform = ((PinModel)Model).Transform;
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
                ((PinModel)Model).Transform = value;
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

        public BaseINPC Model
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
