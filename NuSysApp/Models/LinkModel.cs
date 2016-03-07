using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkModel : ElementModel
    {
        public LinkModel(ElementModel inElement, ElementModel outElement, string id) : base(id)
        {
            InAtomID = inElement.Id;
            OutAtomID = outElement.Id;
            Id = id;
            Atom1 = inElement;
            Atom2 = outElement;
            ElementType = ElementType.Link;
        }

        public string InAtomID { get; set; }

        public string OutAtomID { get; set; }

        public ElementModel Atom1 { get; private set; }

        public ElementModel Atom2 { get; private set; }

        public override async Task UnPack(Message props)
        {
            InAtomID = props.GetString("id1", InAtomID);
            OutAtomID = props.GetString("id2", InAtomID);
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("id1", InAtomID);
            dict.Add("id2", OutAtomID);
            dict.Add("type", ElementType.ToString());
            return dict;
        }
    }
}