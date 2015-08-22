using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkedEventArgs : System.EventArgs
    {
        private readonly string _eventInfo;
        private Link _link;

        public LinkedEventArgs(string text, Link link)
        {
            _eventInfo = text;
            _link = link;
        }

        public Atom Atom1
        {
            get { return _link.Atom1; }
        }

        public Atom Atom2
        {
            get { return _link.Atom2; }
        }

        public string ID
        {
            get { return _link.ID; }
        }
        public string GetInfo()
        {
            return _eventInfo;
        }

        
    }
}
