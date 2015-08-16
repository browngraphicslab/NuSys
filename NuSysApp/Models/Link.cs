﻿
namespace NuSysApp
{
    public class Link
    {
        private readonly Node _inNode, _outNode;

        public Link(Node inNode, Node outNode)
        {
            _inNode = inNode;
            _outNode = outNode;
            InNodeID = inNode.ID;
            OutNodeID = outNode.ID;
        }

        public string InNodeID { get; set; }
        public string OutNodeID { get; set; }
    }
}
