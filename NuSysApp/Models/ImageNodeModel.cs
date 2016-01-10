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
    public class ImageNodeModel : NodeModel
    {
        public ImageNodeModel(string id) : base(id)
        {
            NodeType = NodeType.Image;
        }

        

        public BitmapImage Image { get; set; }

        public string FilePath { get; set; }


       
        public override async Task UnPack(Message props)
        {

            var d = SessionController.Instance.ContentController.Get(props.GetString("contentId", null))?.Data;
            if (d != null)
            { 
                var data = Convert.FromBase64String(d); //Converts to Byte Array
                Image = await MediaUtil.ByteArrayToBitmapImage(data);
            }

            FilePath = props.GetString("filepath", FilePath);
            await base.UnPack(props);
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
