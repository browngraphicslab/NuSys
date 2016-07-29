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
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Image = SharpDX.Direct2D1.Image;

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
            this.CreatePdfRegionViews();
            
            Controller.LibraryElementController.RegionAdded += LibraryElementControllerOnRegionAdded;
            Controller.LibraryElementController.RegionRemoved += LibraryElementController_RegionRemoved; 
            
        }

        private void LibraryElementController_RegionRemoved(object source, Region region)
        {
            var pdfRegion = region as PdfRegion;
            if (pdfRegion == null)
            {
                return;
            }
            foreach (var regionView in RegionViews.ToList<PDFRegionView>())
            {
                if ((regionView.DataContext as PdfRegionViewModel).Model.Id == pdfRegion.Id)
                    RegionViews.Remove(regionView);
            }
            RaisePropertyChanged("RegionViews");
        }

        private void LibraryElementControllerOnRegionAdded(object source, RegionController regionController)
        {
            var pdfRegion = regionController?.Model as PdfRegion;
            var pdfRegionController = regionController as PdfRegionController;
            if (pdfRegion == null)
            {
                return;
            }
            var vm = new PdfRegionViewModel(pdfRegion, Controller.LibraryElementController, pdfRegionController, this);
            vm.Editable = false;
            var view = new PDFRegionView(vm);
            //pdfRegionController.PageLocationChanged += PdfRegionControllerOnPageLocationChanged;

            if (pdfRegion.PageLocation != CurrentPageNumber)
            {
                view.Visibility = Visibility.Collapsed;
            }
            RegionViews.Add(view);
            RaisePropertyChanged("RegionViews");
        }
        
        public void CreatePdfRegionViews()
        {
            var elementController = Controller.LibraryElementController;


            if (Controller.LibraryElementModel.Regions == null)
            {
                return;
            }
            RegionViews.Clear();
            foreach (var regionModel in Controller.LibraryElementModel.Regions)
            {

                var pdfRegion = regionModel as PdfRegion;
                PdfRegionController regionController;
                if (SessionController.Instance.RegionsController.GetRegionController(pdfRegion.Id) == null)
                {
                    //Debug.Fail("Did not load");
                    regionController = SessionController.Instance.RegionsController.AddRegion(pdfRegion, Controller.LibraryElementModel.LibraryElementId) as PdfRegionController;
                }
                else {
                    regionController = SessionController.Instance.RegionsController.GetRegionController(pdfRegion.Id) as PdfRegionController;
                }


                var vm = new PdfRegionViewModel(pdfRegion, elementController, regionController, this);
                vm.Editable = false;
                var view = new PDFRegionView(vm);
                if (pdfRegion.PageLocation != CurrentPageNumber)
                {
                    view.Visibility = Visibility.Collapsed;
                }
                RegionViews.Add(view);

            }
            RaisePropertyChanged("RegionViews");


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
            Controller.LibraryElementController.Loaded -= LibraryElementModelOnOnLoaded;
            await DisplayPdf();
            this.CreatePdfRegionViews();
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
            //await UITask.Run(async delegate { await RenderPage(page); });
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
            Width = width;
            Height = height;
            Buffer = buf;
            //await s.ReadAsync(Bytes, 0, (int) s.Length);

           // var s= buf.AsStream();

            //RandomAccessStream.
           // await s.CopyToAsync(image.PixelBuffer.AsStream());
           // image.Invalidate();

      //      Image x = (Bitmap)((new ImageConverter()).ConvertFrom(jpegByteArray))
            /*
            InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();

            using (Stream stream = image.PixelBuffer)
            {
                await stream.CopyToAsync(ras.AsStreamForWrite());
            }
            */
            //ImageSource = image;

            RaisePropertyChanged("ImageSource");


        }

        public IBuffer Buffer { get; set; }


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
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

            base.SetSize(width, height);

            //SetSize(width, height);
        }

        public override double GetRatio()
        {
            return ImageSource == null ? 1 : (double)ImageSource.PixelHeight / (double)ImageSource.PixelWidth;
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
            return View?.GetWidth() ?? 0;
        }

        public double GetHeight()
        {
            return View?.GetHeight() ?? 0 ;
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

        public double GetViewWidth()
        {
            throw new NotImplementedException();
        }

        public double GetViewHeight()
        {
            throw new NotImplementedException();
        }
    }
}
