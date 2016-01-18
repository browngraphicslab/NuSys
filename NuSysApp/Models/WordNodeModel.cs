using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class WordNodeModel : NodeModel
    {
        public WordNodeModel(string id) : base(id)
        {
            NodeType = NodeType.Word;
        }

        public string FilePath { get; set; }
       
        public override async Task UnPack(Message props)
        {
            await base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            return props;
        }
    }
}
