using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using LdaLibrary;
using NusysIntermediate;

namespace NuSysApp
{
    public class WordNodeViewModel : ElementViewModel
    {
        public int CurrentPageNumber { get; private set; }
        public MuPDFWinRT.Document _document;

        private bool _isLocked;

        public bool IsLocked
        {
            get
            {
                return _isLocked;
            }
            set
            {
                _isLocked = value;
                RaisePropertyChanged("Islocked");
            }
        }
        public WordNodeViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            Debug.Assert(controller?.LibraryElementController is WordNodeLibraryElementController);
            var wnlec = controller?.LibraryElementController as WordNodeLibraryElementController;
            wnlec.Locked += LibraryElementController_Locked;
            wnlec.UnLocked += LibraryElementController_UnLocked;
            controller.LibraryElementController.ContentChanged += ChangeContent;
        }
        private void ChangeContent(object source, string contentData)
        {
            Task.Run(async delegate {
                _document = await MediaUtil.DataToPDF(Controller.LibraryElementModel.Data);
                await UITask.Run(async delegate { await Goto(CurrentPageNumber); });
            });
        }
        private void LibraryElementController_UnLocked(object sender)
        {
            IsLocked = false;
        }

        private void LibraryElementController_Locked(object sender, NetworkUser user)
        {
            IsLocked = true;
        }

        public override void Dispose()
        {
            var model = (WordNodeModel)Controller.Model;
            if (_document != null)
            {
                _document.Dispose();
            }
            base.Dispose();
        }

        public async override Task Init()
        {
            if (Controller.LibraryElementController.IsLoaded)
            {
                await DisplayPdf();
            }
            else
            {
                Controller.LibraryElementController.Loaded += LibraryElementModelOnOnLoaded;
            }
        }

        private async void LibraryElementModelOnOnLoaded(object sender)
        {
            UITask.Run(async delegate {
                await DisplayPdf();
            });
        }

        private async Task DisplayPdf()
        {
            if (Controller.LibraryElementModel == null || Controller.LibraryElementModel.Data == null)
            {
                return;
            }
            if (Controller.LibraryElementModel.Data == "docx too large")
            {
                _document = null;
            }
            else
            {
                _document = await MediaUtil.DataToPDF(Controller.LibraryElementModel.Data);
            }
            await Goto(CurrentPageNumber);
            SetSize(Width, Height);
            //LaunchLDA((PdfNodeModel)this.Model);
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
            await RenderPage(pageNumber);
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

        protected override void OnSizeChanged(object source, double width, double height)
        {
            SetSize(width, height);
        }

        public string GetAllText()
        {
            string data = "";
            int numPages = _document.PageCount;
            int currPage = 0;
            while (currPage < numPages)
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

                string data = GetAllText();
                
                DieStopWords ds = new DieStopWords();
                data = await ds.removeStopWords(data);
                List<string> topics = await TagExtractor.launch(test, new List<string>() { data });
                await UITask.Run(() =>
                {
                    var topicKeywords = new HashSet<Keyword>();
                    foreach (var topic in topics)
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
                this._document = value;
            }
        }

        public WriteableBitmap ImageSource
        {
            get; set;
        }

        public Button MakeTagBlock(string text)
        {
            Button tagBlock = new Button();
            tagBlock.Content = text;
            tagBlock.Foreground = new SolidColorBrush(Constants.foreground6);
            tagBlock.FontStyle = FontStyle.Italic;
            tagBlock.Height = 40;
            tagBlock.Margin = new Thickness(2, 2, 2, 2);
            tagBlock.Padding = new Thickness(5);
            tagBlock.Background = new SolidColorBrush(Colors.Transparent);

            return tagBlock;
        }
    }
}