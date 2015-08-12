
using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class Link
    {
        private readonly Node _inNode, _outNode;

        public Link(Node inNode, Node outNode)
        {
            _inNode = inNode;
            _outNode = outNode;
        }

        public int ID { get; set; }

        /// <summary>
        /// Removes an edge.
        /// </summary>
        public void Delete()
        {
            this.DeleteFromIn();
            this.DeleteFromOut();
        }

        /// <summary>
        /// Removes an incoming edge.
        /// </summary>
        public void DeleteFromIn()
        {
            _outNode.EndLines.Remove(this);
            _outNode.ConnectedNodes.Remove(_inNode);
        }

        /// <summary>
        /// Removes an incoming edge.
        /// </summary>
        public void DeleteFromOut()
        {
            _inNode.StartLines.Remove(this);
            _inNode.ConnectedNodes.Remove(_outNode);
        }
    }
}
