
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{

    public class LinkModel : AtomModel
    {
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);
        public event DeleteEventHandler OnDeletion;
        public LinkModel(AtomModel inAtom, AtomModel outAtom, string id) : base(id)
        {
            InAtomID = inAtom.Id;
            OutAtomID = outAtom.Id;
            Id = id;
            Atom1 = inAtom;
            Atom2 = outAtom;
        }

        public string InAtomID { get; set; }

        public string OutAtomID { get; set; }

        public override async Task UnPack(Message props)
        {
            InAtomID = props.GetString("id1", InAtomID);
            OutAtomID = props.GetString("id2", InAtomID);
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> dict = await base.Pack();
            dict.Add("id1",InAtomID);
            dict.Add("id2", OutAtomID);
            dict.Add("type","linq");
            return dict;
        }

        public NodeModel Annotation { get; set; }

        public AtomModel Atom1 { get; private set; }

        public AtomModel Atom2 { get; private set; }

    }
}
