using MarkdownSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class ContentConverter
    {
        private static WebView wv = new WebView();

        public async static Task<string> HtmlToRtf(string h)
        {
            var html = string.Empty;
            if (!h.StartsWith("<html>")) {
                html = "<html><body>" + h + "</body></html>";
            } else
            {
                html = h;
            }

            html =  html.Replace("\n", "");

            string result = null;
            var manualReset = new ManualResetEvent(false);

      
            wv = new WebView();
        //    wv.Width = 300;
        //    wv.Height = 300;
        //    WorkspaceView.Instance.MainCanvas.Children.Add(wv);
              
            wv.DOMContentLoaded += async delegate
            {
                await wv.InvokeScriptAsync("eval", new string[] { "(function(){ var r = document.createRange(); r.selectNodeContents(document.body); var s = window.getSelection(); s.removeAllRanges(); s.addRange(r);  })();" });
                DataPackage p = await wv.CaptureSelectedContentToDataPackageAsync();
                result = await p.GetView().GetRtfAsync();
                manualReset.Set();
            };
            
            SynchronizationContext context = SynchronizationContext.Current;
            await Task.Factory.StartNew(() =>
            {
                context.Post((a) => {
                    wv.NavigateToString(html);
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

    }
}
