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
    public class TextNodeViewModel : NodeViewModel
    {
        #region Private Members
        private string _rtf = string.Empty;
        private List<BitmapImage> _inlineImages = new List<BitmapImage>();
        private List<byte[]> _imgData = new List<byte[]>();

        #endregion Private Members
        public TextNodeViewModel(TextNodeModel model) : base(model)
        {           
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize;
            this.Height = Constants.DefaultNodeSize;
            this.NodeType = NodeType.Text;
            //this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 152, 192, 113));
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 89, 189, 197));


            ((TextNodeModel) this.Model).OnTextChanged += TextChangedHandler;
        }

        public async Task UpdateRtf()       
        {
           
            try { 
            _inlineImages.Clear();

            const string rtfImagePlaceholder = "---IMAGE---";
            var md = ((TextNodeModel)Model).Text;

            var imgData = new List<byte[]>();
            while (true)
            {

                var match = Regex.Match(md, @"\[!\[.*\]\((.*)\)\]", RegexOptions.IgnoreCase);
                Regex rgx = new Regex(@"\[!\[.*\]\(.*\)\]\(.*\)");
                md = rgx.Replace(md, rtfImagePlaceholder, 1);

                if (match.Groups.Count <= 1)
                {
                    break;
                }

                string imgUrl = "http:" + match.Groups[1].Value;
                var img = await DownloadImageFromWebsiteAsync(imgUrl);

                imgData.Add(img);
                _inlineImages.Add(await ByteArrayToBitmapImage(img));
            }

            var rtf = await ContentConverter.MdToRtf(md);
            rtf = rtf.Replace(@"\fonttbl{\f0\froman\fcharset0 Times New Roman;", @"\fonttbl{\f0\froman\fcharset0 Calibri;");

            for (var i = 0; i < _inlineImages.Count; i++)
            {
                var imageRtf = @"{\pict\pngblip\picw---IMG_W---0\pich---IMG_H---\picwgoal---IMG_GOAL_W---\pichgoal---IMG_GOAL_H---\hex ---IMG_DATA---}";
                imageRtf = imageRtf.Replace("---IMG_W---", _inlineImages[i].PixelWidth.ToString());
                imageRtf = imageRtf.Replace("---IMG_H---", _inlineImages[i].PixelHeight.ToString());
                imageRtf = imageRtf.Replace("---IMG_GOAL_W---", (_inlineImages[i].PixelWidth * 15).ToString());
                imageRtf = imageRtf.Replace("---IMG_GOAL_H---", (_inlineImages[i].PixelHeight * 15).ToString());

                var imgDataHex = BitConverter.ToString(imgData[i]).Replace("-", "");
                imageRtf = imageRtf.Replace("---IMG_DATA---", imgDataHex);

                var regex = new Regex(Regex.Escape(rtfImagePlaceholder));
                rtf = regex.Replace(rtf, imageRtf, 1);
            }

            RtfText = rtf;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Couldn't update rtf.");
            }
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

        private async void TextChangedHandler(object source, TextChangedEventArgs e)
        {
           // this.MarkDownText = ((TextNode)this.Model).Text;
            await UpdateRtf();
        }


        #region Public Properties

        private string _data = string.Empty;
        /// <summary>
        /// data contained by text node
        /// </summary>
      

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