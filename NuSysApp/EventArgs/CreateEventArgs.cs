using System;

namespace NuSysApp
{
    public class CreateEventArgs : System.EventArgs
    {
        public CreateEventArgs(NodeModel node)
        {
            CreatedNode = node;
        }

        public NodeModel CreatedNode { get; }
    }
}
