using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {

        private BitmapImage _bitmapImage;
        private List<BitmapImage> _renderedPages;
        private PdfNodeModel _pdfNodeModel;
        private uint _currentPageNumber;
        private uint _pageCount;
        private readonly WorkspaceViewModel _workspaceViewModel;
        private CompositeTransform _inkScale;

        public PdfNodeViewModel(WorkspaceViewModel workspaceViewModel) : base(workspaceViewModel)
        {
            this.View = new PdfNodeView(this);
            this.PdfNodeModel = new PdfNodeModel(0);
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.CurrentPageNumber = 0;
            this.PageCount = 0;
            this.InkContainer = new List<InkStrokeContainer>();
            _workspaceViewModel = workspaceViewModel;
            var C = new CompositeTransform { 
                ScaleX = 1,
                ScaleY = 1
            };
            this.InkScale = C;
        }
        public async Task InitializePdfNodeAsync(StorageFile storageFile)
        {
            if (storageFile == null) return; // null if file explorer is closed by user
            var fileType = storageFile.FileType;
            switch (fileType)
            {
                case ".pdf":
                    await ProcessPdfFile(storageFile);
                    break;
                case ".pptx":
                    //TODO
                    break;
                case ".docx":
                    //TODO
                    break;
            }
        }

        private async Task ProcessPdfFile(StorageFile pdfStorageFile)
        {
            this.PageCount = await PdfRenderer.GetPageCount(pdfStorageFile);
            this.RenderedPages = await PdfRenderer.RenderPdf(pdfStorageFile);
            this.CurrentPageNumber = 0;
            var firstPage = RenderedPages[0]; // to set the aspect ratio of the node
            this.Width = Constants.DefaultNodeSize * 3;
            this.Height = Constants.DefaultNodeSize * 3 * firstPage.PixelHeight / firstPage.PixelWidth;
            this.InkContainer.Capacity = (int)this.PageCount;
            for (var i = 0; i < PageCount; i++)
            {
                this.InkContainer.Add(new InkStrokeContainer());
            }
        }

        public override void Resize(double dx, double dy)
        {
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = (dy /*/ WorkSpaceViewModel.ScaleX*/) * PdfNodeModel.RenderedPage.PixelWidth / PdfNodeModel.RenderedPage.PixelHeight;
                newDy = dy;// WorkSpaceViewModel.ScaleY;
            }
            else
            {
                newDx = dx; // WorkSpaceViewModel.ScaleX;
                newDy = (dx /*/ WorkSpaceViewModel.ScaleY*/) * PdfNodeModel.RenderedPage.PixelHeight / PdfNodeModel.RenderedPage.PixelWidth;
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

        public PdfNodeModel PdfNodeModel
        {
            get { return _pdfNodeModel; }
            set
            {
                if (_pdfNodeModel == value)
                {
                    return;
                }
                _pdfNodeModel = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public BitmapImage RenderedBitmapImage
        {
            get { return _bitmapImage; }
            set
            {
                _bitmapImage = value;
                _pdfNodeModel.RenderedPage = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public List<BitmapImage> RenderedPages
        {
            get { return _renderedPages; }
            set
            {
                _renderedPages = value;
                _pdfNodeModel.RenderedPages = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public uint CurrentPageNumber
        {
            get { return _currentPageNumber; }
            set
            {
                _currentPageNumber = value;
                _pdfNodeModel.CurrentPageNumber = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }

        public uint PageCount
        {
            get { return _pageCount; }
            set
            {
                _pageCount = value;
                _pdfNodeModel.PageCount = value;
                RaisePropertyChanged("PdfNodeModel");
            }
        }
     //   public List<IReadOnlyList<InkStroke>> InkContainer { get; set;}
        public List<InkStrokeContainer> InkContainer { get; set; }

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
