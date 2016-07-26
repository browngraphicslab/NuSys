using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class TextElementRenderItem : BaseRenderItem
    {
        private TextNodeViewModel _vm;
        private IRandomAccessStream _stream;
        private CanvasBitmap _bmp;
        private ICanvasResourceCreator _ds;

        public TextElementRenderItem(TextNodeViewModel vm, ICanvasResourceCreator ds)
        {
            _vm = vm;
            _ds = ds;


            UITask.Run(async () =>
            {
                var wv = SessionController.Instance.SessionView.WV;
                wv.Width = _vm.Width;
                wv.Height = _vm.Height;
                wv.NavigationCompleted += TextNodeWebViewOnNavigationCompleted;
                wv.ScriptNotify += wvBrowser_ScriptNotify;
                wv.Navigate(new Uri("ms-appx-web:///Components/TextEditor/textview.html"));
            });
        }

        private async void wvBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var wv = SessionController.Instance.SessionView.WV;
            wv.ScriptNotify -= wvBrowser_ScriptNotify;
            InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
            await wv.CapturePreviewToStreamAsync(ms);

            // Create a small thumbnail.
            int longlength = 180, width = 0, height = 0;
            double srcwidth = wv.ActualWidth, srcheight = wv.ActualHeight;
            double factor = srcwidth / srcheight;
            if (factor < 1)
            {
                height = longlength;
                width = (int)(longlength * factor);
            }
            else
            {
                width = longlength;
                height = (int)(longlength / factor);
            }

            _stream = ms.CloneStream();


            _bmp = await CanvasBitmap.LoadAsync(_ds, _stream);


        }

        private async void TextNodeWebViewOnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            var wv = SessionController.Instance.SessionView.WV;
            wv.NavigationCompleted -= TextNodeWebViewOnNavigationCompleted;
            var vm = _vm as TextNodeViewModel;

            var str = vm?.Text;
            if (str != null)
            {
                String[] myString = { str };
                IEnumerable<String> s = myString;
                await wv.InvokeScriptAsync("InsertText", s);
            }
        }

        public async override void Draw(CanvasDrawingSession ds)
        {
            ds.DrawText(_vm.Title, new Vector2((float)_vm.X, (float)(_vm.Y-30)), Colors.Black);
            ds.FillRectangle( new Rect {X=_vm.X, Y= _vm.Y, Width = _vm.Width, Height=_vm.Height}, Colors.Black);

            if (_bmp != null)
                ds.DrawImage(_bmp, new Rect { X = _vm.X, Y = _vm.Y, Width = _vm.Width, Height = _vm.Height });
        }
    }

    sealed class WebviewRenderer
    {
        private static volatile WebviewRenderer instance;
        private static object syncRoot = new Object();

        private WebviewRenderer() { }

        private BlockingQueue<TextElementRenderItem> _queue = new BlockingQueue<TextElementRenderItem>(100); 

        public static WebviewRenderer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new WebviewRenderer();
                    }
                }

                return instance;
            }
        }
    }
}
