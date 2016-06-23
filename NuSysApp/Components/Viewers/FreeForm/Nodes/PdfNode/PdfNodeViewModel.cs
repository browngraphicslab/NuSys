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
using NuSysApp.Components.Viewers.FreeForm;
using System.Net;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class PdfNodeViewModel : ElementViewModel, Sizeable
    {
        private CompositeTransform _inkScale;
        public int CurrentPageNumber { get;  private set; }
        public MuPDFWinRT.Document _document;
        public ObservableCollection<Button> SuggestedTags { get; set; }
        private List<string> _suggestedTags = new List<string>();
        public ObservableCollection<PDFRegionView> RegionViews { private set; get; }

        public Sizeable View { get; set; }

        public PdfNodeViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            var model = (PdfNodeModel) controller.Model;
            model.PageChange += OnPageChange;
            CurrentPageNumber = model.CurrentPageNumber;

            RegionViews = new ObservableCollection<PDFRegionView>();
            RegionViews = this.CreatePdfRegionViews();
            
            Controller.LibraryElementController.RegionAdded += LibraryElementControllerOnRegionAdded;
            Controller.LibraryElementController.RegionRemoved += LibraryElementController_RegionRemoved;
            
        }

        private void LibraryElementController_RegionRemoved(object source, Region region)
        {
            RegionViews = this.CreatePdfRegionViews();
            RaisePropertyChanged("RegionViews");
        }

        private void LibraryElementControllerOnRegionAdded(object source, Region region)
        {
            var pdfRegion = region as PdfRegion;
            if (pdfRegion == null)
            {
                return;
            }

            var regionController = new RegionController(pdfRegion);
            var vm = new PdfRegionViewModel(pdfRegion, Controller.LibraryElementController, regionController, this);
            vm.Editable = false;
            var view = new PDFRegionView(vm);

            if (pdfRegion.PageLocation != CurrentPageNumber+1)
            {
                view.Visibility = Visibility.Collapsed;
            }
            RegionViews.Add(view);
            RaisePropertyChanged("RegionViews");
        }

        public ObservableCollection<PDFRegionView> CreatePdfRegionViews()
        {
            var regionViewCollection = new ObservableCollection<PDFRegionView>();
            foreach (var regionModel in Controller.LibraryElementModel.Regions)
            {
                var pdfRegion = regionModel as PdfRegion;
                if (pdfRegion == null)
                {
                    return null;
                }

                var regionController = new RegionController(pdfRegion);
                var vm = new PdfRegionViewModel(pdfRegion, Controller.LibraryElementController, regionController, this);
                vm.Editable = false;
                var view = new PDFRegionView(vm);
                if (pdfRegion.PageLocation != CurrentPageNumber)
                {
                    view.Visibility = Visibility.Collapsed;
                }
                regionViewCollection.Add(view);

            }
            return regionViewCollection;
            
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
                await DisplayPdf();
            }
            else
            {
                Controller.LibraryElementController.Loaded += LibraryElementModelOnOnLoaded;
            }
        }

        private async void LibraryElementModelOnOnLoaded(object sender)
        {
            await DisplayPdf();
        }


        private async Task DisplayPdf()
        {
            if (Controller.LibraryElementModel == null || Controller.LibraryElementModel.Data == null) {
                return;
            }
            _document = await MediaUtil.DataToPDF(Controller.LibraryElementModel.Data);
            await Goto(CurrentPageNumber);
            SetSize(Width, Height);
            //LaunchLDA((PdfNodeModel)this.Model);
        }

        private async void OnPageChange(int page)
        {
            CurrentPageNumber = page;
            await RenderPage(page);

        }

        public async Task FlipRight()
        {
            await Goto(CurrentPageNumber + 1);
            foreach (var regionView in RegionViews)
            {
                var model = (regionView.DataContext as PdfRegionViewModel)?.Model;
                if ((model as PdfRegion).PageLocation != CurrentPageNumber)
                {
                    regionView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    regionView.Visibility = Visibility.Visible;
                }
            }
        }

        public async Task FlipLeft()
        {
            await Goto(CurrentPageNumber - 1);
            foreach (var regionView in RegionViews)
            {
                var model = (regionView.DataContext as PdfRegionViewModel)?.Model;
                if ((model as PdfRegion).PageLocation != CurrentPageNumber)
                {
                    regionView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    regionView.Visibility = Visibility.Visible;
                }
            }
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
            tagBlock.Foreground = new SolidColorBrush(Constants.foreground6);
            tagBlock.FontStyle = FontStyle.Italic;
            tagBlock.Height = 40;
            tagBlock.Margin = new Thickness(2, 2, 2, 2);
            tagBlock.Padding = new Thickness(5);
            tagBlock.Background = new SolidColorBrush(Colors.Transparent);

            return tagBlock;
        }

        public double GetWidth()
        {
            return View.GetWidth();
        }

        public double GetHeight()
        {
            return View.GetHeight();
        }

       public void SizeChanged(object sender, double width, double height)
       {
           var newHeight = View.GetHeight();
           var newWidth = View.GetWidth();

            foreach (var rv in RegionViews)
            {
                var regionViewViewModel = rv.DataContext as RegionViewModel;
                regionViewViewModel?.ChangeSize(sender, newWidth, newHeight);
            }
        }

    }
}
