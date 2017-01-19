using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Navigation;
using ParserHelper;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Parsertests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var times = new  List<DateTime>();
            var searches = new List<string>()
            {
                "hypertext",
                "moulin rouge",
                "alchemy",
                "nightmare",
                "tarot cards",
                "tesla",
                "MOMA",
                "google",
                "card games",
                "art of war",
                "meditations",
                "julius caesar",
                "george washington",
                "18th century france",
                "nobel",
                "paris"
            };
            times.Add(DateTime.Now);
            foreach (var s in searches)
            {
                Debug.WriteLine(@"\\\\\\\\\\\\\NEW SESSION////////////");
                Debug.WriteLine(s);
                await HtmlImporter.RunWithSearch(s);
                times.Add(DateTime.Now);
            }
            for(int i = 0; i < times.Count - 1; i++)
            {
                Debug.WriteLine((times[i+1]-times[i]).TotalMilliseconds);
            }
            Debug.WriteLine((times.Last()-times.First()).TotalMilliseconds/searches.Count);
        }
    }
}
