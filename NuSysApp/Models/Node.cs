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
        public Node(string id) : base (id)
        {
            StartLines = new List<Link>();
            EndLines = new List<Link>();
        }

        public Content Content { set; get; }

        public List<Link> StartLines { get; }

        public List<Link> EndLines { get; }
        
        public List<Node> ConnectedNodes { get; }
        public int X { get; set; }

        public int Y{get; set; }

        public MatrixTransform Transform { get; set; }

        public double Width {get; set; }

        public double Height { get; set; }

        public Constants.NodeType NodeType { get; set; }

        public GroupViewModel ParentGroup { get; set; }

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
