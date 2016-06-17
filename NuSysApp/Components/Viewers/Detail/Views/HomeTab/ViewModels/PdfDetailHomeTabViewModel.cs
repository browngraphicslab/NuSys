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
using NuSysApp.Components.Viewers.FreeForm;
using System.Net;
using Newtonsoft.Json;
using LdaLibrary;

namespace NuSysApp
{
    public class PdfDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public WriteableBitmap ImageSource { get; set; }
        private int _pageNumber = 0;
        private MuPDFWinRT.Document _document;
        public PdfDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
        }
        public override async Task Init()
        {
            _document = await MediaUtil.DataToPDF(Controller.LibraryElementModel.Data);
            await Goto(_pageNumber);
        }
        private async Task Goto(int pageNumber)
        {
            if (_document == null)
                return;
            if (pageNumber == -1) return;
            if (pageNumber >= (_document.PageCount)) return;
            _pageNumber = pageNumber;
            await RenderPage(_pageNumber);
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
        public async Task FlipLeft()
        {
            await Goto(_pageNumber - 1);
        }
        public async Task FlipRight()
        {
            await Goto(_pageNumber + 1);
            await LaunchLDA();
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
    }
}
