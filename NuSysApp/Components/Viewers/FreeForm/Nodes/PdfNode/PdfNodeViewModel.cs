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
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class PdfNodeViewModel : ElementViewModel
    {
        private CompositeTransform _inkScale;
        private bool _isSelected;
        public int CurrentPageNumber { get;  private set; }
        public MuPDFWinRT.Document _document;
        public ObservableCollection<Button> SuggestedTags { get; set; }
        private List<string> _suggestedTags = new List<string>();

        public PdfNodeViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            var model = (PdfNodeModel) controller.Model;
            model.PageChange += OnPageChange;
            CurrentPageNumber = model.CurrentPageNumber;
            if (controller.LibraryElementModel.Type == ElementType.PdfRegion)
            {
                var pdfRegionModel = controller.LibraryElementModel as PdfRegionModel;
                CurrentPageNumber = pdfRegionModel.PageLocation;
            }

        }           

        public override void Dispose()
        {
            var model = (PdfNodeModel)Controller.Model;
            model.PageChange -= OnPageChange;
            if (_document != null)
            _document.Dispose();
            base.Dispose();
        }

        public async override Task Init()
        {
            if (Controller.LibraryElementController.IsLoaded)
            {
                //await DisplayPdf();
                await DisplayPlaceholderThumbnail();
            }
            else
            {
                Controller.LibraryElementController.Loaded += LibraryElementModelOnOnLoaded;
            }
        }

        private async void LibraryElementModelOnOnLoaded(object sender)
        {
            this.DisplayPlaceholderThumbnail();
        }


        private async Task DisplayPdf()
        {

            if (Controller.LibraryElementModel == null || Controller.LibraryElementModel.Data == null) {
                return;
            }
            _document = await MediaUtil.DataToPDF(Controller.LibraryElementModel.Data);
            await Goto(CurrentPageNumber);
            SetSize(Width, Height);
        }

        /// <summary>
        /// Displays the placeholder thumbnail by retreiving the placholder image and setting the ImageSource to it
        /// </summary>
        /// <returns></returns>
        private async Task DisplayPlaceholderThumbnail()
        {
            // Get the storage file of the placeholder thumbnail
            string root =
               Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            string path = root + @"\Assets";
            StorageFile file =
                await StorageFile.GetFileFromPathAsync(path + @"\placeholder_pdf_thumbnail.png");

            // Save the storage file as a writeable bitmap and set the ImageSource to this bitmap
            using (var stream = await file.OpenReadAsync())
            {
                WriteableBitmap bitmap = new WriteableBitmap(400, 600);
                await bitmap.SetSourceAsync(stream);
                bitmap.Invalidate();
                ImageSource = bitmap;
                RaisePropertyChanged("ImageSource");
            }
            
        }

        private async void OnPageChange(int page)
        {
            CurrentPageNumber = page;
            await RenderPage(page);
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
            if (_document == null)
                return;
            if (pageNumber == -1) return;
            if (pageNumber >= (_document.PageCount)) return;
            CurrentPageNumber = pageNumber;
            ((PdfNodeModel)Model).CurrentPageNumber = CurrentPageNumber;
        }

        private async Task RenderPage(int pageNumber)
        {
            if (_document == null)
                return;
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
            if (ImageSource == null)
                return;

            if (Controller.LibraryElementModel is PdfRegionModel)
            {
                var rect = Controller.LibraryElementModel as PdfRegionModel;

                if (ImageSource.PixelWidth * rect.Width > ImageSource.PixelHeight * rect.Height)
                {
                    var r = ((double)ImageSource.PixelHeight * rect.Height) / ((double)ImageSource.PixelWidth * rect.Width);
                    base.SetSize(width, width * r);
                }
                else
                {
                    var r = ((double)ImageSource.PixelWidth * rect.Width) / ((double)ImageSource.PixelHeight * rect.Height);
                    base.SetSize(height * r, height);
                }
            }
            else
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

        }

        protected override void OnSizeChanged(object source, double width, double height)
        {
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

            SetSize(width, height);
        }


        public string GetAllText()
        {
            string data = "";
            int numPages = _document.PageCount;
            int currPage = 0;
            while (currPage<numPages)
            {
                data = data + _document.GetAllTexts(currPage);
                currPage++;
            }
            return data;
        }

        public async Task LaunchLDA(PdfNodeModel model)
        {

            Task.Run(async () =>
            {
                var test = new List<string>();

                // parameters for our LDA algorithm
                string filename = model.Title;
                test.Add(filename);
                test.Add("niters 8");
                test.Add("ntopics 5");
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


                DieStopWords ds = new DieStopWords();
                data = await ds.removeStopWords(data);
                List<string> topics = await TagExtractor.launch(test, new List<string>() {data});
                await UITask.Run(() =>
                {
                    var topicKeywords = new HashSet<Keyword>();
                    foreach(var topic in topics)
                    {
                        topicKeywords.Add(new Keyword(topic, Keyword.KeywordSource.TopicModeling));
                    }
                    Controller.LibraryElementController.SetKeywords((topicKeywords));
                    RaisePropertyChanged("Tags");
                });
            });
        }

        public MuPDFWinRT.Document Document
        {
            get
            {
                return this._document;
            }
            set
            {
               this. _document = value;
            }
        }

        public WriteableBitmap ImageSource
        {
            get; set;
        }
    }
}
