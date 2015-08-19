using System.Collections.Generic;
using System.Xml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class PdfNodeModel : Node
    {
        //public PdfNodeModel(string filePath, int id) : base(id)
        //{
        //    FilePath = filePath;
        //}
        private uint _currentPageNum;
        public PdfNodeModel(string id) : base(id)
        {

        }

        //public string FilePath { get; set; }

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
        public List<HashSet<Polyline>> InkContainer { get; set; }
    }
}
