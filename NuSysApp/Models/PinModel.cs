using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class PinModel : BaseINPC, Sendable
    {
        private double _x;
        private double _y;
        private string _text;
        private MatrixTransform _transform;
        private Network.DebouncingDictionary _dict;

        public PinModel (string id) : base()
        {
            this.Transform = new MatrixTransform();
            this.Text = "NusysLand";
            ID = id;
            _dict = new Network.DebouncingDictionary(this);
        }



        public void Delete()
        {

        }

        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
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
                if (NetworkConnector.Instance.ModelIntermediate.IsSendableLocked(ID))
                {
                    RaisePropertyChanged("Model_Text");
                }
                else
                {
                    _dict.Add("text", Text);
                }
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
