using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;

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

        public LinkedTimeBlockModel InFineGrain { get; set; }

        public Dictionary<string, object> InFGDictionary { get; set; }
        public Dictionary<string, object> OutFGDictionary { get; set; }

        public override async Task UnPack(Message props)
        {
            InAtomId = props.GetString("id1", InAtomId);
            OutAtomId = props.GetString("id2", InAtomId);
            InFGDictionary = props.GetDict<string, object>("inFGDictionary");
            OutFGDictionary = props.GetDict<string, object>("outFGDictionary");
            Annotation = props.GetString("annotation", "");
            if (props.ContainsKey("inFineGrain"))
            {
                var v = JsonConvert.DeserializeObject<LinkedTimeBlockModel>(props.Get("inFineGrain"));
                var list =
                    ((AudioNodeModel) SessionController.Instance.IdToControllers[OutAtomId].Model)
                        .LinkedTimeModels;
                foreach (var element in list)
                {
                    if (element.Start == v.Start && element.End == v.End)
                    {
                        InFineGrain = element;
                        Debug.WriteLine(InFineGrain.ToString());
                        //Debug.WriteLine(InFineGrain.);
                        break;
                    }
                }
               

            }
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
            dict.Add("inFineGrain", InFineGrain);

            return dict;
        }
    }
}