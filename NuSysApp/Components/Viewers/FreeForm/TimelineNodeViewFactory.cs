using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using MuPDFWinRT;
using NuSysApp.Controller;


namespace NuSysApp
{
    public class TimelineNodeViewFactory
    {
        public MuPDFWinRT.Document _document;
        public async Task<Image> CreateFromSendable(ElementController controller)
        {
            Image view = null;

            var model = controller.Model;

            switch (model.ElementType)
            {
                case ElementType.Text:
                    view = new Image();
                    BitmapImage textimage = new BitmapImage(new Uri("ms-appx:///Assets/icon_text.png", UriKind.Absolute));
                    view.Source = textimage;
                    break;
                case ElementType.Collection:
                    view = new Image();
                    //TODO change icon
                    BitmapImage collectionImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_tag.png", UriKind.Absolute));
                    view.Source = collectionImage;
                    break;
                case ElementType.Tag:
                    view = new Image();
                    BitmapImage tagImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_tag.png", UriKind.Absolute));
                    view.Source = tagImage;
                    break;
                case ElementType.Image:
                    view = new Image();
                    BitmapImage imageImage = new BitmapImage(controller.LibraryElementController.GetSource());
                    view.Source = imageImage;
                    break;
                case ElementType.Word:
                    view = new Image();
                    BitmapImage wordImage = new BitmapImage(new Uri("ms-appx:///Assets/wordIcon.png", UriKind.Absolute));
                    view.Source = wordImage;
                    break;
                case ElementType.Powerpoint:
                    view = new Image();
                    BitmapImage pptImage = new BitmapImage(new Uri("ms-appx:///Assets/powerpointIcon.png", UriKind.Absolute));
                    view.Source = pptImage;
                    break;
                case ElementType.Audio:
                    view = new Image();
                    BitmapImage audioImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_recording.png", UriKind.Absolute));
                    view.Source = audioImage;
                    break;
                case ElementType.PDF:
                    view = new Image();
                    var data = controller.LibraryElementModel.Data;
                    var dataBytes = Convert.FromBase64String(data);
                    var ms = new MemoryStream(dataBytes);
                    using (IInputStream inputStreamAt = ms.AsInputStream())


                    using (var dataReader = new DataReader(inputStreamAt))
                    {
                        uint u = await dataReader.LoadAsync((uint)dataBytes.Length);
                        IBuffer readBuffer = dataReader.ReadBuffer(u);
                        _document = MuPDFWinRT.Document.Create(readBuffer, DocumentType.PDF, 100);
                    }

                    var pageSize = _document.GetPageSize(1);
                    var width = pageSize.X;
                    var height = pageSize.Y;
                    var pdfImage = new WriteableBitmap(width, height);
                    IBuffer buf = new Windows.Storage.Streams.Buffer(pdfImage.PixelBuffer.Capacity);
                    buf.Length = pdfImage.PixelBuffer.Length;

                    _document.DrawPage(1, buf, 0, 0, width, height, false);

                    var s = buf.AsStream();
                    await s.CopyToAsync(pdfImage.PixelBuffer.AsStream());
                    pdfImage.Invalidate();

                    view.Source = pdfImage;
                    break;

                case ElementType.Video:
                    //TODO change icon
                    view = new Image();
                    BitmapImage videoImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_recording.png", UriKind.Absolute));
                    view.Source = videoImage;
                    break;
                case ElementType.Web:
                    view = new Image();
                    BitmapImage webImage = new BitmapImage(new Uri("ms-appx:///Assets/icon_web_color.png", UriKind.Absolute));
                    view.Source = webImage;
                    break;
                //case ElementType.Area:
                //    view = new AreaNodeView(new AreaNodeViewModel((ElementCollectionController)controller));
                //    break;
                //case ElementType.Link:
                //    view = new BezierLinkView(new LinkViewModel((LinkElementController)controller));
                //     break;
            }
            //await ((ElementViewModel)view.DataContext).Init();
            view.Width = 130;
            view.Height = 80;
            return view;
        }  
    }
}
