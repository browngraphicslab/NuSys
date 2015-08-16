
namespace NuSysApp
{
    public class Link : Atom
    {
        public Link(Atom inAtom, Atom outAtom, string id) : base(id)
        {
            InAtomID = inAtom.ID;
            OutAtomID = outAtom.ID;
            ID = id;
        }
        public string InAtomID { get; set; }
        public string OutAtomID { get; set; }
    }
}
