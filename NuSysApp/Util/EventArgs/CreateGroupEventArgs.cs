using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CreateGroupEventArgs : SuperEventArgs
    {

        public CreateGroupEventArgs(string text, Group group):base(text)
        {
            CreatedGroup = group;
        }

        public Group CreatedGroup { get; }
    }
}
