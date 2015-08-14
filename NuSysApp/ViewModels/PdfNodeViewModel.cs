using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NuSysApp.MISC;
using Windows.UI.Xaml;
using System.Xml;
using Windows.UI.Input.Inking;

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
            this.View = new PdfNodeView2(this);
            this.PdfNodeModel = new PdfNodeModel(0);
            this.Model = this.PdfNodeModel;
            this.Transform = new MatrixTransform();
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.NodeType = Constants.NodeType.pdf;
            this.CurrentPageNumber = 0;
            this.PageCount = 0;
            this.InkContainer = new List<Dictionary<Windows.UI.Xaml.Shapes.Polyline,InkStroke>>();
            _workspaceViewModel = workspaceViewModel;
            var C = new CompositeTransform {
                ScaleX = 1,
                ScaleY = 1
            };
            this.InkScale = C;
        }

        /// <summary>
        /// Takes in a storageFile, converts it to PDF if possible, and opens the PDF in the workspace.
        /// </summary>
        /// <param name="storageFile"></param>
        /// <returns></returns>
        public async Task InitializePdfNodeAsync(StorageFile storageFile)
        {
            if (storageFile == null) return; // null if file explorer is closed by user
            var fileType = storageFile.FileType;
            switch (fileType)
            {
                case ".pdf":
                    await ProcessPdfFile(storageFile);
                    break;
                case ".docx":
                case ".pptx":
                    await WatchForOfficeConversions(storageFile);
                    break;
            }
        }

        /// <summary>
        /// Takes in a storageFile (either .docx or .pptx), waits for OfficeInterop to convert
        /// it to PDF, and opens the PDF in the workspace.
        /// </summary>
        /// <param name="storageFile"></param>
        /// <returns></returns>
        private async Task WatchForOfficeConversions(StorageFile storageFile)
        {
            var taskComplete = false;
            var folder = NuSysStorages.OfficeToPdfFolder;
            string previousPathToPdf = null;
            var folderWatcher = new FolderWatcher(NuSysStorages.OfficeToPdfFolder);
            folderWatcher.FilesChanged += async () =>
            {
                var files = await NuSysStorages.OfficeToPdfFolder.GetFilesAsync();
                foreach (var pdfPathFile in files.Where(file => file.Name == "path_to_pdf.nusys"))
                {
                    var tempPath = await FileIO.ReadTextAsync(pdfPathFile);
                    if (tempPath == previousPathToPdf) continue;
                    previousPathToPdf = tempPath;
                    var pdfFilePath = tempPath;
                    if (string.IsNullOrEmpty(pdfFilePath)) continue;
                    storageFile = await StorageFile.GetFileFromPathAsync(pdfFilePath);
                    taskComplete = true;
                }
            };
            var outputFile = await StorageUtil.CreateFileIfNotExists(folder, "path_to_office.nusys");
            await FileIO.WriteTextAsync(outputFile, storageFile.Path); // write path to office file
            while (!taskComplete) { await Task.Delay(50); } // loop until office file is converted and opened in workspace
            await DeleteInteropTransferFiles(); // to prevent false file-change notifications
            await ProcessPdfFile(storageFile); // process the .pdf StoragFeile
        }

        /// <summary>
        /// Deletes all .nusys files involved in the office to PDF conversion process
        /// in order to prevent false-flags and accidental creation of PDF nodes.
        /// </summary>
        /// <returns></returns>
        private static async Task DeleteInteropTransferFiles()
        {
            var path = NuSysStorages.OfficeToPdfFolder.Path;
            var pathToOfficeFile = await StorageFile.GetFileFromPathAsync(path + @"\path_to_office.nusys");
            var pathToPdfFile = await StorageFile.GetFileFromPathAsync(path + @"\path_to_pdf.nusys");
            await pathToOfficeFile.DeleteAsync(StorageDeleteOption.PermanentDelete); // PermanentDelete bypasses the Recycle Bin
            await pathToPdfFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }


        /// <summary>
        /// Takes in a .pdf StorageFile and renders it in the workspace.
        /// </summary>
        /// <param name="pdfStorageFile"></param>
        /// <returns></returns>
        private async Task ProcessPdfFile(StorageFile pdfStorageFile)
        {
            this.RenderedPages = await PdfRenderer.RenderPdf(pdfStorageFile);
            this.PageCount = (uint)this.RenderedPages.Count();
            this.CurrentPageNumber = 0;
            var firstPage = RenderedPages[0]; // to set the aspect ratio of the node
            this.Width = firstPage.PixelWidth;
            this.Height = firstPage.PixelHeight;
            this.InkContainer.Capacity = (int)this.PageCount;
            for (var i = 0; i < PageCount; i++)
            {
                this.InkContainer.Add(new Dictionary<Windows.UI.Xaml.Shapes.Polyline, InkStroke>());

            }
        }

        public override void Resize(double dx, double dy)
        {
            double newDx, newDy;
            if (dx > dy)
            {
                newDx = dy * PdfNodeModel.RenderedPage.PixelWidth / PdfNodeModel.RenderedPage.PixelHeight;
                newDy = dy;
            }
            else
            {
                newDx = dx;
                newDy = dx * PdfNodeModel.RenderedPage.PixelHeight / PdfNodeModel.RenderedPage.PixelWidth;
            }
            if (newDx / WorkSpaceViewModel.CompositeTransform.ScaleX + Width <= Constants.MinNodeSizeX || newDy / WorkSpaceViewModel.CompositeTransform.ScaleY + Height <= Constants.MinNodeSizeY)
            {
                return;
            }
            CompositeTransform ct = this.InkScale;
            ct.ScaleX *= (Width + newDx / WorkSpaceViewModel.CompositeTransform.ScaleX) / Width;
            ct.ScaleY *= (Height + newDy / WorkSpaceViewModel.CompositeTransform.ScaleY) / Height;
            base.Resize(newDx, newDy);
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {
            PdfNodeModel currModel = (PdfNodeModel)this.Model;

            //XmlElement 
            XmlElement pdfNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name
            doc.AppendChild(pdfNode);

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                pdfNode.SetAttributeNode(attr);
            }

            return pdfNode;
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
        public List<Dictionary<Windows.UI.Xaml.Shapes.Polyline,InkStroke>> InkContainer { get; set; }

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
