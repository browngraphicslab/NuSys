using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    class PdfNodeModel : Node
    {
        public PdfNodeModel(string filePath, int id) : base(id)
        {
            FilePath = filePath;
        }

        public string FilePath { get; set; }

        public BitmapImage RenderedBitmapImage { get; set; }
    }
}
