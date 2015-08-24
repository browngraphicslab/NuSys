using System;

namespace NuSysApp
{
    public class LinkedEventArgs : SuperEventArgs
    {
        
        private Link _link;

        public LinkedEventArgs(string text, Link link):base(text)
        {
 
            _link = link;
        }

        public Link Link
        {
            get
            {
                return _link;
            } 
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
       
        
    }
}
