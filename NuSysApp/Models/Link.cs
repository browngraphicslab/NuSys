﻿
namespace NuSysApp
{
    public class Link : Atom
    {
        public Link(Atom inAtom, Atom outAtom, int id) : base(id)
        {
            InAtomID = inAtom.ID;
            OutAtomID = outAtom.ID;
            ID = id;
        }

        public int InAtomID { get; set; }
        public int OutAtomID { get; set; }

        public AtomViewModel atom1 { get; set; }
        public AtomViewModel atom2 { get; set; }
    }
}
