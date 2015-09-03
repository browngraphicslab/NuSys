using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class AddToGroupEventArgs : SuperEventArgs
    {
        public AddToGroupEventArgs(string text, Group addedTo, Node sourceNode): base(text)
        {
            Group = addedTo;
            Node = sourceNode;
        }

        public Node Node { get;}

        public Group Group { get;}
    }
}
