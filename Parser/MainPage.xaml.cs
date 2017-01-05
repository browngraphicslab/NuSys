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
            // We then take the results of the document and go through each node and parse different information that exists 
            // in the html
            await dc.loadResults();
            var docs = new List<CognitiveApiDocument>();

            

            foreach(var dh in dc.DataObjects)
            {
                if (dh is TextDataHolder)
                {
                    docs.Add(new CognitiveApiDocument(""+(dh as TextDataHolder).Text.GetHashCode(), (dh as TextDataHolder).Text));
                    docs.Add(new CognitiveApiDocument(""+(dh as TextDataHolder).Text.GetHashCode()+1, (dh as TextDataHolder).Text));
                    docs.Add(new CognitiveApiDocument(""+(dh as TextDataHolder).Text.GetHashCode()+2, (dh as TextDataHolder).Text));
                    docs.Add(new CognitiveApiDocument(""+(dh as TextDataHolder).Text.GetHashCode()+3, (dh as TextDataHolder).Text));
                }


                var stack = new StackPanel();
                //stack.Width = 500;
                
                var cap = new TextBlock {Text = dh.Title??"", FontSize=20};
                switch (dh.DataType)
                {
                    case DataType.Text:
                        var tb = new TextBlock {Text = (dh as TextDataHolder).Text,TextWrapping=TextWrapping.WrapWholeWords};
                        var links = new TextBlock();
                        foreach(var link in (dh as TextDataHolder).links)
                        {
                            links.Text += link + "\n";
                        }
                        stack.Children.Add(tb);
                        stack.Children.Add(links);
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
                stack.DataContext = dh;
                xItems.Items.Add(stack);
            }

            //////////
            // END OF RENDERING, START OF TAGGING 
            //////////
            var d = await TextProcessor.GetTextTopicsAsync(docs);
            
            foreach(var ta in d.operationProcessingResult.topicAssignments)
            {
                foreach(var stack in xItems.Items.Where(f=>(f as StackPanel)?.DataContext is TextDataHolder && ""+((f as StackPanel).DataContext as TextDataHolder).Text.GetHashCode() == ta.documentId))
                {
                    (stack as StackPanel).Children.Add(new TextBox() { Text = d.operationProcessingResult.topics.First(f => f.id == ta.topicId).keyPhrase });
                }
            }
            //////////
            // END OF TAGGING, START OF DICTIONARY MAKING
            //////////
            var DocumentIdToTopic = new Dictionary<string, HashSet<string>>();

            foreach (var ta in d.operationProcessingResult.topicAssignments)
            {
                if (!DocumentIdToTopic.Keys.Contains(ta.documentId))
                {
                    DocumentIdToTopic.Add(ta.documentId, new HashSet<string>());
                }
                    DocumentIdToTopic[ta.documentId].Add(d.operationProcessingResult.topics.First(f => f.id == ta.topicId).keyPhrase);
            }
            //////////
            // THIS WILL BE USED WHEN EVERYTHING IS SERVER SIDE IN NUSYS AND THEN WE CAN JUST POINT EACH TAG TO AN EXISTING ID
            //////////
        }
    }
}
