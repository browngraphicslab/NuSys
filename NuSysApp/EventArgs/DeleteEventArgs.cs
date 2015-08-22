using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class DeleteEventArgs : System.EventArgs
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
