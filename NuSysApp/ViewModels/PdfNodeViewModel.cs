using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NuSysApp.MISC;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {

        private BitmapImage _bitmapImage;
        private List<BitmapImage> _renderedPages;
        private PdfNodeModel _pdfNodeModel;
        private uint _currentPageNumber;
        private uint _pageCount;
        //private readonly WorkspaceViewModel _workspaceViewModel;

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
            this.inkManager = new InkManager();
            //_workspaceViewModel = workspaceViewModel;
        }

        public async Task InitializePdfNodeAsync(StorageFile storageFile)
        {
            if (storageFile == null) return; // null if file explorer is closed by user
            var fileType = storageFile.FileType;
            StorageFolder folder;
            StorageFile outputFile;
            var folderWatcher = new FolderWatcher(NuSysStorages.OfficeToPdfFolder);
            switch (fileType)
            {
                case ".pdf":
                    await ProcessPdfFile(storageFile);
                    break;
                case ".pptx":
                    var complete = false;
                    folder = NuSysStorages.OfficeToPdfFolder;
                    //outputFile = await StorageUtil.CreateFileIfNotExists(folder, "path_to_pptx.nusys");
                    folderWatcher.FilesChanged += async () =>
                    {
                        var files = await NuSysStorages.OfficeToPdfFolder.GetFilesAsync();
                        foreach (var nusysFile in files.Where(file => file.Name == "path_to_pdf.nusys"))
                        {
                            var pdfFilePath = await FileIO.ReadTextAsync(nusysFile);
                            if (string.IsNullOrEmpty(pdfFilePath)) continue;
                            Debug.WriteLine("received pptx to pdf file path: " + pdfFilePath);

                            //var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                            //await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                            //{
                            //    storageFile = await StorageFile.GetFileFromPathAsync(pdfFilePath);
                            //    await ProcessPdfFile(storageFile);
                            //    Debug.WriteLine("checkpoint");
                            //});

                            /* An exception of type 'System.Runtime.InteropServices.COMException' occurred in mscorlib.ni.dll
                            but was not handled in user code.
                            Additional information: The application called an interface that was marshalled for a different
                            thread. (Exception from HRESULT: 0x8001010E (RPC_E_WRONG_THREAD)) */
                            ////storageFile = await StorageFile.GetFileFromPathAsync(pdfFilePath);
                            ////await ProcessPdfFile(storageFile);

                            //await ProcessPdfFile(pdfFilePath);
                            complete = true;
                        }
                        //complete = true;

                        //var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                        //var count = 0;
                        //foreach (var file in transferFiles)
                        //{
                        //    Debug.WriteLine(file.Path);

                        //    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        //    {
                        //        var readFile = await FileIO.ReadTextAsync(file);
                        //        //var nodeVm = _factory.CreateNewRichText(readFile);
                        //        var nodeVm = Factory.CreateNewRichText(this, readFile);
                        //        var p = CompositeTransform.Inverse.TransformPoint(new Point((count++) * 250, 200));
                        //        PositionNode(nodeVm, p.X, p.Y);
                        //        NodeViewModelList.Add(nodeVm);
                        //        AtomViewList.Add(nodeVm.View);
                        //    });
                        //}
                    };
                    outputFile = await StorageUtil.CreateFileIfNotExists(folder, "path_to_pptx.nusys");
                    await FileIO.WriteTextAsync(outputFile, storageFile.Path);
                    while (!complete) { }
                    break;
                case ".docx":
                    folder = NuSysStorages.OfficeToPdfFolder;
                    outputFile = await StorageUtil.CreateFileIfNotExists(folder, "path_to_pptx.nusys");
                    folderWatcher.FilesChanged += async delegate
                    {
                        var files = await NuSysStorages.OfficeToPdfFolder.GetFilesAsync();
                        foreach (var nusysFile in files.Where(file => file.Name == "path_docx_to_pdf.nusys"))
                        {
                            var pdfFilePath = await FileIO.ReadTextAsync(nusysFile);
                            if (string.IsNullOrEmpty(pdfFilePath)) continue;
                            Debug.WriteLine("received docx to pdf file path: " + pdfFilePath);
                            storageFile = await StorageFile.GetFileFromPathAsync(pdfFilePath);
                            await ProcessPdfFile(storageFile);
                        }
                    };
                    await FileIO.WriteTextAsync(outputFile, storageFile.Path);
                    break;
            }
        }

        private async Task ProcessPdfFile(string pdfFilePath)
        {
            var storageFile = await StorageFile.GetFileFromPathAsync(pdfFilePath);
            await ProcessPdfFile(storageFile);
        }

        private async Task ProcessPdfFile(StorageFile pdfStorageFile)
        {
            this.RenderedPages = await PdfRenderer.RenderPdf(pdfStorageFile);
            this.PageCount = (uint)this.RenderedPages.Count();
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
                newDx = (dy /*/
                            WorkSpaceViewModel.ScaleX*/) * PdfNodeModel.RenderedPage.PixelWidth / PdfNodeModel.RenderedPage.PixelHeight;
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
     //   public List<IReadOnlyList<InkStroke>> InkContainer { get; set;}
        public List<InkStrokeContainer> InkContainer { get; set; }
        public InkManager inkManager { get; set; }


    }
}
