
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;


namespace NuSysApp
{
    public class Node : Atom
    {
        #region Private Members
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private Group _group;
        #endregion Private Members

        #region Events and Handlers
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);
        public event DeleteEventHandler OnDeletion;

        public delegate void LocationUpdateEventHandler(object source, LocationUpdateEventArgs e);
        public event LocationUpdateEventHandler OnLocationUpdate;

        public delegate void WidthHeightUpdateEventHandler(object source, WidthHeightUpdateEventArgs e);
        public event WidthHeightUpdateEventHandler OnWidthHeightUpdate;

        public delegate void AddToGroupEventHandler(object source, AddToGroupEventArgs e);

        public event AddToGroupEventHandler OnAddToGroup;
        #endregion Events and Handlers

        public Node(string id) : base(id)

        {
            
        }

        public void Delete()
        {
            OnDeletion?.Invoke(this, new DeleteEventArgs("Deleted", this));
        }

        public void MoveToGroup(Group group)
        {
            this.ParentGroup = group;
            group.Add(this);
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
                if (NetworkConnector.Instance.ModelLocked)
                {
                    OnLocationUpdate?.Invoke(this, new LocationUpdateEventArgs("Changed X-coordinate", X, Y));
                }
                else
                {
                    this.DebounceDict.Add("x", _x.ToString());
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
                if (NetworkConnector.Instance.ModelLocked)
                {
                    OnLocationUpdate?.Invoke(this, new LocationUpdateEventArgs("Changed Y-coordinate", X, Y));
                }
                else
                {
                    this.DebounceDict.Add("y", _y.ToString());
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
                if (NetworkConnector.Instance.ModelLocked)
                {
                    OnWidthHeightUpdate?.Invoke(this, new WidthHeightUpdateEventArgs("Changed width", Width, Height));
                }
                else
                {
                    this.DebounceDict.Add("width", _width.ToString());
                }

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

                if (NetworkConnector.Instance.ModelLocked)
                {
                    OnWidthHeightUpdate?.Invoke(this, new WidthHeightUpdateEventArgs("Changed width", Width, Height));
                }
                else
                {
                    this.DebounceDict.Add("height", _height.ToString());
                }
            }
        }

        public NodeType NodeType { get; set; }

        private Group _parentGroup;

        public Group ParentGroup
        {
            get
            {
                return _parentGroup;
            }
            set
            {
                _parentGroup = value;
                if (NetworkConnector.Instance.ModelLocked)
                {
                    OnAddToGroup?.Invoke(this, new AddToGroupEventArgs("added to group", _parentGroup, this));
                }
                else
                {
                    this.DebounceDict.Add("parentGroup", _parentGroup.ID);
                    this.DebounceDict.MakeNextMessageTCP();
                }
            }
          
        }

        public bool IsAnnotation { get; set; }

        public Atom ClippedParent { get; set; }

        public virtual string GetContentSource()
        {
            return null;
        }

        public override async Task UnPack(Dictionary<string, string> props)
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
            if (props.ContainsKey("parentGroup"))
            {
                if (NetworkConnector.Instance.ModelIntermediate.WorkSpaceModel.IDToSendableDict.ContainsKey(props["parentGroup"]))
                {
                    this.MoveToGroup((Group)NetworkConnector.Instance.ModelIntermediate.WorkSpaceModel.IDToSendableDict[props["parentGroup"]]);
                }
            }
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> dict = await base.Pack();
            dict.Add("x",X.ToString());
            dict.Add("y", Y.ToString());
            dict.Add("width", Width.ToString());
            dict.Add("height", Height.ToString());
            dict.Add("type", "node");
            if (ParentGroup != null)
            {
                dict.Add("parentGroup", ParentGroup.ID);
            }
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
            basicXml.Add(x);

            XmlAttribute y = doc.CreateAttribute("y");
            y.Value = Y.ToString();
            basicXml.Add(y);


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


