using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class Node
    {
        public Node(int id)
        {
            StartLines = new List<Link>();
            EndLines = new List<Link>();
            ID = id;
        }
        
        public void ConnectNodes(Node node)
        {
            if (!this.ConnectedNodes.Contains(node))
            {
                var connector = new Link(this, node);
            }
        }

        public void Delete()
        {
            foreach(var connect in this.StartLines) 
            {
                connect.DeleteFromOut();
            }
            foreach(var connect in this.EndLines)
            {
                connect.DeleteFromIn();
            }
        }

        public Content Content { set; get; }

        public List<Link> StartLines { get; }

        public List<Link> EndLines { get; }

        public List<Node> ConnectedNodes { get; }

        public int ID { get; }

        public int X { get; set; }

        public int Y { get; set; }

        public MatrixTransform Transform { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public Color Color { get; set; }
    }
}
