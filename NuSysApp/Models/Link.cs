
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public override void Update(Dictionary<string, string> props)
        {
            if (props.ContainsKey("id1"))
            {
                this.InAtomID = props["id1"];
            }
            if (props.ContainsKey("id2"))
            {
                this.InAtomID = props["id2"];
            }
            base.Update(props);
        }
    }
}
