using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageNodeViewModel : NodeViewModel
    {
        private CompositeTransform _inkScale;

        public ImageNodeViewModel(ImageNodeModel model) : base(model)
        {
            this.NodeType = NodeType.Image; //Also sets model value
            this.View = new ImageNodeView(this);
            this.Transform = new CompositeTransform();
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            var C = new CompositeTransform
            {
                ScaleX = 1,
                ScaleY = 1
            };
            this.InkScale = C;
        }

        public override void Resize(double dx, double dy)
        { /*
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = dy * ((ImageNodeModel)Model).Image.PixelWidth / ((ImageNodeModel)Model).Image.PixelHeight;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * ((ImageNodeModel)Model).Image.PixelHeight / ((ImageNodeModel)Model).Image.PixelWidth;
            }
            if ((newDx / WorkSpaceViewModel.CompositeTransform.ScaleX + Width <= Constants.MinNodeSizeX && dx < 1) || (newDy / WorkSpaceViewModel.CompositeTransform.ScaleY + Height <= Constants.MinNodeSizeY && dy < 1))
            {
                return;
            }
            CompositeTransform ct = this.InkScale;
            ct.ScaleX *= (Width + newDx / WorkSpaceViewModel.CompositeTransform.ScaleX) / Width;
            ct.ScaleY *= (Height + newDy / WorkSpaceViewModel.CompositeTransform.ScaleY) / Height;
            this.InkScale = ct;

            base.Resize(newDx, newDy);
            */
            
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