using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageModel : Node
    {
        private BitmapImage _image;
        public ImageModel(BitmapImage img, int id) : base(id)
        {
            Image = img;
        }
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }
    }
}
