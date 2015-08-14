using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageModel : Node
    {
        public ImageModel(BitmapImage img, int id) : base(id)
        {
            this.Image = img;
            this.NodeType = "ImageNode";
        }
        public BitmapImage Image { get; set; }

        public string FilePath { get; set; }

        public override string GetContentSource()
        {
            return FilePath;
        }
    }
}
