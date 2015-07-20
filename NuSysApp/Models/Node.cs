using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuStarterProject
{
    public class Node
    {
        private List<Link> _startLines, _endLines;
        private List<Node> _connectedNodes;

        private Content _content;
        private int _id;
        public Node(int id)
        {
            _startLines = new List<Link>();
            _endLines = new List<Link>();
            _id = id;
        }
        
        public void ConnectNodes(Node node)
        {
            if (!this.ConnectedNodes.Contains(node))
            {
                Link connector = new Link(this, node);
            }
        }

        public void Delete()
        {
            foreach(Link connect in this.StartLines) 
            {
                connect.DeleteFromOut();
            }
            foreach(Link connect in this.EndLines)
            {
                connect.DeleteFromIn();
            }
        }

        public Content Content
        {
            set { _content = value; }
            get { return _content; }
        }
        public List<Link> StartLines
        {
            get { return _startLines;}
        }
        public List<Link> EndLines
        {
            get { return _endLines; }
        }
        public List<Node> ConnectedNodes
        {
            get { return _connectedNodes; }
        }

        public int ID
        {
            get{ return _id; }
        }
    }
}
