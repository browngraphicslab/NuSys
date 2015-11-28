using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    [DataContract]
    public class NodeModel : AtomModel
    {
        public NodeModel(string id) : base(id)
        {
            InqCanvas = new InqCanvasModel(id);
            if (SessionController.Instance.ActiveWorkspace != null)
            {
                Metadata["group"] = SessionController.Instance.ActiveWorkspace.ID;
            }
        }

        public InqCanvasModel InqCanvas { get; }
        public ContentModel Content { set; get; }

        public double X
        {
            get { return _x; }
            set
            {
                if (_x == value)
                {
                    return;
                }
                _x = value;
                if (NetworkConnector.Instance.IsSendableBeingUpdated(ID))
                {
                    PositionChanged?.Invoke(this, new LocationUpdateEventArgs("Changed X-coordinate", X, Y));
                }
                else
                {
                    DebounceDict.Add("x", _x.ToString());
                    PositionChanged?.Invoke(this, new LocationUpdateEventArgs("Changed X-coordinate", X, Y));
                }
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                if (_y == value)
                {
                    return;
                }
                _y = value;
                if (NetworkConnector.Instance.IsSendableBeingUpdated(ID))
                {
                    PositionChanged?.Invoke(this, new LocationUpdateEventArgs("Changed Y-coordinate", X, Y));
                }
                else
                {
                    DebounceDict.Add("y", _y.ToString());
                    PositionChanged?.Invoke(this, new LocationUpdateEventArgs("Changed Y-coordinate", X, Y));
                }
            }
        }

        public virtual double Width
        {
            get { return _width; }
            set
            {
                if (_width == value)
                {
                    return;
                }
                _width = value;
                if (NetworkConnector.Instance.IsSendableBeingUpdated(ID))
                {
                    SizeChanged?.Invoke(this, new WidthHeightUpdateEventArgs("Changed width", Width, Height));
                }
                else
                {
                    DebounceDict.Add("width", _width.ToString());
                }
            }
        }

        public virtual double Height
        {
            get { return _height; }
            set
            {
                if (_height == value)
                {
                    return;
                }
                _height = value;

                if (NetworkConnector.Instance.IsSendableBeingUpdated(ID))
                {
                    SizeChanged?.Invoke(this, new WidthHeightUpdateEventArgs("Changed width", Width, Height));
                }
                else
                {
                    DebounceDict.Add("height", _height.ToString());
                }
            }
        }

        public string Title { get; set; }

        public NodeType NodeType { get; set; }

        public bool IsAnnotation { get; set; }

        public AtomModel ClippedParent { get; set; }

        public void MoveToGroup(GroupModel group, bool keepInOld = false)
        {
            //this.ParentGroup = group;
            var oldGroupId = Metadata["group"];
            Metadata["group"] = group.ID;
            group?.AddChild(this); //only add if group isn't null

            if (!keepInOld)
            {
                var currentGroup = SessionController.Instance.IdToSendables[oldGroupId] as GroupModel;
                currentGroup.RemoveChild(this);
            }
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("x"))
            {
                X = double.Parse(props["x"]);
            }
            if (props.ContainsKey("y"))
            {
                Y = double.Parse(props["y"]);
            }
            if (props.ContainsKey("width"))
            {
                Width = double.Parse(props["width"]);
            }
            if (props.ContainsKey("height"))
            {
                Height = double.Parse(props["height"]);
            }
            if (props.ContainsKey("parentGroup"))
            {
                if (props["parentGroup"] == "null")
                {
                    MoveToGroup(null);
                }
                else if (SessionController.Instance.IdToSendables.ContainsKey(props["parentGroup"]))
                {
                    // TODO: re-add
                    //this.MoveToGroup((GroupModel)SessionController.Instance.IdToSendables[props["parentGroup"]]);
                }
            }

            await base.UnPack(props);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("x", X.ToString());
            dict.Add("y", Y.ToString());
            dict.Add("width", Width.ToString());
            dict.Add("height", Height.ToString());
            dict.Add("type", "node");
            return dict;
        }

        public virtual XmlElement WriteXML(XmlDocument doc)
        {
            var node = doc.CreateElement(string.Empty, "Node", string.Empty);
                //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            var basicXml = getBasicXML(doc);
            foreach (var attr in basicXml)
            {
                node.SetAttributeNode(attr);
            }

            return node;
        }

        /// <summary>
        ///     Writes the XML of the attributes that all nodes have
        /// </summary>
        /// <param name="doc">Main xmlDocument</param>
        /// <returns></returns>
        public List<XmlAttribute> getBasicXML(XmlDocument doc)
        {
            var basicXml = new List<XmlAttribute>();

            //create xml attribute nodes
            var type = doc.CreateAttribute("nodeType");
            type.Value = NodeType.ToString();
            basicXml.Add(type);

            var id = doc.CreateAttribute("id");
            id.Value = ID;
            basicXml.Add(id);

            /*
            if (ParentGroup != null)
            {
                XmlAttribute groupID = doc.CreateAttribute("groupID");
                groupID.Value = this.ParentGroup.ID.ToString();
                basicXml.Add(groupID);
            }
            */

            var x = doc.CreateAttribute("x");
            x.Value = X.ToString();
            basicXml.Add(x);

            var y = doc.CreateAttribute("y");
            y.Value = Y.ToString();
            basicXml.Add(y);


            var height = doc.CreateAttribute("height");
            height.Value = Height.ToString();
            basicXml.Add(height);

            var width = doc.CreateAttribute("width");
            width.Value = Width.ToString();
            basicXml.Add(width);

            // if the node is an annotation, add information to the xml about the link it is attached to
            if (IsAnnotation)
            {
                var clippedParent = doc.CreateAttribute("ClippedParent");
                clippedParent.Value = ClippedParent.ID;
                basicXml.Add(clippedParent);
            }

            return basicXml;
        }

        #region Private Members

        private double _x;
        private double _y;
        private double _width;
        private double _height;

        #endregion Private Members

        #region Events and Handlers

        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);

        public event DeleteEventHandler Deleted;

        public delegate void LocationUpdateEventHandler(object source, LocationUpdateEventArgs e);

        public event LocationUpdateEventHandler PositionChanged;

        public delegate void WidthHeightUpdateEventHandler(object source, WidthHeightUpdateEventArgs e);

        public event WidthHeightUpdateEventHandler SizeChanged;

        #endregion Events and Handlers
    }
}