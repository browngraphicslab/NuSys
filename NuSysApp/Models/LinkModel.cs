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
        public string Annotation { get; set; }

        public Dictionary<string, object> InFGDictionary { get; set; }
        public Dictionary<string, object> OutFGDictionary { get; set; }

        public override async Task UnPack(Message props)
        {
            InAtomId = props.GetString("id1", InAtomId);
            OutAtomId = props.GetString("id2", InAtomId);
            InFGDictionary = props.GetDict<string, object>("inFGDictionary");
            OutFGDictionary = props.GetDict<string, object>("outFGDictionary");

            Annotation = props.GetString("annotation", "");
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("id1", InAtomId);
            dict.Add("id2", OutAtomId);
            dict.Add("inFGDictionary", InFGDictionary);
            dict.Add("outFGDictionary", OutFGDictionary);
            dict.Add("type", ElementType.ToString());
            dict.Add("annotation", Annotation);
            return dict;
        }
    }
}