using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    [DataContract]
    public class NodeModel : AtomModel
    {
        private double _x;
        private double _y;
        private double _alpha = 1;
        private double _scaleX = 1;
        private double _scaleY = 1;
        private double _width;
        private double _height;

        public string Title { get; set; }
        public NodeType NodeType { get; set; }

        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);

        public event DeleteEventHandler Deleted;

        public delegate void LocationUpdateEventHandler(object source, PositionChangeEventArgs e);

        public event LocationUpdateEventHandler PositionChanged;

        public delegate void WidthHeightUpdateEventHandler(object source, WidthHeightUpdateEventArgs e);

        public event WidthHeightUpdateEventHandler SizeChanged;

        public delegate void ScaleChangedEventHandler(object source);

        public delegate void AlphaChangedEventHandler(object source);

        public event ScaleChangedEventHandler ScaleChanged;

        public event AlphaChangedEventHandler AlphaChanged;

        public InqCanvasModel InqCanvas { get; }
        public NodeContentModel Content { set; get; }

        public NodeModel(string id) : base(id)
        {
            InqCanvas = new InqCanvasModel(id);
            if (SessionController.Instance.ActiveWorkspace != null)
            {
                Metadata["workspace"] = SessionController.Instance.ActiveWorkspace.Id;
            }
        }
        
        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                PositionChanged?.Invoke(this, new PositionChangeEventArgs(X, Y));
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                PositionChanged?.Invoke(this, new PositionChangeEventArgs(X, Y));
            }
        }

        public virtual double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                SizeChanged?.Invoke(this, new WidthHeightUpdateEventArgs(Width, Height));
            }
        }

        public virtual double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                SizeChanged?.Invoke(this, new WidthHeightUpdateEventArgs(Width, Height));
            }
        }

        public virtual double ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
                ScaleChanged?.Invoke(this);
            }
        }

        public virtual double ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
                ScaleChanged?.Invoke(this);
            }
        }

        public virtual double Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;
                AlphaChanged?.Invoke(this);
            }
        }


        public void MoveToGroup(NodeContainerModel nodeContainer, bool keepInOld = false)
        {
            //this.ParentGroup = nodeContainer;
            var oldGroupId = (string)Metadata["nodeContainer"];
            Metadata["nodeContainer"] = nodeContainer.Id;
            nodeContainer?.AddChild(this); //only add if nodeContainer isn't null

            if (!keepInOld)
            {
                var currentGroup = SessionController.Instance.IdToSendables[oldGroupId] as NodeContainerModel;
                currentGroup.RemoveChild(this);
            }
        }

        public override async Task UnPack(Message props)
        {
            X = props.GetDouble("x", X);
            Y = props.GetDouble("y", Y);
            Width = props.GetDouble("width", Width);
            Height = props.GetDouble("height", Height);
            Alpha = props.GetDouble("alpha", Alpha);
            ScaleX = props.GetDouble("scaleX", ScaleX);
            ScaleY = props.GetDouble("scaleY", ScaleY);
            Title = props.GetString("title", "");
            if (props.ContainsKey("nodeType")) { 
                string t = props["nodeType"];
                NodeType = (NodeType)Enum.Parse(typeof(NodeType), t);
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
            dict.Add("alpha", Alpha.ToString());
            dict.Add("scaleX", ScaleX.ToString());
            dict.Add("scaleY", ScaleY.ToString());
            dict.Add("nodeType", NodeType.ToString());
            dict.Add("title", Title);
            dict.Add("type", "node");
            return dict;
        }
    }
}