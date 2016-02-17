using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas.Brushes;
using MuPDFWinRT;
using LdaLibrary;

namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {
        private readonly WorkspaceViewModel _workspaceViewModel;
        private CompositeTransform _inkScale;
        private MuPDFWinRT.Document _document;
        public int CurrentPageNumber { get;  private set; }

        public PdfNodeViewModel(PdfNodeModel model) : base(model)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            model.PageChange += OnPageChange;
            CurrentPageNumber = model.CurrentPageNumber;
        }

        private async void OnPageChange(int page)
        {
            CurrentPageNumber = page;
            await RenderPage(page);

        }

        public async Task InitPdfViewer()
        {
            var content = SessionController.Instance.ContentController.Get(ContentId);
            if (content == null)
                return;

            var data = content.Data;
            var dataBytes = Convert.FromBase64String(data);
            var ms = new MemoryStream(dataBytes);
            using (IInputStream inputStreamAt = ms.AsInputStream())
            using (var dataReader = new DataReader(inputStreamAt))
            {
                uint u = await dataReader.LoadAsync((uint)dataBytes.Length);
                IBuffer readBuffer = dataReader.ReadBuffer(u);
                _document = MuPDFWinRT.Document.Create(readBuffer, DocumentType.PDF, 140);
            }

            
            await Goto(CurrentPageNumber);
            SetSize(Width, Height);
            LaunchLDA((PdfNodeModel)this.Model);

            var text = _document.GetAllTexts(0);
            Debug.WriteLine(text);
        }

        public async Task FlipRight()
        {
            await Goto(CurrentPageNumber + 1);
        }

        public async Task FlipLeft()
        {
            await Goto(CurrentPageNumber - 1);
        }

        public async Task Goto(int pageNumber)
        {
            if (pageNumber == -1) return;
            if (pageNumber >= (_document.PageCount)) return;
            CurrentPageNumber = pageNumber;
            ((PdfNodeModel)Model).CurrentPageNumber = CurrentPageNumber;
            
        }

        private async Task RenderPage(int pageNumber)
        {
            var pageSize = _document.GetPageSize(pageNumber);
            var width = pageSize.X;
            var height = pageSize.Y;
            var image = new WriteableBitmap(width, height);
            IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf.Length = image.PixelBuffer.Length;
            
            _document.DrawPage(pageNumber, buf, 0, 0, width, height, false);
            
            var s = buf.AsStream();
            await s.CopyToAsync(image.PixelBuffer.AsStream());
            image.Invalidate();
            ImageSource = image;
            RaisePropertyChanged("ImageSource");
        }


        public override void SetSize(double width, double height)
        {
  
            if (ImageSource.PixelWidth > ImageSource.PixelHeight)
            {
                var r = ImageSource.PixelHeight / (double)ImageSource.PixelWidth;
                base.SetSize(width, width * r);
            }
            else
            {
                var r = ImageSource.PixelWidth / (double)ImageSource.PixelHeight;
                base.SetSize(height * r, height);
            }
        }

        public async Task LaunchLDA(PdfNodeModel model)
        {
            var test = new List<string>();
            // here we hard code our starting parameters
            string filename = model.Title;
            test.Add(filename);
            test.Add("niters 10");
            test.Add("ntopics 1");
            test.Add("twords 10");
            test.Add("dir ");
            test.Add("est true");
            test.Add("alpha 12.5");
            test.Add("beta .1");
            test.Add("model model-final");

            string data = "";
            int numPages = _document.PageCount;
            int currPage = 0;
            while (currPage < numPages)
            {
                data = data + _document.GetAllTexts(currPage);
                currPage++;
            }
            //StorageFolder folder = await KnownFolders.PicturesLibrary.GetFolderAsync("bla");
            //StorageFile file = await TagExtractor.CreateFileIfNotExists(folder, model.Title + ".txt");
            //string data = await FileIO.ReadTextAsync(file);


            DieStopWords ds = new DieStopWords();
            data = await ds.removeStopWords(data);
            List<string> topics = await TagExtractor.launch(test, new List<string>() { data });

        }

        public WriteableBitmap ImageSource
        {
            get; set;
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
