using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;
using NuSysApp.Viewers;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class LinkModel : ElementModel
    {
        public LinkModel(string id) : base(id)
        {
            Id = id;
            ElementType = ElementType.Link;
        }

        public bool IsPresentationLink { get; set; }

        public LinkId InAtomId { get; set; }

        public LinkId OutAtomId { get; set; }
        public string Annotation { get; set; }

        //public LinkedTimeBlockModel InFineGrain { get; set; }

        //TODO: public RegionView
        
        public Region InFineGrain { set; get; }
        public RectangleViewModel RectangleMod { get; set; }

        public Dictionary<string, object> InFGDictionary { get; set; }
        public Dictionary<string, object> OutFGDictionary { get; set; }

        public override async Task UnPack(Message props)
        {
            IsPresentationLink = props.GetBool("isPresentationLink", false);
            InAtomId = JsonConvert.DeserializeObject<LinkId>(props.GetString("id1"));
            OutAtomId = JsonConvert.DeserializeObject<LinkId>(props.GetString("id2"));
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
            dict.Add("inFineGrain", InFineGrain);
            dict.Add("isPresentationLink", IsPresentationLink);
            dict.Add("rectangleModel", RectangleMod);
            return dict;
        }
    }
}