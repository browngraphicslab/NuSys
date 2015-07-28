using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
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
            this.ImageModel = new ImageModel(igm, 0);
        }

        public ImageNodeViewModel(WorkspaceViewModel vm) : base(vm)
        {
            this.View = new ImageNodeView(this);
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.IsEditing = false;
        }

        public async Task InitializeImageNodeViewModel(StorageFile storageFile)
        {
            if (storageFile == null) return; // null if file explorer is closed by user
            var supportedFileTypes = Constants.IMAGE_FILE_TYPES;
            if (!supportedFileTypes.Contains(storageFile.FileType.ToLower())) return;
            using (var fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                this.ImageModel = new ImageModel(bitmapImage, 0);
                this.Width = Constants.DEFAULT_NODE_SIZE;
                this.Height = Constants.DEFAULT_NODE_SIZE * bitmapImage.PixelHeight / bitmapImage.PixelWidth;
            }
        }

        public override void Resize(double dx, double dy)
        {
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = dy * ImageModel.Image.PixelWidth / ImageModel.Image.PixelHeight;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * ImageModel.Image.PixelHeight / ImageModel.Image.PixelWidth;
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