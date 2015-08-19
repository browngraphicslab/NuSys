using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI;
using Windows.UI.Xaml.Media;
using NuSysApp.Network;

namespace NuSysApp
{
    public class Node : Atom
    { 
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        public Node(string id) : base (id)
        {
            StartLines = new List<Link>();
            EndLines = new List<Link>();
        }

        public string Data { get; set; }

        public Content Content { set; get; }

        public List<Link> StartLines { get; }

        public List<Link> EndLines { get; }
        
        public List<Node> ConnectedNodes { get; }

        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                if (_x == value)
                {
                    return;
                }
                _x = value;
                this.DebounceDict.Add("x",_x.ToString());
                RaisePropertyChanged("Model_X");
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
                if (_y == value)
                {
                    return;
                }
                _y = value;
                this.DebounceDict.Add("y", _y.ToString());
                RaisePropertyChanged("Model_Y");
            }
        }

        public MatrixTransform Transform { get; set; }

        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (_width == value)
                {
                    return;
                }
                _width = value;
                this.DebounceDict.Add("width", _width.ToString());
                RaisePropertyChanged("Model_Width");
            }
        }

        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_height == value)
                {
                    return;
                }
                _height = value;
                this.DebounceDict.Add("height", _height.ToString());
                RaisePropertyChanged("Model_Height");
            }
        }

        public Constants.NodeType NodeType { get; set; }

        public GroupViewModel ParentGroup { get; set; }

        public virtual string GetContentSource()
        {
            return null;
        }

        public override void UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("x"))
            {
                X = Double.Parse(props["x"]);
            }
            if (props.ContainsKey("y"))
            {
                Y = Double.Parse(props["y"]);
            }
            if (props.ContainsKey("width"))
            {
                Width = Double.Parse(props["width"]);
            }
            if (props.ContainsKey("height"))
            {
                Height = Double.Parse(props["height"]);
            }
            base.UnPack(props);
        }

        public override Dictionary<string, string> Pack()
        {
            Dictionary<string, string> dict = base.Pack();
            dict.Add("x",X.ToString());
            dict.Add("y", Y.ToString());
            dict.Add("width", Width.ToString());
            dict.Add("height", Height.ToString());
            return dict;
        }

        public virtual XmlElement WriteXML(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                node.SetAttributeNode(attr);
            }

            return node;
        }

        /// <summary>
        /// Writes the XML of the attributes that all nodes have
        /// </summary>
        /// <param name="doc">Main xmlDocument</param>
        /// <returns></returns>

        public List<XmlAttribute> getBasicXML(XmlDocument doc)
        {
            List<XmlAttribute> basicXml = new List<XmlAttribute>();

            //create xml attribute nodes
            XmlAttribute type = doc.CreateAttribute("nodeType");
            type.Value = NodeType.ToString();

            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = ID.ToString();


            //TODO: Uncomment this when parent group IDs are set
            //XmlAttribute groupID = doc.CreateAttribute("groupID");
            //groupID.Value = ParentGroup.Model.ID.ToString();

            XmlAttribute x = doc.CreateAttribute("x");
            x.Value = Transform.Matrix.OffsetX.ToString();

            XmlAttribute y = doc.CreateAttribute("y");
            y.Value = Transform.Matrix.OffsetY.ToString();

            XmlAttribute height = doc.CreateAttribute("height");
            height.Value = Height.ToString();

            XmlAttribute width = doc.CreateAttribute("width");
            width.Value = Width.ToString();

            //append to list and return
            basicXml.Add(type);
            basicXml.Add(id);
            basicXml.Add(x);
            basicXml.Add(y);
            basicXml.Add(height);
            basicXml.Add(width);

            return basicXml;
        }

    }
}


