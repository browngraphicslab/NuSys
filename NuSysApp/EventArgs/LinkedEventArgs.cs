using System;

namespace NuSysApp
{
    public class LinkedEventArgs : System.EventArgs
    {
        
        private LinkModel _link;

        public LinkedEventArgs(LinkModel link)
        {
            _link = link;
        }

        public LinkModel Link
        {
            get
            {
                return _link;
            } 
        }

        public AtomModel Atom1
        {
            get { return _link.Atom1; }
        }

        public AtomModel Atom2
        {
            get { return _link.Atom2; }
        }

        public string ID
        {
            get { return _link.Id; }
        }
       
        
    }
}
