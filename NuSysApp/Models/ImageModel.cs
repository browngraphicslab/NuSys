using System.Collections.Generic;
using System.Xml;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageModel : Node
    {
        private BitmapImage _image;
        public ImageModel(BitmapImage img, string id) : base(id)
        {
            this.Image = img;
        }
        public BitmapImage Image { get; set; }

        public string FilePath { get; set; }

        public override string GetContentSource()
        {
            return FilePath;
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
    }
}
