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
        private CompositeTransform _inkScale;
      
        public ImageNodeViewModel(WorkspaceViewModel vm, BitmapImage igm) : base(vm)
        {
            this.View = new ImageNodeView(this);
            this.Transform = new MatrixTransform();
            this.Width = igm.PixelWidth;
            this.Height = igm.PixelHeight;//maintains aspect ratio
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.ImageModel = new ImageModel(igm, 0);
            var C = new CompositeTransform { 
                ScaleX = 1,
                ScaleY = 1
            };
            this.InkScale = C;
        }

        public ImageNodeViewModel(WorkspaceViewModel vm) : base(vm)
        {
            this.View = new ImageNodeView(this);
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.IsEditing = false;
        }

        public async Task InitializeImageNodeAsync(StorageFile storageFile)
        {
            if (storageFile == null) return; // null if file explorer is closed by user
            if (!Constants.ImageFileTypes.Contains(storageFile.FileType.ToLower())) return;
            using (var fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                this.ImageModel = new ImageModel(bitmapImage, 0);
                this.Width = Constants.DefaultNodeSize;
                this.Height = Constants.DefaultNodeSize * bitmapImage.PixelHeight / bitmapImage.PixelWidth;
            var C = new CompositeTransform { 
                ScaleX = 1,
                ScaleY = 1
            };
            this.InkScale = C;
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
            if (newDx + Width <= Constants.MinNodeSize || newDy + Width <= Constants.MinNodeSize)
            {
                return;
            }
            CompositeTransform ct = this. InkScale;
            ct.ScaleX *= (newDx + Width) / Width;
            ct.ScaleY *= (newDy + Height) / Height;
            this.InkScale = ct;
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

        public CompositeTransform InkScale
        {
            get { return _inkScale; }
            set
            {
                if (_inkScale == value)
                {
                    return;
                }
                _inkScale = value;
                RaisePropertyChanged("InkScale");
            }
        }
    }
}