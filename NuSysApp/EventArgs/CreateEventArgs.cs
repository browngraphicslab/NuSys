using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CreateEventArgs : System.EventArgs
    {
        private readonly string _eventInfo;
        private Node _node;

        public CreateEventArgs(string text, Node node)
        {
            _eventInfo = text;
            _node = node;
        }

        public string GetInfo()
        {
            return _eventInfo;
        }

        public Node CreatedNode => _node;
    }
}
