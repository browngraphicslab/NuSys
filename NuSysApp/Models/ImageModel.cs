using NuStarterProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace NuStarterProjects
{
    public class ImageModel : Node
    {
        private BitmapImage _image;
        public ImageModel(Uri img, int id) : base(id)
        {
            _image = new BitmapImage();
            _image.UriSource = img;
        }
        internal void setPicture(Uri uri)
        {
            Image.UriSource = uri;
        }
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }
    }
}
