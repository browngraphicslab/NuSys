using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageElementModel : ElementModel
    {
        public ImageElementModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.Image;
        }

        public string FilePath { get; set; }
       
        public override async Task UnPack(Message props)
        {
            await base.UnPack(props);
            if (props.ContainsKey("filepath"))
            {
                FilePath = props.GetString("filepath", FilePath);
            }
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props.Add("filepath", FilePath);
          //  props.Add("data", Convert.ToBase64String(Content.Data));
            return props;
        }
    }
}
