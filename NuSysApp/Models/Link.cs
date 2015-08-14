
namespace NuSysApp
{
    public class Link
    {
        public Link(Node inNode, Node outNode)
        {
            InNodeID = inNode.ID;
            OutNodeID = outNode.ID;
        }

        public int InNodeID { get; set; }
        public int OutNodeID { get; set; }
    }
}
