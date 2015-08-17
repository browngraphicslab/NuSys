using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using NuSysApp.Network;

namespace NuSysApp
{
    public class Node : Atom
    {
        private int _x;
        private int _y;
        private double _width;
        private double _height;
        public Node(string id) : base (id)
        {
            StartLines = new List<Link>();
            EndLines = new List<Link>();
        }

        public Content Content { set; get; }

        public List<Link> StartLines { get; }

        public List<Link> EndLines { get; }
        
        public List<Node> ConnectedNodes { get; }

        public int X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
                this.DebounceDict.Add("x",_x.ToString());
                RaisePropertyChanged("Model_X");
            }
        }


        public int Y
        {
            get
            {
                return _y;
            }
            set
            {
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

        public void Update(Dictionary<string, string> props)
        {
            if (props.ContainsKey("x"))
            {
                X = Int32.Parse(props["x"]);
            }
            if (props.ContainsKey("y"))
            {
                Y = Int32.Parse(props["y"]);
            }
            if (props.ContainsKey("width"))
            {
                Width = Double.Parse(props["width"]);
            }
            if (props.ContainsKey("height"))
            {
                Height = Double.Parse(props["height"]);
            }
            base.Update(props);
        }


    }
}
