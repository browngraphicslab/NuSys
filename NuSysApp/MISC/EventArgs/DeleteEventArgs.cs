using System;

namespace NuSysApp
{
    public class DeleteEventArgs : SuperEventArgs
    {
    
        private Node _node;
        private Link _link;

        public DeleteEventArgs(string text, Node node):base(text)
        {
      
            _node = node;
        }

        public DeleteEventArgs(string text, Link link) : base(text)
        {
            _link = link;
        }

        public Node DeletedNode => _node;

        public Link DeletedLink => _link;
    }
}
