using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class BucketWindowView : UserControl
    {
        private ContentImporter _contentImporter = new ContentImporter();

        private FloatingMenuView _floatingMenu;


        public ObservableCollection<BucketItem> BucketItems { get; set; }

        public BucketWindowView()
        {
            this.InitializeComponent();

            BucketItems = new ObservableCollection<BucketItem>();
            DataContext = this;

            Border.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 98, 189, 197));

            _contentImporter.ContentImported += async delegate (List<string> contents)
            {
                if (contents.Count == 0)
                    return;

                var nodes = new List<NodeModel>();
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                var createdNodes = new List<string>();
                var cdEvent = new System.Threading.CountdownEvent(contents.Count);
                
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    foreach (var content in contents)
                    {                        
                        var rtf = await ContentConverter.HtmlToRtfWithImages(await ContentConverter.MdToHtml(content));
                        BucketItems.Add(new BucketItem(content, rtf));
                    }

                });

                cdEvent.Wait();  
                
            };
        }

        private void ContentContainer_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.SetText(((BucketItem)e.Items[0]).MarkDown);
            e.Cancel = false;
        }

        private void ContentContainer_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            Debug.WriteLine("drag starting");
        }

        private void ContentContainer_DragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine("drag enter");
        }

        private void ContentContainer_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            BucketItems.Remove((BucketItem)args.Items[0]);            
            Debug.WriteLine("drag items completed!");
        }

        public void getFloatingMenu(FloatingMenuView floatingMenu)
        {
            _floatingMenu = floatingMenu;
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            _floatingMenu.CloseAllSubMenus();
        }
    }



    public class BucketItem
        {
            public BucketItem(string markDown, string rtf)
            {
                Rtf = rtf;
                MarkDown = markDown;
            }

            public string MarkDown
            {
                get; set;
            }

            public string Rtf
            {
                get; set;
            }
        }
    }