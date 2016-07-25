using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class ElementModel : Sendable
    {       
        private double _alpha = 1;
        private SolidColorBrush _color;
        private double _height;
        private double _scaleX = 1;
        private double _scaleY = 1;
        private string _title = string.Empty;
        private double _width;
        private double _x;
        private double _y;
        

        public ElementModel(string id) : base(id)
        {
            ElementType = ElementType.None;
            InqCanvas = new InqCanvasModel(id);
        }

        public ElementType ElementType { get; set; }

        public InqCanvasModel InqCanvas { get; set; }

        public string LibraryId { set; get; }

        public string ParentCollectionId { get; set; }

        public List<Windows.Foundation.Point> ShapePoints { get; set; } 

        // TODO: Move color to higher level type

        public SolidColorBrush Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
            }
        }

        public virtual double Width
        {
            get { return _width; }
            set
            {
                _width = value;
            }
        }

        public virtual double Height
        {
            get { return _height; }
            set
            {
                
                _height = value;
            }
        }

        public virtual double ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
            }
        }

        public virtual double ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
            }
        }

        public virtual double Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;

            }
        }

        public virtual string Title
        {
            get { return _title; }
            set
            {
                _title = value;
            }
        }
        public string CreatorId { get; set; }

        public virtual void Delete()
        {
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("x", X);
            dict.Add("y", Y);
            dict.Add("width", Width);
            dict.Add("height", Height);
            dict.Add("alpha", Alpha);
            dict.Add("scaleX", ScaleX);
            dict.Add("scaleY", ScaleY);
            dict.Add("title", Title);
            dict.Add("type", ElementType.ToString());
            dict.Add("contentId", LibraryId);

            var lines = new List<Dictionary<string, object>>();
            foreach (var inqLineModel in InqCanvas.Lines)
            {
                lines.Add(await inqLineModel.Pack());
            }

            dict.Add("inqLines", lines);
            dict.Add("points", ShapePoints);
            return dict;
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
            CreatorId = props.GetString("creator_user_id", null);

            if (props.ContainsKey("system_sender_ip") && SessionController.Instance.NuSysNetworkSession != null && SessionController.Instance.NuSysNetworkSession.NetworkMembers != null &&
                SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(
                    props.GetString("system_sender_ip")))
            {
               // TODO: Refactor
               // LastNetworkUser = SessionController.Instance.NuSysNetworkSession.NetworkMembers[props.GetString("system_sender_ip")];
            }

            if (props.ContainsKey("type"))
            {
                string t = props.GetString("type");
                ElementType = (ElementType)Enum.Parse(typeof(ElementType), t);
            }
            else if (props.ContainsKey("nodeType"))
            {
                string t = props.GetString("nodeType");
                ElementType = (ElementType)Enum.Parse(typeof(ElementType), t);
            }
            if (props.ContainsKey("contentId"))
            {
                LibraryId = props.GetString("contentId", "");
            }
            if (props.ContainsKey("creator"))
            {
                ParentCollectionId = props.GetString("creator", ParentCollectionId);
            }
            if (props.ContainsKey("points"))
            {
                ShapePoints = props.GetList<Windows.Foundation.Point>("points");
            }

            InqCanvas.UnPack(props);

            await base.UnPack(props);
        }
    }
}