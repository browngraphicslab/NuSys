using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageNodeViewModel : NodeViewModel
    {
        private ImageModel _imgm;

        public ImageNodeViewModel(WorkspaceViewModel vm, BitmapImage igm) : base(vm)
        {
            this.View = new ImageNodeView(this);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DEFAULT_NODE_SIZE;
            this.Height = Constants.DEFAULT_NODE_SIZE*igm.PixelHeight/igm.PixelWidth;//maintains aspect ratio
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.ImageModel = new ImageModel(igm, 0);
        }
        public override void Resize(double dx, double dy)
        {
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = (dy /*/ WorkSpaceViewModel.ScaleX*/) * ImageModel.Image.PixelWidth / ImageModel.Image.PixelHeight;
                newDy = dy;/// WorkSpaceViewModel.ScaleY;
            }
            else
            {
                newDx = dx; /// WorkSpaceViewModel.ScaleX;
                newDy = (dx /*/ WorkSpaceViewModel.ScaleY*/) * ImageModel.Image.PixelHeight / ImageModel.Image.PixelWidth;
            }
            base.Resize(newDx, newDy);
        }

        public ImageModel ImageModel
        {
            get { return _imgm; }
            set
            {
                if (_imgm == value)
                {
                    return;
                }
                _imgm = value;
                RaisePropertyChanged("ImageModel");
            }
        }

    }
}