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

        public async Task<BitmapImage> ByteArrayToBitmapImage(byte[] byteArray)
        {
            var bitmapImage = new BitmapImage();

            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(byteArray.AsBuffer());
            stream.Seek(0);

            bitmapImage.SetSource(stream);
            return bitmapImage;
        }

       
        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("contentId"))
            {
                var data =
                    Convert.FromBase64String(
                        SessionController.Instance.ContentController.Get(props.GetString("contentId", null)).Data);
                    //Converts to Byte Array
                Image = await ByteArrayToBitmapImage(data);
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
