using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageModel : Node
    {
        private BitmapImage _image;
        public ImageModel(BitmapImage img, string id) : base(id)
        {
            Image = img;
            this.NodeType = "ImageNode";
        }
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public string FilePath { get; set; }

        public override string GetContentSource()
        {
            return FilePath;
        }
    }
}
