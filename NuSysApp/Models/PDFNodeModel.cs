using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using System.Diagnostics;

namespace NuSysApp
{
    public class PdfNodeModel : Node
    {
        public delegate void PdfImagesCreatedHandler();
        public event PdfImagesCreatedHandler OnPdfImagesCreated;

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
            InqLines = new List<HashSet<InqLineModel>>();
            InqLines.Capacity = (int) PageCount;
            for (int i = 0; i < PageCount; i++)
            {
                InqLines.Add(new InqCanvasModel(ID).Lines);
            }
            /*
            InkContainer = new List<HashSet<InqLineView>>();
            InkContainer.Capacity = (int) PageCount;
            for (var i = 0; i < PageCount; i++)
            {
                InkContainer.Add(new HashSet<InqLineView>());
            }
            */
            OnPdfImagesCreated?.Invoke();

        }



        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("nodeType", NodeType.PDF.ToString());
            props.Add("data",Convert.ToBase64String(ByteArray));
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
                if (RenderedPages == null) {
                    base.Width = value;
                    return;
                }
                var RenderedPage = RenderedPages[0];
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
                var RenderedPage = RenderedPages[0];
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
        //public List<HashSet<InqLineView>> InkContainer { get; set; }

        public List<HashSet<InqLineModel>> InqLines { get; set; }

        private byte[] ByteArray { set; get; }
        public List<BitmapImage> RenderedPages { get; set; }
    }
}
