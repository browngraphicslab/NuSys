﻿using MarkdownSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ContentConverter
    {
        private static bool dirty = false;
        private static string result;
        private static WebView _wv = new WebView();
        private static ManualResetEvent manualReset = new ManualResetEvent(false);
        private static TypedEventHandler<WebView, WebViewNavigationCompletedEventArgs> NavigationCompleted = async delegate
        {
            await _wv.InvokeScriptAsync("eval", new string[] { "(function(){ var r = document.createRange(); r.selectNodeContents(document.body); var s = window.getSelection(); s.removeAllRanges(); s.addRange(r);  })();" });
            DataPackage p = await _wv.CaptureSelectedContentToDataPackageAsync();
            result = await p.GetView().GetRtfAsync();
            manualReset.Set();
        };

        public async static Task<string> HtmlToRtfWithImages(string htmlString)
        {
            var md = await HtmlToMd(htmlString);
            var inlineImages = new List<BitmapImage>();
            var imgData = new List<byte[]>();
            const string rtfImagePlaceholder = "---IMAGE---";

            while (true)
            {
                //Debug.WriteLine(md);
                var match = Regex.Match(md, @"!\[.*\]\((.*)\)", RegexOptions.IgnoreCase);
                Regex rgx = new Regex(@"!\[.*\]\(.*\)");
                md = rgx.Replace(md, rtfImagePlaceholder, 1);

                if (match.Groups.Count <= 1)
                {
                    break;
                }

                string imgUrl = match.Groups[1].Value;
                var img = await DownloadImageFromWebsiteAsync(imgUrl);

                imgData.Add(img);
                inlineImages.Add(await ByteArrayToBitmapImage(img));
            }

            var rtf = await MdToRtf(md);
            rtf = rtf.Replace(@"\fonttbl{\f0\froman\fcharset0 Times New Roman;", @"\fonttbl{\f0\froman\fcharset0 Calibri;");

            for (var i = 0; i < inlineImages.Count; i++)
            {
                var imageRtf = @"{\pict\pngblip\picw---IMG_W---0\pich---IMG_H---\picwgoal---IMG_GOAL_W---\pichgoal---IMG_GOAL_H---\hex ---IMG_DATA---}";
                imageRtf = imageRtf.Replace("---IMG_W---", inlineImages[i].PixelWidth.ToString());
                imageRtf = imageRtf.Replace("---IMG_H---", inlineImages[i].PixelHeight.ToString());
                imageRtf = imageRtf.Replace("---IMG_GOAL_W---", (inlineImages[i].PixelWidth * 15).ToString());
                imageRtf = imageRtf.Replace("---IMG_GOAL_H---", (inlineImages[i].PixelHeight * 15).ToString());

                var imgDataHex = BitConverter.ToString(imgData[i]).Replace("-", "");
                imageRtf = imageRtf.Replace("---IMG_DATA---", imgDataHex);

                var regex = new Regex(Regex.Escape(rtfImagePlaceholder));
                rtf = regex.Replace(rtf, imageRtf, 1);
            }

            return rtf;
        }

        public async static Task<string> HtmlToRtf(string htmlString )
        {

            if (htmlString == "")
            {
                htmlString = "<span>&nbsp;</span>";
            }
            var html = string.Empty;

            if (!htmlString.StartsWith("<html>")) {
                html = "<html><body>" + htmlString + "</body></html>";
            } else
            {
                html = htmlString;
            }

            html =  html.Replace("\n", "");
            result = null;

            _wv.NavigationCompleted -= NavigationCompleted;            
            _wv.NavigationCompleted += NavigationCompleted;       
            manualReset = new ManualResetEvent(false);     
         

            SynchronizationContext context = SynchronizationContext.Current;
            await Task.Run(() =>
            {
                context.Post((a) => {
                    _wv.NavigateToString(html);
                }, null);
                
                manualReset.WaitOne();
            });
  

            return result;
        }
            
        public static async Task<string> HtmlToMd(string html)
        {
            var rmd = new ReverseMarkdown.Converter();
            return rmd.Convert(html);
        }

        public static async Task<string> MdToHtml(string md)
        {
            var r = new MarkdownDeep.Markdown
            {
                ExtraMode = true
            };

            return r.Transform(md);
        }

        public static async Task<string> MdToRtf(string md)
        {
            var html = await MdToHtml(md);
            return await HtmlToRtf(html);
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

        private static async Task<byte[]> DownloadImageFromWebsiteAsync(string url)
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(url);
                using (var response = await request.GetResponseAsync())
                using (var result = new MemoryStream())
                {
                    var imageStream = response.GetResponseStream();
                    await imageStream.CopyToAsync(result);
                    return result.ToArray();
                }
            }
            catch (WebException ex)
            {
                Debug.WriteLine("WEB EXCEPTION");
                return null;
            }
        }

    }
}
