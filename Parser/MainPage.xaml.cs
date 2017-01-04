using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Parser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = new HTMLParserDataContext(null);
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as HTMLParserDataContext;
            await dc.loadResults();
            foreach(var dh in dc.DataObjects)
            {
                var stack = new StackPanel();
                //stack.Width = 500;
                
                var cap = new TextBlock {Text = dh.Title??"", FontSize=20};
                switch (dh.DataType)
                {
                    case DataType.Text:
                        var tb = new TextBlock {Text = (dh as TextDataHolder).Text,TextWrapping=TextWrapping.WrapWholeWords};
                        stack.Children.Add(tb);
                        break;
                    case DataType.Image:
                        var im = new BitmapImage() { UriSource = (dh as ImageDataHolder).Uri};
                        stack.Children.Add(new Image() { Source = im ,Stretch = Stretch.Uniform,Height=150});
                        break;
                    case DataType.Video:
                        var me = new MediaElement();
                        me.Source = (dh as VideoDataHolder).Uri;
                        me.Play();
                        stack.Children.Add(me);
                        break;
                    case DataType.Audio:
                        var mea = new MediaElement();
                        mea.Source = (dh as AudioDataHolder).Uri;
                        mea.Play();
                        stack.Children.Add(mea);
                        break;
                    case DataType.Pdf:
                        var capu = new TextBlock {Text = (dh as PdfDataHolder).Uri.OriginalString};
                        stack.Children.Add(capu);
                        break;
                }
                stack.Children.Add(cap);
                xItems.Items.Add(stack);
            }
        }
    }
}
