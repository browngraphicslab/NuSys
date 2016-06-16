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

namespace NuSysApp
{
    public class PdfDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public WriteableBitmap ImageSource { get; set; }
        private int _pageNumber = 0;
        private Document _document;
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
        }
    }
}
