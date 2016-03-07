using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MyToolkit.Utilities;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NuSysApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WaitingRoomView : Page
    {
        public FreeFormViewer _freeFormViewer;

        public static string InitialWorkspaceId { get; private set; }
        public static bool IsLocal { get; set; }
        public static string ServerName { get; private set; }
        public static string UserName { get; private set; }
        public static string Password { get; private set; }


        private static IEnumerable<string> _firstLoadList;
        public WaitingRoomView()
        {
            this.InitializeComponent();
            //waitingroomanimation.Begin();

            
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;

            //ServerName = "localhost:54764";
            ServerName = "nusysrepo.azurewebsites.net";
            ServerNameText.Text = ServerName;
            ServerNameText.TextChanged += delegate
            {
                ServerName = ServerNameText.Text;
                Init();
            };

            ellipse.Begin();
            Init();
        }

        private async void Init()
        {
            List?.Items?.Clear();
            try
            {
                var url = "http://" + ServerName + "/api/getworkspace";
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(new Uri(url));
                string data;
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
                var list = JsonConvert.DeserializeObject<List<string>>(data);
                foreach (var s in list)
                {
                    var box = new TextBlock();
                    box.Text = s;
                    List.Items.Add(box);
                }
                if (SessionController.Instance.NuSysNetworkSession.LocalIP == null)
                {
                    await SessionController.Instance.NuSysNetworkSession.Init();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("not a valid server");
                // TODO: fix this
            }
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
        private async void Join_Workspace_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItems.Count == 1)
            {
                var item = List.SelectedItems.First();
                var id = ((TextBlock) item).Text;
                var url = "http://" + ServerName + "/api/getworkspace/"+id;
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(new Uri(url));
                string data;
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
                _firstLoadList = JsonConvert.DeserializeObject<List<string>>(data);
                InitialWorkspaceId = id;
                this.Frame.Navigate(typeof(SessionView));
            }
        }

        public static IEnumerable<string> GetFirstLoadList()
        {
            if (_firstLoadList == null)
            {
                return new List<string>();
            }
            var l = new List<string>(_firstLoadList);
            _firstLoadList = null;
            return l;
        }

        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var cred = new Dictionary<string, string>();
                /*
                cred["user"] = usernameInput.Text;
                cred["pass"] = passwordInput.Text;
                */
                var url = "http://" + ServerName + "/api/login/" ;
                var client = new HttpClient(
                 new HttpClientHandler
                 {
                     Credentials = new NetworkCredential("unusys","pnusys"),
                     ClientCertificateOptions = ClientCertificateOption.Automatic
                 });

                var response = await client.PutAsync(new Uri(url),new StringContent(JsonConvert.SerializeObject(cred), Encoding.UTF8, "application/json"));
                //var response = await client.GetAsync(new Uri(url));
                string data;
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException h)
            {
                Debug.WriteLine("cannot connect to server");
            }
            
        }
        //TODO: move this crypto stuff elsewhere, only here temporarily
        public static byte[] Encrypt(string plain)
        {
            return Encrypt(GetBytes(plain));
        }

        public static byte[] Encrypt(byte[] bytes)
        {
            var sha = SHA256.Create();
            return sha.ComputeHash(bytes);
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
