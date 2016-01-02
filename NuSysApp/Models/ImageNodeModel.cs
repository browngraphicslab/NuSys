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

        public override double Width
        {
            get
            {
                return base.Width;
            }

            set
            {
                if (Image.PixelWidth > Image.PixelHeight) { 
                    var r = Image.PixelHeight / (double)Image.PixelWidth;
                    base.Width = value;
                    base.Height = base.Width * r;
                } else
                {
                    var r = Image.PixelWidth / (double)Image.PixelHeight;
                    base.Width = base.Height* r;
                }
            }
        }

        public override double Height
        {
            get
            {
                return base.Height;
            }

            set
            {
                if (Image.PixelWidth > Image.PixelHeight)
                {
                    var r = Image.PixelHeight / (double)Image.PixelWidth;
                    base.Height = base.Width * r;

                } else
                {
                    var r = Image.PixelWidth / (double)Image.PixelHeight;
                    base.Height = value;
                    base.Width = base.Height * r;
                }

            }
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
            if (props.ContainsKey("data"))
            {

                var data = Convert.FromBase64String(props["data"]); //Converts to Byte Array
            //    MakeImage(byteArray); // Todo: don't call async methods from a ctor
            // Content = new NodeContentModel(byteArray, id);


                Image = await ByteArrayToBitmapImage(data);
            }

            FilePath = props.GetString("filepath", FilePath);

           await base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props.Add("filepath", FilePath);
            props.Add("data", Convert.ToBase64String(Content.Data));
            return props;
        }
    }
}
