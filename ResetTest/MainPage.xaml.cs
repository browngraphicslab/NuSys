using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ResetTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                var n = new Networker();
                await Task.Run( async () =>
                {
                    var result = await n.foo("id");
                    Debug.WriteLine("got result:");
                    Debug.WriteLine(result);
                });
            };
            Debug.WriteLine("Main done.");
        }
    }

    public class Networker
    {
        private Dictionary<string, ManualResetEvent> _resets = new Dictionary<string, ManualResetEvent>();

        private Timer _timer;

        private string _id;

        public async Task<string> foo(string id)
        {
            _id = id;
            var result = string.Empty;

            // send request
            Debug.WriteLine("request sent.");
            var reset = new ManualResetEvent(false);
            _resets.Add(id, reset);

            // receive request
            _timer = new Timer(SendMessage, null, 4000, 4000 );
            reset.WaitOne();
            return "NewNode";
        }

        
        private async void SendMessage(object state)
        {
            Debug.WriteLine("request received.");
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _resets[_id].Set();

        }
    }
}
