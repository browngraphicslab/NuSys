using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class PdfNodeModel : Node
    {
        //public PdfNodeModel(string filePath, int id) : base(id)
        //{
        //    FilePath = filePath;
        //}
        public PdfNodeModel(int id) : base(id)
        {
            
        }

        //public string FilePath { get; set; }

        public BitmapImage RenderedPage { get; set; }
    }
}
