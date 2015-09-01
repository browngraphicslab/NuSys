using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class PinModel : BaseINPC, Sendable
    {
        public delegate void LocationUpdateEventHandler(object source, LocationUpdateEventArgs e);
        public event LocationUpdateEventHandler OnLocationUpdate;

        private double _x;
        private double _y;
        private string _text;
        private MatrixTransform _transform;
        private Network.DebouncingDictionary _dict;

        public PinModel (string id) : base()
        {
            this.Transform = new MatrixTransform();
            ID = id;
            this.Text = "NusysLand";
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
                if (_x == value) return;
                _x = value;
                if (NetworkConnector.Instance.ModelIntermediate.IsSendableLocked(ID))
                {
                    OnLocationUpdate?.Invoke(this, new LocationUpdateEventArgs("pin X change", X, Y));
                }
                else
                {
                    _dict.Add("x", X.ToString());
                }
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
                if (_y == value) return;
                _y = value;
                if (NetworkConnector.Instance.ModelIntermediate.IsSendableLocked(ID))
                {
                    OnLocationUpdate?.Invoke(this, new LocationUpdateEventArgs("pin Y change", X, Y));
                }
                else
                {
                    _dict.Add("y", Y.ToString());
                }
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
            props.Add("x", X.ToString());
            props.Add("y", Y.ToString());
            return props;
        }

        public async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("text"))
            {
                Text = props["text"];
            }
            if (props.ContainsKey("x"))
            {
                X = double.Parse(props["x"]);
            }
            if(props.ContainsKey("y"))
            {
                Y = double.Parse(props["y"]);
            }
        }

        public XmlElement WriteXML(XmlDocument doc)
        {
            //XmlElement 
            XmlElement pin = doc.CreateElement(string.Empty, "Pin", string.Empty); //TODO: Change how we determine node type for name

            //ID of this pin
            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = this.ID;
            pin.SetAttributeNode(id);

            //Atoms that this link is bound to
            XmlAttribute x = doc.CreateAttribute("x");
            x.Value = this.X.ToString();
            pin.SetAttributeNode(x);

            XmlAttribute y = doc.CreateAttribute("y");
            y.Value = this.Y.ToString();
            pin.SetAttributeNode(y);

            XmlAttribute text = doc.CreateAttribute("text");
            text.Value = this.Text;
            pin.SetAttributeNode(text);

            return pin;
        }
    }
}
