using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {
        private readonly WorkspaceViewModel _workspaceViewModel;
        private CompositeTransform _inkScale;
        public PdfNodeViewModel(PdfNodeModel model) : base(model)
        {
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            this.NodeType = NodeType.PDF;
            this.CurrentPageNumber = model.CurrentPageNumber;
            this.RenderedLines = InqPages[0];
           // this.View = new PdfNodeView(this);
 
            var C = new CompositeTransform {
                ScaleX = 1,
                ScaleY = 1
            };
            this.InkScale = C;


            model.OnPageChange += delegate
            {
                Goto(model.CurrentPageNumber);
            };

            model.OnPdfImagesCreated += delegate
            {
                Goto(model.CurrentPageNumber);
            };

            model.AddedToGroup += delegate(object source, AddToGroupEventArgs args)
            {
                Goto(model.CurrentPageNumber);
            };

          
        }

        public override void Init(UserControl view)
        {
            base.Init(view);

            if (((PdfNodeModel)Model).RenderedPages != null)
                Goto(0);
        }

        public void FlipRight()
        {
            Goto(CurrentPageNumber + 1);
        }

        public void FlipLeft()
        {
            Goto(CurrentPageNumber-1);
        }

        public void Goto(uint pageNumber)
        {
            if (pageNumber == -1) return;
            if (pageNumber >= (PageCount)) return;
            CurrentPageNumber = pageNumber;
            RenderedBitmapImage = RenderedPages[(int)CurrentPageNumber];
            RenderedLines = InqPages[(int)CurrentPageNumber]; 
            RaisePropertyChanged("RenderedBitmapImage");
        }

        public override void Resize(double dx, double dy)
        {
       
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = dy * RenderedBitmapImage.PixelWidth / RenderedBitmapImage.PixelHeight;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * RenderedBitmapImage.PixelHeight / RenderedBitmapImage.PixelWidth;
            }
            if (newDx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX + Width <= Constants.MinNodeSizeX || newDy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY + Height <= Constants.MinNodeSizeY)
            {
                return;
            }
            CompositeTransform ct = this.InkScale;
            ct.ScaleX *= (Width + newDx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX) / Width;
            ct.ScaleY *= (Height + newDy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY) / Height;
            base.Resize(newDx, newDy);
            
        }

        public BitmapImage RenderedBitmapImage
        {
            get; set;
        }

        public HashSet<InqLineModel> RenderedLines { get; set; }

        public List<BitmapImage> RenderedPages
        {
            get { return ((PdfNodeModel)Model).RenderedPages; }
            set
            {
                ((PdfNodeModel)Model).RenderedPages = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public List<HashSet<InqLineModel>> InqPages
        {
            get { return ((PdfNodeModel) Model).InqLines; }
        }

        public uint CurrentPageNumber
        {
            get { return ((PdfNodeModel)Model).CurrentPageNumber; }
            set
            {
                ((PdfNodeModel)Model).CurrentPageNumber = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public uint PageCount
        {
            get { return ((PdfNodeModel)Model).PageCount; }
            set
            {
                ((PdfNodeModel)Model).PageCount = value;
                RaisePropertyChanged("PdfNodeModel");
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
