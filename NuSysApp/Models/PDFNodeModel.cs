using SQLite.Net.Attributes;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class PdfNodeModel : Node
    {
        //public PdfNodeModel(string filePath, int id) : base(id)
        //{
        //    FilePath = filePath;
        //}
        private uint _currentPageNum;
        public PdfNodeModel(int id) : base(id)
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
        
        [Column("PageCount")]
        public uint PageCount { get; set; }
    }
}
