using System;

namespace NuSysApp
{
    public class CreateEventArgs : System.EventArgs
    {
        public CreateEventArgs(NodeType type, NodeModel node, NodeViewModel)
        {
            CreatedNode = node;
        }

        public NodeModel CreatedNode { get; }
    }
}
