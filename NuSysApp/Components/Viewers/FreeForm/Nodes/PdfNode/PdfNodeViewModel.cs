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
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using NusysIntermediate;
using Image = SharpDX.Direct2D1.Image;

namespace NuSysApp
{
    public class PdfNodeViewModel : ElementViewModel
    {
        private CompositeTransform _inkScale;
        private bool _isSelected;
        public int CurrentPageNumber { get;  private set; }
        public ObservableCollection<Button> SuggestedTags { get; set; }
        private List<string> _suggestedTags = new List<string>();

        public Size ImageSize { get; set; }= new Size(1, 1);


        public PdfNodeViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            var model = (PdfNodeModel) controller.Model;
            CurrentPageNumber = model.CurrentPageNumber;
        }           

        public async override Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
            await DisplayPdf();
        }

        private async Task DisplayPdf()
        {
            if (Controller.LibraryElementModel == null || Controller.LibraryElementController.Data == null) {
                return;
            }
            await Goto(CurrentPageNumber);
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
            var content = Controller.LibraryElementController.ContentDataController.ContentDataModel as PdfContentDataModel;
            if (content == null || pageNumber == -1)
            {
                return;
            }

            if (pageNumber >= (content.PageCount))
            {
                return;
            }
        
            CurrentPageNumber = pageNumber;
            ((PdfNodeModel)Model).CurrentPageNumber = CurrentPageNumber;
            RaisePropertyChanged("ImageSource");
        }

        public override void SetSize(double width, double height)
        {
            if (ImageSize.Width > ImageSize.Height)
            {
                var r = ImageSize.Height / (double)ImageSize.Width;
                base.SetSize(width, width * r);
            }
            else
            {
                var r = ImageSize.Width / (double)ImageSize.Height;
                base.SetSize(height * r, height);
            }
        }

        protected override void OnSizeChanged(object source, double width, double height)
        {
            SetSize(width, height);       
        }
    }
}
