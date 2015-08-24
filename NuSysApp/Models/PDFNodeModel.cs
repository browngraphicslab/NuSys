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

namespace NuSysApp
{
    public class PdfNodeModel : Node
    {
        //public PdfNodeModel(string filePath, int id) : base(id)
        //{
        //    FilePath = filePath;
        //}
        private uint _currentPageNum;
        public PdfNodeModel(byte[] bytes,string id) : base(id)
        {
            ByteArray = bytes;
        }

        public async Task SaveFile()
        {
            StorageFolder folder = NuSysStorages.Media;
            StorageFile file = await folder.CreateFileAsync(ID+".pdf", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, ByteArray);

            RenderedPages = await PdfRenderer.RenderPdf(file);
            PageCount = (uint)RenderedPages.Count;
            CurrentPageNumber = 0;
            var firstPage = RenderedPages[0]; // to set the aspect ratio of the node
            Width = firstPage.PixelWidth;
            Height = firstPage.PixelHeight;
            InkContainer = new List<HashSet<InqLine>>();
            InkContainer.Capacity = (int) PageCount;
            for (var i = 0; i < PageCount; i++)
            {
                InkContainer.Add(new HashSet<InqLine>());
            }
        }
        //public string FilePath { get; set; }
        private byte[] ByteArray { set; get; }
        public BitmapImage RenderedPage { get; set; }
        public List<BitmapImage> RenderedPages { get; set; }

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

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("type",NodeType.PDF.ToString());
            //props.Add("pdf",await System.IO.File.ReadAllBytes(path));
            return props;
        }
        //TODO add in UnPack function
    }
}
