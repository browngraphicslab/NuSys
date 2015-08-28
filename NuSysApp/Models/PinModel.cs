using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    class PinModel : BaseINPC, Sendable
    {
        private string _text;
        private MatrixTransform _transform; 

        public PinModel (string id) : base()
        {
            this.Transform = new MatrixTransform();
            this.Text = "NusysLand";
            ID = id;
        }

        public Atom.EditStatus CanEdit{get; set; }

        public string ID { get; }

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

        public async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string,string> props = new Dictionary<string, string>();
            props.Add("text",Text);
            return props;
        }

        public async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("text"))
            {
                Text = props["text"];
            }
        }
    }
}
