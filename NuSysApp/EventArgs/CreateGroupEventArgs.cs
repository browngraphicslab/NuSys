using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CreateGroupEventArgs
    {
        private readonly string _eventInfo;

        public CreateGroupEventArgs(string text, Group group)
        {
            _eventInfo = text;
            CreatedGroup = group;
        }

        public string GetInfo()
        {
            return _eventInfo;
        }

        public Group CreatedGroup { get; }
    }
}
