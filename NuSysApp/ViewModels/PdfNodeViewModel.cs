using System;
using System.Collections.Generic;
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
            NodeType = NodeType.PDF;

            model.UnPacked += async delegate(object source)
            {
                InitPdfViewer();
            };
        }

        private async void InitPdfViewer()
        {
            CurrentPageNumber = ((PdfNodeModel)Model).CurrentPageNumber;
            var ms = new MemoryStream(((PdfNodeModel)Model).Content.Data);
            using (IInputStream inputStreamAt = ms.AsInputStream())
            using (var dataReader = new DataReader(inputStreamAt))
            {
                uint u = await dataReader.LoadAsync((uint)((PdfNodeModel)Model).Content.Data.Length);
                IBuffer readBuffer = dataReader.ReadBuffer(u);
                _document = Document.Create(readBuffer, DocumentType.PDF, 140);
            }

            Width = Width;

            await Goto(CurrentPageNumber);
        }

        public override async Task Init(UserControl view)
        {
            await base.Init(view);
            if (Model.IsUnpacked)
                InitPdfViewer();
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
            
            await Task.Run(() =>
            {
                _document.DrawPage(pageNumber, buf, 0, 0, width, height, false);
            });

            var s = buf.AsStream();
            await s.CopyToAsync(image.PixelBuffer.AsStream());
            image.Invalidate();
            ImageSource = image;
            RaisePropertyChanged("ImageSource");

        }

        public override double Width
        {
            get { return base.Width; }
            set
            {
                if (_document == null)
                {
                    base.Width = value;
                    return;
                }

                var pageSize = _document.GetPageSize(CurrentPageNumber);
                if (pageSize.X > pageSize.Y)
                {
                    var r = pageSize.Y / (double)pageSize.X;
                    base.Width = value;
                    base.Height = base.Width * r;
                }
                else
                {
                    var r = pageSize.X / (double)pageSize.Y;
                    base.Width = base.Height * r;
                }
            }
        }

        /// <summary>
        /// Height of this atom
        /// </summary>
        public override double Height
        {
            get { return base.Height; }
            set
            {
                if (_document == null)
                {
                    base.Height = value;
                    return;
                }

                var pageSize = _document.GetPageSize(CurrentPageNumber);

                if (pageSize.X > pageSize.Y)
                {
                    var r = pageSize.Y / (double)pageSize.X;
                    base.Height = base.Width * r;

                }
                else
                {
                    var r = pageSize.X / (double)pageSize.Y;
                    base.Height = value;
                    base.Width = base.Height * r;
                }
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
