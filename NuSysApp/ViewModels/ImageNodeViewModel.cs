using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageNodeViewModel : NodeViewModel
    {
        //private ImageModel _imgm;
        private CompositeTransform _inkScale;
        public ImageNodeViewModel(ImageModel model, WorkspaceViewModel vm, string id, BitmapImage igm) : base(model, vm, id)
        {
            this.View = new ImageNodeView2(this);
            this.Transform = new MatrixTransform();
            this.Width = igm.PixelWidth;
            this.Height = igm.PixelHeight;
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            this.NodeType = NodeType.Image; //Also sets model value



            
            var C = new CompositeTransform
            {
                ScaleX = 1,
                ScaleY = 1,
                CenterX = 0,
                CenterY = 0
            };
            this.InkScale = C;
        }
        public ImageNodeViewModel(ImageModel model, WorkspaceViewModel vm, string id) : base(model, vm, id)
        {
            this.NodeType = NodeType.Image; //Also sets model value
            this.View = new ImageNodeView2(this);
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

        public async Task InitializeImageNodeAsync(StorageFile storageFile)
        {
            if (storageFile == null) return; // null if file explorer is closed by user
            if (!Constants.ImageFileTypes.Contains(storageFile.FileType.ToLower())) return;
            using (var fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                //this.Model = new ImageModel(bitmapImage,this.ID);//TODO - should not initialize a new model here
                ((ImageModel)Model).Image = bitmapImage;
                ((ImageModel)Model).FilePath = storageFile.Path;
                this.Width = bitmapImage.PixelWidth;
                this.Height = bitmapImage.PixelHeight;
                var C = new CompositeTransform
                {
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
                newDx = dy * ((ImageModel)Model).Image.PixelWidth / ((ImageModel)Model).Image.PixelHeight;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * ((ImageModel)Model).Image.PixelHeight / ((ImageModel)Model).Image.PixelWidth;
            }
            if (newDx / WorkSpaceViewModel.CompositeTransform.ScaleX + Width <= Constants.MinNodeSizeX || newDy / WorkSpaceViewModel.CompositeTransform.ScaleY + Height <= Constants.MinNodeSizeY)
            {
                return;
            }
            CompositeTransform ct = this.InkScale;
            ct.ScaleX *= (Width + newDx / WorkSpaceViewModel.CompositeTransform.ScaleX) / Width;
            ct.ScaleY *= (Height + newDy / WorkSpaceViewModel.CompositeTransform.ScaleY) / Height;
            this.InkScale = ct;

            base.Resize(newDx, newDy);
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