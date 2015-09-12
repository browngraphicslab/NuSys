using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CreateGroupEventArgs : SuperEventArgs
    {

        public CreateGroupEventArgs(string text, GroupNodeModel group):base(text)
        {
            CreatedGroup = group;
        }

        public GroupNodeModel CreatedGroup { get; }
    }
}
