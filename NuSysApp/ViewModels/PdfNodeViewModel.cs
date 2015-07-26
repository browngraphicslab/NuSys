using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NuSysApp.MISC;

namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {

        //private readonly string _filePath;
        private BitmapImage _bitmapImage;
        private List<BitmapImage> _renderedPages; 
        private PdfNodeModel _pdfNodeModel;
        private uint _currentPageNumber;
        private uint _pageCount;
        private readonly WorkspaceViewModel _workspaceViewModel;

        public PdfNodeViewModel(WorkspaceViewModel workspaceViewModel) : base(workspaceViewModel)
        {
            this.View = new PdfNodeView(this);
            this.PdfNodeModel = new PdfNodeModel(0);
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.IsEditing = false;
            this.CurrentPageNumber = 0;
            this.PageCount = 0;
            _workspaceViewModel = workspaceViewModel;
        }

        public async Task InitializePdfNodeAsync()
        {
            var storageFile = await FileManager.PromptUserForFile(new List<string> { ".pdf", ".pptx", ".docx" });
            var fileName = storageFile.Name;
            var fileType = storageFile.FileType;
            if (fileType == ".pdf")
            {
                this.PageCount = await PdfRenderer.GetPageCount(fileName);
                this.RenderedPages = await PdfRenderer.RenderPdf(fileName);
                this.CurrentPageNumber = 0;
                var firstPage = RenderedPages[0]; // to set the aspect ratio of the node
                this.Width = Constants.DEFAULT_NODE_SIZE * 3;
                this.Height = Constants.DEFAULT_NODE_SIZE * 3 * firstPage.PixelHeight / firstPage.PixelWidth;
                _workspaceViewModel.CurrentMode = WorkspaceViewModel.Mode.PDF;
                var p = _workspaceViewModel.CompositeTransform.Inverse.TransformPoint(new Point(0,0));
                _workspaceViewModel.CreateNewNode(p.X, p.Y, null);
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

    }
}
