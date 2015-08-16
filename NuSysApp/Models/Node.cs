using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using NuSysApp.Network;

namespace NuSysApp
{
    public class Node
    {
        private string _ID;
        private int _X;
        private int _Y;
        private double _Width;
        private double _Height;
        private DebouncingDictionary _debounceDict;
        public Node(string id)
        {
            StartLines = new List<Link>();
            EndLines = new List<Link>();
            ID = id;
            _debounceDict = new DebouncingDictionary(id.ToString());
        }

        public Content Content { set; get; }

        public List<Link> StartLines { get; }

        public List<Link> EndLines { get; }
        
        public List<Node> ConnectedNodes { get; }
        public string ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
            } 
        } //TODO not have id be settable, ACTUALLY IMPLEMENT THEM

        public int X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    _debounceDict.Add("x", _X.ToString());
                }
            } 
        }

        public int Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    _debounceDict.Add("y", _Y.ToString());
                }
            }
        }

        public MatrixTransform Transform { get; set; }

        public double Width {
            get { return _Width; }
            set
            {
                if (_Width != value)
                {
                    _Width = value;
                    _debounceDict.Add("width", _Width.ToString());
                }
            }
        }

        public double Height
        {
            get { return _Height; }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    _debounceDict.Add("height", _Height.ToString());
                }
            }
        }

        public string NodeType { get; set; }

        public virtual string GetContentSource()
        {
            return null;
        }

        public async Task Update(Dictionary<string, string> props)
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
        }

    }
}
