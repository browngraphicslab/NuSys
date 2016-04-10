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

        public string InType { get; set; }

        public string OutType { get; set; }

        public Dictionary<string,object> InFGDictionary { get; set; }
        public Dictionary<string, object> OutFGDictionary { get; set; }


        public override async Task UnPack(Message props)
        {
            InAtomId = props.GetString("id1", InAtomId);
            //new
            InFGDictionary = props.GetDict<string, object>("inFGDict");
            OutFGDictionary = props.GetDict<string, object>("outFGDict");
            InType = props.GetString("InType", InType);
            OutType = props.GetString("OutType", OutType);
            //
            OutAtomId = props.GetString("id2", InAtomId);
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("id1", InAtomId);
            //new
            dict.Add("inFGDict", InFGDictionary);
            dict.Add("outFGDict", OutFGDictionary);
            dict.Add("InType", InType);
            dict.Add("OutType", OutType);
            //
            dict.Add("id2", OutAtomId);
            dict.Add("type", ElementType.ToString());
            return dict;
        }
    }
}