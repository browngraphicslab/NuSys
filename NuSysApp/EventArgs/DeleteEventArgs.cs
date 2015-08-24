using System;

namespace NuSysApp
{
    public class DeleteEventArgs : SuperEventArgs
    {
    
        private Node _node;

        public DeleteEventArgs(string text, Node node):base(text)
        {
      
            _node = node;
        }

        public Node DeletedNode => _node;
    }
}
