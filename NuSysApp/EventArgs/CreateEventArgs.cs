using System;

namespace NuSysApp
{
    public class CreateEventArgs : SuperEventArgs
    {
        public CreateEventArgs(string text, NodeModel node):base(text)
        {
            CreatedNode = node;
        }

        public NodeModel CreatedNode { get; }
    }
}
