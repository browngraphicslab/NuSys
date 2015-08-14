using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class Node : Atom
    {
        public Node(int id) : base()
        {
            StartLines = new List<Link>();
            EndLines = new List<Link>();
            ID = id;
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

        public Constants.NodeType NodeType { get; set; }

        public virtual string GetContentSource()
        {
            return null;
        }


    }
}
