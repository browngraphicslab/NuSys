using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
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
using Windows.UI.Xaml;
using System.Net;
using Newtonsoft.Json;
using LdaLibrary;
using WinRTXamlToolkit.Imaging;
using Image = SharpDX.Direct2D1.Image;
using Point = Windows.Foundation.Point;

namespace NuSysApp
{
    public class PdfDetailHomeTabViewModel : DetailHomeTabViewModel, Sizeable
    {
        public LibraryElementController Controller { get; }
        public ObservableCollection<PDFRegionView> RegionViews { set; get; }
        public WriteableBitmap ImageSource { get; set; }
        
        private int _pageNumber = 0;
        private MuPDFWinRT.Document _document;

        public static int InitialPageNumber;
        
        public PdfDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            RegionViews = new ObservableCollection<PDFRegionView>();
            Editable = true;

            _pageNumber = InitialPageNumber;
        }

        public override async Task Init()
        {
            await Task.Run(async delegate {
                _document = await MediaUtil.DataToPDF(Controller.LibraryElementModel.Data);
            });

            await Goto(_pageNumber);
        }

        public async Task Goto(int pageNumber, Region region = null)
        {
            if (_document == null)
                return;
            if (pageNumber == -1) return;
            if (pageNumber >= (_document.PageCount))
            {
                return;
            }
            _pageNumber = pageNumber;
            await RenderPage(_pageNumber, region);


        }
        private async Task RenderPage(int pageNumber, Region region = null)
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



            foreach (var regionView in RegionViews)
            {
                var model = (regionView.DataContext as PdfRegionViewModel)?.Model;
                if ((model as PdfRegionModel).PageLocation != _pageNumber)
                {
                    regionView.Visibility = Visibility.Collapsed;
                    regionView.Deselect();

                }
                else
                {
                    regionView.Visibility = Visibility.Visible;
                    if (region != null)
                    {
                        if (model?.LibraryElementId == region.LibraryElementId)
                        {
                            regionView.Select();
                        }
                        else
                        {
                            regionView.Deselect();
                            //ensures that only thing selected is the pdf you just clicked.
                        }
                    }
                }
            }




        }
        public async Task FlipLeft()
        {
            await Goto(_pageNumber - 1);
            /*
            foreach (var regionView in RegionViews)
            {
                var model = (regionView.DataContext as PdfRegionViewModel)?.Model;
                if ((model as PdfRegionModel).PageLocation != _pageNumber)
                {
                    regionView.Visibility = Visibility.Collapsed;
                }

                else
                {
                    regionView.Visibility = Visibility.Visible;
                }
            }
            */
        }
        public async Task FlipRight()
        {
            await Goto(_pageNumber + 1);
            //await LaunchLDA();
            /*
            foreach (var regionView in RegionViews)
            {
                var model = (regionView.DataContext as PdfRegionViewModel)?.Model;
                if ((model as PdfRegionModel).PageLocation != _pageNumber)
                {
                    regionView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    regionView.Visibility = Visibility.Visible;
                }
            }
            */
        }
        public async Task LaunchLDA()
        {
            Task.Run(async () =>
            {
                var test = new List<string>();

                // parameters for our LDA algorithm
                string filename = Controller.LibraryElementModel.Title;
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
                List<string> topics = await TagExtractor.launch(test, new List<string>() { data });
                await UITask.Run(() =>
                {
                    var topicKeywords = new HashSet<Keyword>();
                    foreach (var topic in topics)
                    {
                        topicKeywords.Add(new Keyword(topic, Keyword.KeywordSource.TopicModeling));
                    }
                    Controller.SetKeywords((topicKeywords));
                    RaisePropertyChanged("Tags");
                });
            });
        }

        public override void AddRegion(object sender, RegionLibraryElementController regionLibraryElementController)
        {
            var pdfRegion = regionLibraryElementController.LibraryElementModel as PdfRegionModel;
            if (pdfRegion == null)
            {
                return;
            }
            var pdfRegionController = regionLibraryElementController as PdfRegionLibraryElementController;
            //pdfRegionController?.SetPageLocation(_pageNumber);
            var vm = new PdfRegionViewModel(pdfRegion, pdfRegionController, this);
            if (!Editable)
                vm.Editable = false;

            var view = new PDFRegionView(vm);
            
            RegionViews.Add(view);


            if (pdfRegion.PageLocation != _pageNumber)
            {
                view.Visibility = Visibility.Collapsed;
            }
            else
            {
                view.Visibility = Visibility.Visible;
            }

            RaisePropertyChanged("RegionViews");
        }


        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            var imageRegion = displayedRegion as PdfRegionModel;
            if (imageRegion == null)
            {
                return;
            }

            foreach (var regionView in RegionViews.ToList<PDFRegionView>())
            {
                if ((regionView.DataContext as PdfRegionViewModel).Model.LibraryElementId == imageRegion.LibraryElementId)
                    RegionViews.Remove(regionView);
            }

            RaisePropertyChanged("RegionViews");
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            var newHeight = this.GetHeight();
            var newWidth = this.GetWidth();

            foreach (var rv in RegionViews)
            {
                var regionViewViewModel = rv.DataContext as RegionViewModel;
                regionViewViewModel?.ChangeSize(sender, newWidth, newHeight);
            }
        }

        public double GetHeight()
        {
            var view = (View as PdfDetailHomeTabView);
            if (view == null)
            {
                return 0;
            }
            //return view.ActualHeight;

            return view.GetPdfHeight();
        }
        public double GetWidth()
        {
            var view = (View as PdfDetailHomeTabView);
            if (view == null)
            {
                return 0;
            }
            //return view.ActualWidth;
            return view.GetPdfWidth();
        }

        public double GetViewWidth()
        {
            var view = (View as PdfDetailHomeTabView);
            if (view == null)
            {
                return 0;
            }
            //return view.ActualWidth;
            return view.ActualWidth;
        }

        public override void SetExistingRegions()
        {
            
            RegionViews.Clear();

            var regionsLibraryElementIds =
                SessionController.Instance.RegionsController.GetClippingParentRegionLibraryElementIds(
                    Controller.LibraryElementModel.LibraryElementId);
            foreach (var regionLibraryElementId in regionsLibraryElementIds)
            {
                var regionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementId) as PdfRegionLibraryElementController;
                Debug.Assert(regionLibraryElementController != null);
                Debug.Assert(regionLibraryElementController.LibraryElementModel is PdfRegionModel);
                var vm = new PdfRegionViewModel(regionLibraryElementController.LibraryElementModel as PdfRegionModel, regionLibraryElementController, this);
                
                var view = new PDFRegionView(vm);
                
                if ((regionLibraryElementController.LibraryElementModel as PdfRegionModel).PageLocation != _pageNumber)
                {
                    view.Visibility = Visibility.Collapsed;
                }
                vm.Editable = Editable;
                RegionViews.Add(view);

            }

            RaisePropertyChanged("RegionViews");
        }

        public override Message GetNewRegionMessage()
        {
            var m = new Message();
            m["rectangle_location"] = new Point(.25, .25);
            m["rectangle_width"] = .5;
            m["rectangle_height"] = .5;
            m["page_location"] = _pageNumber;
            return m;
        }

        public double GetViewHeight()
        {
            var view = (View as PdfDetailHomeTabView);
            if (view == null)
            {
                return 0;
            }
            //return view.ActualWidth;
            return view.ActualHeight;
        }
    }
}
