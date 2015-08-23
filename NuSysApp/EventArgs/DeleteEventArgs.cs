using System;

namespace NuSysApp
{
    public class DeleteEventArgs : EventArgs
    {
        private string EventInfo;
        private Node _node;

        public DeleteEventArgs(string text, Node node)
        {
            EventInfo = text;
            _node = node;
        }

        public string GetInfo()
        {
            return EventInfo;
        }

        public Node DeletedNode => _node;
    }
}
