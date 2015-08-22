using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageModel : Node
    {
        public ImageModel(BitmapImage img, string id) : base(id)
        {
            this.Image = img;
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
                byte[] imageBytes = Convert.FromBase64String(props["image"]);

                var stream = new InMemoryRandomAccessStream();
                var image = new BitmapImage();
                image.SetSource(stream);
                await stream.WriteAsync(imageBytes.AsBuffer());
                stream.Seek(0);
                Image = image;
            }
            if (props.ContainsKey("filepath"))
            {
                FilePath = props["filepath"];
            }
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("filepath",FilePath);

            string imageString = Convert.ToBase64String(ByteArray);
            props.Add("image",imageString);
            return props;
        }
    }
}
