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
    }
}
