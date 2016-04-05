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

        //public FillInDict(double x, double y, double r)
        //{
        //}

        //public FillInDict(double wordnumber)
        //{
        //}

        public string InAtomId { get; set; }

        public string OutAtomId { get; set; }

        public Dictionary<string,string> InAtomFinegrainedDictionary { get; set; } 

        public override async Task UnPack(Message props)
        {
            InAtomId = props.GetString("id1", InAtomId);
            //InAtomFinegrainedDictionary = props.GetDict<string, string>("fgdict");
            OutAtomId = props.GetString("id2", InAtomId);
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("id1", InAtomId);
            //dict.Add("fgdict", InAtomFinegrainedDictionary);
            dict.Add("id2", OutAtomId);
            dict.Add("type", ElementType.ToString());
            return dict;
        }
    }
}