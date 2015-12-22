using System;

namespace NuSysApp
{
    public class DeleteEventArgs : System.EventArgs
    {
        public DeleteEventArgs(NodeModel node)
        {
            DeletedNode = node;
        }

        public DeleteEventArgs(LinkModel link)
        {
            DeletedLink = link;
        }

        public NodeModel DeletedNode { get; }

        public LinkModel DeletedLink { get; }
    }
}
