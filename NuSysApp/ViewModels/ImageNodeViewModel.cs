using NuStarterProjects;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuStarterProject
{
    public class ImageNodeViewModel : NodeViewModel
    {
        private UserControl _view;
        private BitmapImage _image;
        private ImageModel _imgm;
        public ImageNodeViewModel(WorkspaceViewModel vm, ImageModel igm) : base(vm)
        {
            this.View = new ImageNodeView(this);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DEFAULT_NODE_SIZE;
            this.Height = Constants.DEFAULT_NODE_SIZE;
            this.IsSelected = false;
            this.IsEditing = false;
            this._imgm = igm;
        }

        public override UserControl View
        {
            get { return _view; }
            set { _view = value; }
        }

        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                if (_image == value)
                {
                    return;
                }
                _image = value;
                RaisePropertyChanged("Image");
            }
        }
        public ImageModel Imgm
        {
            get { return _imgm; }
            set
            {
                if (_imgm == value)
                {
                    return;
                }
                _imgm = value;
            }
        }
        public Uri ImageSource
        {
            get { return Imgm.Image.UriSource; }
        }
    }
}
