using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class AddToGroupEventArgs : SuperEventArgs
    {
        public AddToGroupEventArgs(string text, GroupModel addedTo, NodeModel sourceNode): base(text)
        {
            Group = addedTo;
            Node = sourceNode;
        }

        public NodeModel Node { get;}

        public GroupModel Group { get;}
    }
}
