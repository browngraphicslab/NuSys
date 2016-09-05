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


        private BitmapImage _imageSource = new BitmapImage();
        /// <summary>
        /// the image source for the current pdf page. 
        /// </summary>
        public BitmapImage ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                RaisePropertyChanged("ImageSource");
            }
        }

        private int _pageNumber;

        public int CurrentPageNumber => _pageNumber;

        public static int InitialPageNumber;
        
        public PdfDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            Editable = true;
            _pageNumber = InitialPageNumber;
        }

        public override async Task Init()
        {
            await Goto(_pageNumber);
        }

        public async Task Goto(int pageNumber)
        {
            var content = LibraryElementController.ContentDataController.ContentDataModel as PdfContentDataModel;
            if (content == null || pageNumber == -1)
            {
                return;
            }

            if (pageNumber >= content.PageCount)
            {
                return;
            }
            _pageNumber = pageNumber;
            ImageSource.UriSource = new Uri(content.PageUrls[pageNumber]);
            RaisePropertyChanged("ImageSource");
            PageLocationChanged?.Invoke(this, CurrentPageNumber);
        }

        public async Task FlipLeft()
        {
            await Goto(_pageNumber - 1);
        }
        public async Task FlipRight()
        {
            await Goto(_pageNumber + 1);
        }
       

        public override CreateNewLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
        {
            var imageLibraryElement = (LibraryElementController as ImageLibraryElementController)?.ImageLibraryElementModel;

            Debug.Assert(imageLibraryElement != null);

            var args = new CreateNewPdfLibraryElementModelRequestArgs();
            args.PdfPageEnd = _pageNumber;
            args.PdfPageStart = _pageNumber;
            args.NormalizedX = .25 * imageLibraryElement.NormalizedX;
            args.NormalizedY = .25 * imageLibraryElement.NormalizedY;
            args.NormalizedHeight = .5 * imageLibraryElement.NormalizedHeight;
            args.NormalizedWidth = .5 * imageLibraryElement.NormalizedWidth;

            return args;
        }

        /// <summary>
        /// Call this when you need to update region visiblity to reflect the current page
        /// </summary>
        public void InvokePageLocationChanged()
        {
            PageLocationChanged?.Invoke(this, CurrentPageNumber);
        }
    }
}
