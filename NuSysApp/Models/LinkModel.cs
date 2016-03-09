using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkModel : ElementModel
    {
        public LinkModel(string id) : base(id)
        {
            Id = id;
            ElementType = ElementType.Link;
        }

        public string InAtomId { get; set; }

        public string OutAtomId { get; set; }

        public override async Task UnPack(Message props)
        {
            InAtomId = props.GetString("id1", InAtomId);
            OutAtomId = props.GetString("id2", InAtomId);
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("id1", InAtomId);
            dict.Add("id2", OutAtomId);
            dict.Add("type", ElementType.ToString());
            return dict;
        }
    }
}