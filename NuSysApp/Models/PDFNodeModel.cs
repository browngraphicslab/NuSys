using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.MISC;
using System.Diagnostics;

namespace NuSysApp
{
    public class PdfNodeModel : Node
    {
        private uint _currentPageNum;

        public PdfNodeModel(byte[] bytes,string id) : base(id)
        {
            ByteArray = bytes;
            Content = new Content(ByteArray, id);
        }

        public async Task SaveFile()
        {
            StorageFolder folder = NuSysStorages.Media;
            StorageFile file = await folder.CreateFileAsync(ID + ".pdf", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, ByteArray);

            RenderedPages = await PdfRenderer.RenderPdf(file);
            PageCount = (uint)RenderedPages.Count;
            InkContainer = new List<HashSet<InqLine>>();
            InkContainer.Capacity = (int) PageCount;
            for (var i = 0; i < PageCount; i++)
            {
                InkContainer.Add(new HashSet<InqLine>());
            }

        }



        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("type", NodeType.PDF.ToString());
            return props;
        }

        public override async Task UnPack(Dictionary<string, string> props)
        {
            base.UnPack(props);
        }

        public uint CurrentPageNumber
        {
            get { return _currentPageNum; }
            set
            {
                _currentPageNum = value;
                if (RenderedPages == null) return;
                RenderedPage = RenderedPages[(int)value];
            }
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {  
            //XmlElement 
            XmlElement pdfNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                pdfNode.SetAttributeNode(attr);
            }

            return pdfNode;
        }

        public override double Width
        {
            get
            {
                return base.Width;
            }

            set
            {
                if (RenderedPage == null) {
                    base.Width = value;
                    return;
                }
                if (RenderedPage.PixelWidth > RenderedPage.PixelHeight)
                {
                    var r = RenderedPage.PixelHeight / (double)RenderedPage.PixelWidth;
                    base.Width = value;
                    base.Height = base.Width * r;
                }
                else
                {
                    var r = RenderedPage.PixelWidth / (double)RenderedPage.PixelHeight;
                    base.Width = base.Height * r;
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
                if (RenderedPage == null)
                {
                    base.Height = value;
                    return;
                }

                if (RenderedPage.PixelWidth > RenderedPage.PixelHeight)
                {
                    var r = RenderedPage.PixelHeight / (double)RenderedPage.PixelWidth;
                    base.Height = base.Width * r;

                }
                else
                {
                    var r = RenderedPage.PixelWidth / (double)RenderedPage.PixelHeight;
                    base.Height = value;
                    base.Width = base.Height * r;
                }

            }
        }

        public uint PageCount { get; set; }
        public List<HashSet<InqLine>> InkContainer { get; set; }

        private byte[] ByteArray { set; get; }
        public BitmapImage RenderedPage { get; set; }
        public List<BitmapImage> RenderedPages { get; set; }
    }
}
