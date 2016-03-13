using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
        public static string ServerName { get; private set; }
        public static string UserName { get; private set; }
        public static string Password { get; private set; }
        public static string ServerSessionID { get; private set; }


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
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            try
            {
                var url = "https://" + ServerName + "/api/getworkspace";
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
                    Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(s,settings);
                    var box = new CollectionTextBox();
                    box.ID = dict.ContainsKey("id") ? (string) dict["id"] : null;//todo do error handinling since this shouldnt be null
                    box.Text = dict.ContainsKey("title") ? (string)dict["title"] : "Unnamed Collection";
                    List.Items.Add(box);
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
        private async void NewWorkspaceOnClick(object sender, RoutedEventArgs e)
        {
            var name = NewWorkspaceName.Text;
            var request = new CreateNewLibraryElementRequest(SessionController.Instance.GenerateId(),null,ElementType.Collection,name);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            Init();
            //IsLocal = true;
            //this.Frame.Navigate(typeof(SessionView));
        }
        private async void Join_Workspace_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItems.Count == 1)
            {
                var item = List.SelectedItems.First();
                var id = ((CollectionTextBox) item).ID;
                var url = "https://" + ServerName + "/api/getworkspace/"+id;
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
                JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                var cred = new Dictionary<string, string>();
                
                cred["user"] = Convert.ToBase64String(Encrypt(usernameInput.Text));
                cred["pass"] = Convert.ToBase64String(Encrypt(passwordInput.Text));
                
                var url = "https://" + ServerName + "/api/login/" ;
                var client = new HttpClient(
                 new HttpClientHandler
                 {
                     ClientCertificateOptions = ClientCertificateOption.Automatic
                 });
                string getData;
                var getResponse = await client.GetAsync(url);
                using (var content = getResponse.Content)
                {
                    getData = await content.ReadAsStringAsync();
                }
                try
                {
                    var timestamp = long.Parse(getData);
                    cred["timestamp"] = timestamp.ToString();
                }
                catch (Exception longParseException)
                {
                    throw new Exception("error trying to parse timestamp to long");
                }

                string data;
                var text = JsonConvert.SerializeObject(cred,settings);
                var response = await client.PostAsync(new Uri(url),new StringContent(text, Encoding.UTF8, "application/xml"));
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
                bool validCredentials;
                string serverSessionId;
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(data);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(doc.ChildNodes[0].InnerText);
                    validCredentials = bool.Parse(dict["valid"]);
                    serverSessionId = dict["server_session_id"];
                }
                catch (Exception boolParsException)
                {
                    Debug.WriteLine("error parsing bool and serverSessionId returned from server");
                    validCredentials = false;
                    serverSessionId = null;
                }
                if (validCredentials)
                {
                    ServerSessionID = serverSessionId;
                    try
                    {
                        await SessionController.Instance.NuSysNetworkSession.Init();

                        loggedInText.Text = "Logged In!";

                        NewWorkspaceButton.IsEnabled = true;
                        JoinWorkspaceButton.IsEnabled = true;
                        LoginButton.IsEnabled = false;
                    }
                    catch (ServerClient.IncomingDataReaderException loginException)
                    {
                        loggedInText.Text = "Log in failed!";
                        throw new Exception("Your account is probably already logged in");
                    }
                }
                else
                {
                    loggedInText.Text = "Log in failed!";
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

        private partial class CollectionTextBox : TextBox
        {
            public string ID { set; get; }

            public CollectionTextBox() : base()
            {
                IsEnabled = false;
                Background = new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
