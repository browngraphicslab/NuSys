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
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class PdfNodeViewModel : NodeViewModel
    {
        private readonly WorkspaceViewModel _workspaceViewModel;
        private CompositeTransform _inkScale;
        private Document _document;
        public int CurrentPageNumber { get;  private set; }
        public ObservableCollection<Button> SuggestedTags { get; set; }
        private List<string> _suggestedTags;

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
                _document = Document.Create(readBuffer, DocumentType.PDF, 140);
            }

            
            await Goto(CurrentPageNumber);
            SetSize(Width, Height);

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

        public void MakeTagList()
        {
            _suggestedTags.Add("hello");
            SuggestedTags = new ObservableCollection<Button>();
            foreach (string tag in _suggestedTags)
            {
                Button tagBlock = this.MakeTagBlock(tag);
                SuggestedTags.Add(tagBlock);
            }
        }

        public Button MakeTagBlock(string text)
        {
            Button tagBlock = new Button();
            tagBlock.Content = text;
            tagBlock.Foreground = new SolidColorBrush(Colors.White);
            tagBlock.FontStyle = FontStyle.Italic;
            tagBlock.Height = 40;
            tagBlock.Margin = new Thickness(2, 2, 2, 2);
            tagBlock.Padding = new Thickness(5);
            tagBlock.Background = new SolidColorBrush(Colors.Transparent);

            return tagBlock;
        }
    }
}
