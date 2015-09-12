using System;

namespace NuSysApp
{
    public class DeleteEventArgs : SuperEventArgs
    {
    
        private NodeModel _node;
        private LinkModel _link;

        public DeleteEventArgs(string text, NodeModel node):base(text)
        {
      
            _node = node;
        }

        public DeleteEventArgs(string text, LinkModel link) : base(text)
        {
            _link = link;
        }

        public NodeModel DeletedNode => _node;

        public LinkModel DeletedLink => _link;
    }
}
