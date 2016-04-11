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

        //public ElementType InType { get; set; }

        //public ElementType OutType { get; set; }

        public Dictionary<string,object> InFGDictionary { get; set; }
        public Dictionary<string, object> OutFGDictionary { get; set; }


        public override async Task UnPack(Message props)
        {
            InAtomId = props.GetString("id1", InAtomId);
            //new
            InFGDictionary = props.GetDict<string, object>("inFGDictionary");
            OutFGDictionary = props.GetDict<string, object>("outFGDictionary");
            //InType = props. Get("inType");
            //OutType = props.Get("outType");
            //
            OutAtomId = props.GetString("id2", InAtomId);
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("id1", InAtomId);
            //new
            dict.Add("inFGDictionary", InFGDictionary);
            dict.Add("outFGDictionary", OutFGDictionary);
            //dict.Add("inType", InType);
            //dict.Add("outType", OutType);
            //
            dict.Add("id2", OutAtomId);
            dict.Add("type", ElementType.ToString());
            return dict;
        }
    }
}