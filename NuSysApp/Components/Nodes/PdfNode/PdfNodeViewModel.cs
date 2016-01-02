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

namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {
        private readonly WorkspaceViewModel _workspaceViewModel;
        private CompositeTransform _inkScale;
        private Document _document;
        private int CurrentPageNumber = 0;

        public PdfNodeViewModel(PdfNodeModel model) : base(model)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            RenderedLines = new HashSet<InqLineModel>();
        }

        public async Task InitPdfViewer()
        {
            var data = SessionController.Instance.ContentController.Get(ContentId).Data;
            var dataBytes = Convert.FromBase64String(data);
            CurrentPageNumber = ((PdfNodeModel)Model).CurrentPageNumber;
            var ms = new MemoryStream(dataBytes);
            using (IInputStream inputStreamAt = ms.AsInputStream())
            using (var dataReader = new DataReader(inputStreamAt))
            {
                uint u = await dataReader.LoadAsync((uint)dataBytes.Length);
                IBuffer readBuffer = dataReader.ReadBuffer(u);
                _document = Document.Create(readBuffer, DocumentType.PDF, 140);
            }

            
            await Goto(CurrentPageNumber);
            SetSize(Width, Height);
        }

        public async void FlipRight()
        {
            Goto(CurrentPageNumber + 1);
        }

        public async void FlipLeft()
        {
            Goto(CurrentPageNumber - 1);
        }

        public async Task Goto(int pageNumber)
        {
            if (pageNumber == -1) return;
            if (pageNumber >= (_document.PageCount)) return;
            CurrentPageNumber = pageNumber;
            await RenderPage(pageNumber);
            ((PdfNodeModel) Model).CurrentPageNumber = CurrentPageNumber;
        }

        private async Task RenderPage(int pageNumber)
        {
            var pageSize = _document.GetPageSize(pageNumber);
            var width = pageSize.X;
            var height = pageSize.Y;
            var image = new WriteableBitmap(width, height);
            IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf.Length = image.PixelBuffer.Length;
            


            //_document.SearchText(pageNumber).
            
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

       
        public WriteableBitmap ImageSource
        {
            get; set;
        }

        public HashSet<InqLineModel> RenderedLines { get; set; }

        public List<HashSet<InqLineModel>> InqPages
        {
            get { return null; }
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
