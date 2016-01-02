using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NuSysApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WaitingRoomView : Page
    {
        public WorkspaceView _workspaceView;
        public WaitingRoomView()
        {
            this.InitializeComponent();
            //waitingroomanimation.Begin();

            
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
           

            ellipse.Begin();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (SessionView));
        }
        private void Local_OnClick(object sender, RoutedEventArgs e)
        {
            IsLocal = true;
            this.Frame.Navigate(typeof(SessionView));
        }
        private void clear_OnClick(object sender, RoutedEventArgs e)
        {
            const string URL = "http://aint.ch/nusys/clients.php";
            var urlParameters = "?action=clear";
            var client = new HttpClient { BaseAddress = new Uri(URL) };
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(urlParameters).Result;
        }
        public static bool IsLocal { get; set; }
    }
}
