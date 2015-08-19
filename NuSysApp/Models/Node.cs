
using System;
﻿using NuSysApp.Models;
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
        private Group _group;
        public Node(string id) : base (id)
        {

        }

        public string Data { get; set; }

        public Content Content { set; get; }


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
                Debug.WriteLine(_x + " " + _y);
                this.DebounceDict.Add("x",_x.ToString());
                if (NetworkConnector.Instance.WorkSpaceModel.Locked)
                {
                    RaisePropertyChanged("Model_X");
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
                if (_y == value)
                {
                    return;
                }
                _y = value;
                this.DebounceDict.Add("y", _y.ToString());
                if (NetworkConnector.Instance.WorkSpaceModel.Locked)
                {
                    RaisePropertyChanged("Model_Y");
                }  
            }
        }

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

        public Group ParentGroup {
            get; set;
            }
        

        public bool IsAnnotation { get; set; }
        public Atom ClippedParent { get; set; }

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
            basicXml.Add(type);

            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = ID.ToString();
            basicXml.Add(id);

            if (ParentGroup != null)
            {
                XmlAttribute groupID = doc.CreateAttribute("groupID");
                groupID.Value = this.ParentGroup.ID.ToString();
                basicXml.Add(groupID);
            }

            XmlAttribute x = doc.CreateAttribute("x");

            x.Value = X.ToString();

            XmlAttribute y = doc.CreateAttribute("y");
            y.Value = Y.ToString();


            XmlAttribute height = doc.CreateAttribute("height");
            height.Value = Height.ToString();
            basicXml.Add(height);

            XmlAttribute width = doc.CreateAttribute("width");
            width.Value = Width.ToString();
            basicXml.Add(width);

            // if the node is an annotation, add information to the xml about the link it is attached to
            if (this.IsAnnotation)
            {
                XmlAttribute clippedParent = doc.CreateAttribute("ClippedParent");
                clippedParent.Value = ClippedParent.ID.ToString();
                basicXml.Add(clippedParent);
            }

            return basicXml;
        }
    }
}


