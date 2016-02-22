using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkModel : ElementInstanceModel
    {
        public LinkModel(ElementInstanceModel inElementInstance, ElementInstanceModel outElementInstance, string id) : base(id)
        {
            InAtomID = inElementInstance.Id;
            OutAtomID = outElementInstance.Id;
            Id = id;
            Atom1 = inElementInstance;
            Atom2 = outElementInstance;
            Type = ElementType.Link;
        }

        public string InAtomID { get; set; }

        public string OutAtomID { get; set; }

        public ElementInstanceModel Atom1 { get; private set; }

        public ElementInstanceModel Atom2 { get; private set; }

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
            dict.Add("type", Type.ToString());
            return dict;
        }
    }
}