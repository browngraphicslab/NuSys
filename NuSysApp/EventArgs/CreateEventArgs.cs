using System;

namespace NuSysApp
{
    public class CreateEventArgs : SuperEventArgs
    {
        public CreateEventArgs(string text, Node node):base(text)
        {
            CreatedNode = node;
        }

        public Node CreatedNode { get; }
    }
}
