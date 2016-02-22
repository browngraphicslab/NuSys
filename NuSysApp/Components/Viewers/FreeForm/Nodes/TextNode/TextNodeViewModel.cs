using System.Collections.Generic;
using System.IO;
using System;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class TextNodeViewModel : ElementInstanceViewModel
    {
        #region Private Members
        private string _rtf = string.Empty;
        private List<BitmapImage> _inlineImages = new List<BitmapImage>();
        private List<byte[]> _imgData = new List<byte[]>();

        #endregion Private Members
        public TextNodeViewModel(ElementInstanceController controller) : base(controller)
        {           
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 164, 220, 255));
            //((TextNodeModel) Model).TextChanged += TextChangedHandler;
        }
        
        public override async Task Init()       
        {

        }

        private static async Task<BitmapImage> ByteArrayToBitmapImage(byte[] byteArray)
        {
            if (byteArray != null)
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(byteArray.AsBuffer());
                    var image = new BitmapImage();
                    stream.Seek(0);
                    image.SetSource(stream);
                    return image;
                }
            }
            return null;
        }

        private async Task<byte[]> DownloadImageFromWebsiteAsync(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                using (WebResponse response = await request.GetResponseAsync())
                using (var result = new MemoryStream())
                {
                    Stream imageStream = response.GetResponseStream();
                    await imageStream.CopyToAsync(result);
                    return result.ToArray();
                }
            }
            catch (WebException ex)
            {
                return null;
            }
        }

        public List<BitmapImage> InlineImages
        {
            get
            {
                return _inlineImages;
            }
        }

        //private async void TextChangedHandler(object source, TextChangedEventArgs e)
        //{
        //   // this.MarkDownText = ((TextNode)this.Model).Text;
        //    await Init();
        //}


        #region Public Properties

        private string _data = string.Empty;
        

        public string RtfText
        {
            get
            {
                return _rtf;
            }

            set
            {
                _rtf = value;
                RaisePropertyChanged("RtfText");
            }

        }

        #endregion Public Properties
    }
}