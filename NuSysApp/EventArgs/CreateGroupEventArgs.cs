using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CreateGroupEventArgs : SuperEventArgs
    {

        public CreateGroupEventArgs(string text, GroupModel group):base(text)
        {
            CreatedGroup = group;
        }

        public GroupModel CreatedGroup { get; }
    }
}
