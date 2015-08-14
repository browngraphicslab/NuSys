
namespace NuSysApp
{
    public class Link : Atom
    {
        public Link(Node inNode, Node outNode, int id) : base(id)
        {
            ID = id;
            InNodeID = inNode.ID;
            OutNodeID = outNode.ID;
        }

        public int InNodeID { get; set; }
        public int OutNodeID { get; set; }
    }
}
