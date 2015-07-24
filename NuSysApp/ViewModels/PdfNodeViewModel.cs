using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NuSysApp.MISC;

namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {

        //private readonly string _filePath;
        private BitmapImage _bitmapImage;
        private PdfNodeModel _pdfNodeModel;
        private readonly WorkspaceViewModel _workspaceViewModel;

        public PdfNodeViewModel(WorkspaceViewModel workspaceViewModel) : base(workspaceViewModel)
        {
            this.View = new PdfNodeView(this);
            this.Transform = new MatrixTransform();
            //_pdfNodeModel = new PdfNodeModel("FILEPATH HERE", 0); // TODO: BIND FILEPATH DATA TO PDFNODEMODEL
            _pdfNodeModel = new PdfNodeModel(0);
            _workspaceViewModel = workspaceViewModel;
            //this.RenderedBitmapImage = InitializePdfNode().Result;
        }

        public async Task InitializePdfNode()
        {
            //_bitmapImage = await PdfRenderer.RenderPdfPage(_filePath, 0);
            var storageFile = await FileManager.PromptUserForFile(new List<string> {".pdf", ".pptx", ".docx"});
            var fileName = storageFile.Name;
            var fileType = storageFile.FileType;
            if (fileType == ".pdf")
            {
                this.RenderedBitmapImage = await PdfRenderer.RenderPdfPage(fileName, 0);
                _workspaceViewModel.CurrentMode = WorkspaceViewModel.Mode.PDF;
                var pdfNodeViewModel = _workspaceViewModel.CreateNewNode(0, 0, null) as PdfNodeViewModel;
                if (pdfNodeViewModel == null) return;
                _workspaceViewModel.CurrentMode = WorkspaceViewModel.Mode.IMAGE;
                _workspaceViewModel.CreateNewNode(0, 0, RenderedBitmapImage);
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
            }
        }

    }
}
