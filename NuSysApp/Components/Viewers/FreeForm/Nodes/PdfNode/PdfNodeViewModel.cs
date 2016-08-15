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
using NusysIntermediate;

namespace NuSysApp
{
    public class PdfNodeViewModel : ElementViewModel
    {
        private CompositeTransform _inkScale;
        private bool _isSelected;
        public int CurrentPageNumber { get;  private set; }
        public ObservableCollection<Button> SuggestedTags { get; set; }
        private List<string> _suggestedTags = new List<string>();

        /// <summary>
        /// the image source for the current pdf page. 
        /// </summary>
        public BitmapImage ImageSource
        {
            get; set;
        }

        public PdfNodeViewModel(ElementController controller) : base(controller)
        {
            ImageSource = new BitmapImage();
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            var model = (PdfNodeModel) controller.Model;
            CurrentPageNumber = model.CurrentPageNumber;
            if (controller.LibraryElementModel.Type == NusysConstants.ElementType.PdfRegion)
            {
                var pdfRegionModel = controller.LibraryElementModel as PdfRegionModel;
                CurrentPageNumber = pdfRegionModel.PageLocation;
            }
        }           

        public override void Dispose()
        {
            var model = (PdfNodeModel)Controller.Model;
            ImageSource.ImageOpened -= ImageSourceOnImageOpened;
            base.Dispose();
        }

        public async override Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
            ImageSource.ImageOpened += ImageSourceOnImageOpened;
            await DisplayPdf();
        }

        private void ImageSourceOnImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            SetSize(ImageSource.PixelWidth, ImageSource.PixelHeight);
        }

        private async void LibraryElementModelOnOnLoaded(object sender)
        {
            this.DisplayPlaceholderThumbnail();
        }


        private async Task DisplayPdf()
        {
            if (Controller.LibraryElementModel == null || Controller.LibraryElementController.Data == null) {
                return;
            }
            await Goto(CurrentPageNumber);
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
                BitmapImage bitmap = new BitmapImage(Controller.LibraryElementController.LargeIconUri);
                ImageSource = bitmap;
                RaisePropertyChanged("ImageSource");
            }
            
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
            var content = Controller.LibraryElementController.ContentDataModel as PdfContentDataModel;
            if (content == null || pageNumber == -1)
            {
                return;
            }

            if (pageNumber >= (content.PageUrls.Count))
            {
                return;
            }
            ImageSource.UriSource = new Uri(content.PageUrls[pageNumber]);
            CurrentPageNumber = pageNumber;
            ((PdfNodeModel)Model).CurrentPageNumber = CurrentPageNumber;
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
    }
}
