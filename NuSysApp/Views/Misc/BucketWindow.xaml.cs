
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BucketWindow : UserControl
    {
        private ContentImporter _contentImporter = new ContentImporter();

        public BucketWindow()
        {
            this.InitializeComponent();
            Border.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 81, 220, 231));

            _contentImporter.ContentImported += async delegate (List<string> contents)
            {
                if (contents.Count == 0)
                    return;

                var nodes = new List<Node>();
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                var createdNodes = new List<string>();
                var cdEvent = new System.Threading.CountdownEvent(contents.Count);
                
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    foreach (var content in contents)
                    {                        
                        var rtf = await ContentConverter.HtmlToRtfWithImages(await ContentConverter.MdToHtml(content));
                        ContentContainer.Children.Add(new BucketItem(rtf));
                    }

                });

                cdEvent.Wait();  
                
            };
        }
    }
}