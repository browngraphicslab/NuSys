using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TagNodeModel : ElementModel
    {
        public List<string> TitleSuggestions { get; set; } 

        public TagNodeModel(string id) : base(id)
        {
            TitleSuggestions = new List<string>();
        }

        public override async Task UnPack(Message props)
        {
            TitleSuggestions = props.GetList("titleSuggestions", new List<string>());
            await base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var packed =  await base.Pack();
            packed["titleSuggestions"] = TitleSuggestions;
            return packed;
        }
    }
}
