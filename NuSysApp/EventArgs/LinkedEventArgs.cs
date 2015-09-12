using System;

namespace NuSysApp
{
    public class LinkedEventArgs : SuperEventArgs
    {
        
        private LinkModel _link;

        public LinkedEventArgs(string text, LinkModel link):base(text)
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
            get { return _link.ID; }
        }
       
        
    }
}
