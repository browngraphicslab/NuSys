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
using NusysIntermediate;
using WinRTXamlToolkit.Imaging;
using Image = SharpDX.Direct2D1.Image;
using Point = Windows.Foundation.Point;

namespace NuSysApp
{
    public class PdfDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public delegate void PageLocationChangedEventHandler(object sender, int pageLocation);
        public event PageLocationChangedEventHandler PageLocationChanged;
        public LibraryElementController LibraryElementController { get; }
        public WriteableBitmap ImageSource { get; set; }

        private int _pageNumber;

        public int CurrentPageNumber => _pageNumber;

        private MuPDFWinRT.Document _document;

        public static int InitialPageNumber;
        
        public PdfDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            Editable = true;
            _pageNumber = InitialPageNumber;
        }

        public override async Task Init()
        {
            await Task.Run(async delegate {
                _document = await MediaUtil.DataToPDF(LibraryElementController.Data);
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
            PageLocationChanged?.Invoke(this, pageNumber);
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
        }

        public async Task FlipLeft()
        {
            await Goto(_pageNumber - 1);
        }
        public async Task FlipRight()
        {
            await Goto(_pageNumber + 1);
        }
        public async Task LaunchLDA()
        {
            Task.Run(async () =>
            {
                var test = new List<string>();

                // parameters for our LDA algorithm
                string filename = LibraryElementController.LibraryElementModel.Title;
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
                    LibraryElementController.SetKeywords((topicKeywords));
                    RaisePropertyChanged("Tags");
                });
            });
        }

        public override Message GetNewRegionMessage()
        {
            var m = new Message();
            m["rectangle_location"] = new Point(.25, .25);
            m["rectangle_width"] = .5;
            m["rectangle_height"] = .5;
            m["page_location"] = _pageNumber;


            // if the library element LibraryElementController is a region noramlize top left point, height, and width for original content
            if (LibraryElementController is RectangleRegionLibraryElementController)
            {
                var imageRegionLibraryElementController =
                    LibraryElementController as RectangleRegionLibraryElementController;
                var rectangleRegionModel = imageRegionLibraryElementController?.RectangleRegionModel;

                // normalizes the top left point so that it is in the correct place on the original content
                m["rectangle_location"] = new Point(.25 * rectangleRegionModel.Width + rectangleRegionModel.TopLeftPoint.X,
                                                    .25 * rectangleRegionModel.Height + rectangleRegionModel.TopLeftPoint.Y);
                // same for width and height
                m["rectangle_width"] = .5 * rectangleRegionModel.Width;
                m["rectangle_height"] = .5 * rectangleRegionModel.Height;
            }

            return m;
        }
    }
}
