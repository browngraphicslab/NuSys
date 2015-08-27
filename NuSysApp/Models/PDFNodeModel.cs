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
            var firstPage = RenderedPages[0];
            Width = firstPage.PixelWidth;
            Height = firstPage.PixelHeight;
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

        public uint PageCount { get; set; }
        public List<HashSet<InqLine>> InkContainer { get; set; }

        private byte[] ByteArray { set; get; }
        public BitmapImage RenderedPage { get; set; }
        public List<BitmapImage> RenderedPages { get; set; }
    }
}
