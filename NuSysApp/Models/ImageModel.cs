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
    public class ImageModel : Node
    {
        public ImageModel(byte[] byteArray, string id) : base(id)
        {
            ByteArray = byteArray;
            MakeImage(byteArray); // Todo: don't call async methods from a ctor
            Content = new Content(byteArray, id);
        }

        private async Task MakeImage(byte[] bytes)
        {
            Image = await ByteArrayToBitmapImage(bytes);
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

        public byte[] ByteArray { get; set; }

        public override string GetContentSource()
        {
            return FilePath;
        }

        public async Task<BitmapImage> ByteArrayToBitmapImage(byte[] byteArray)
        {
            var bitmapImage = new BitmapImage();

            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(byteArray.AsBuffer());
            stream.Seek(0);

            bitmapImage.SetSource(stream);
            return bitmapImage;
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {
            //Main XmlElement 
            XmlElement imageNode = doc.CreateElement(string.Empty, "Node", string.Empty);

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                imageNode.SetAttributeNode(attr);
            }

            //Source for image
            XmlAttribute source = doc.CreateAttribute("Source");
            source.Value = this.FilePath;
            imageNode.SetAttributeNode(source);

            return imageNode;
        }

        public override async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("image"))
            {
                ByteArray = Convert.FromBase64String(props["image"]); //Converts to Byte Array

                var stream = new InMemoryRandomAccessStream();
                var image = new BitmapImage();
                await stream.WriteAsync(ByteArray.AsBuffer());
                stream.Seek(0);
                image.SetSource(stream);
                Image = image;
            }
            if (props.ContainsKey("filepath"))
            {
                FilePath = props["filepath"];
            }
            base.UnPack(props);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("filepath",FilePath);
            props.Add("image", Convert.ToBase64String(ByteArray));
            props.Add("nodeType",NodeType.Image.ToString());
            return props;
        }
    }
}
