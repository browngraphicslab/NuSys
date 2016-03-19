﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

        public static bool TEST_LOCAL_BOOLEAN = false;
        private static IEnumerable<string> _firstLoadList;
        private bool _loggedIn = false;
        private bool _isLoaded = false;
        public WaitingRoomView()
        {
            this.InitializeComponent();
            //waitingroomanimation.Begin();

            
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;

            ServerName = TEST_LOCAL_BOOLEAN ? "localhost:54764" : "nusysrepo.azurewebsites.net";
            //ServerName = "nusysrepo.azurewebsites.net";
            ServerNameText.Text = ServerName;
            ServerNameText.TextChanged += delegate
            {
                ServerName = ServerNameText.Text;
                Init();
            };
            
            Init();
        }

        private async void Init()
        {
            List?.Items?.Clear();
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            try
            {
                var url = (TEST_LOCAL_BOOLEAN ? "http://" : "https://") + ServerName + "/api/getworkspace";
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
                var url = (TEST_LOCAL_BOOLEAN ? "http://" : "https://") + ServerName + "/api/getworkspace/"+id;
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
                cred["pass"] = Convert.ToBase64String(Encrypt(passwordInput.Password));
                
                var url = (TEST_LOCAL_BOOLEAN ? "http://" : "https://") + ServerName + "/api/login/" ;
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

                        SessionController.Instance.ContentController.OnNewContent += delegate (LibraryElementModel element)
                        {
                            if (element.Type == ElementType.Collection)
                            {
                                UITask.Run(delegate
                                {
                                    var box = new CollectionTextBox();
                                    box.ID = element.Id;
                                    box.Text = element.Title ?? "Unnamed Collection";
                                    List.Items.Add(box);
                                });
                            }
                        };

                        loggedInText.Text = "Logged In!";

                        NewWorkspaceButton.IsEnabled = true;
                        _loggedIn = true;
                        if (_isLoaded)
                        {
                            UITask.Run(delegate {
                                JoinWorkspaceButton.IsEnabled = true;
                                JoinWorkspaceButton.Visibility = Visibility.Visible;
                            });
                        }
                        LoginButton.IsEnabled = false;
                        SlideOutLogin.Begin();
                        SlideInWorkspace.Begin();

                        await Task.Run(async delegate
                        {
                            var dictionaries = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                            foreach (var kvp in dictionaries)
                            {
                                var id = (string)kvp.Value["id"];
                                //var element = new LibraryElementModel(kvp.Value);

                                var dict = kvp.Value;

                                string title = null;
                                ElementType type = ElementType.Text;

                                if (dict.ContainsKey("title"))
                                {
                                    title = (string)dict["title"]; // title
                                }
                                if (dict.ContainsKey("type"))
                                {
                                    try
                                    {
                                        type = (ElementType)Enum.Parse(typeof(ElementType), (string)dict["type"], true);
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }
                                }

                                LibraryElementModel element;
                                if (type == ElementType.Collection)
                                {
                                    element = new CollectionLibraryElementModel(id, title);
                                }
                                else
                                {
                                    element = new LibraryElementModel(id, type, title);
                                }
                                if (SessionController.Instance.ContentController.Get(id) == null)
                                {
                                    SessionController.Instance.ContentController.Add(element);
                                }
                            }
                            _isLoaded = true;
                            if (_loggedIn)
                            {
                                UITask.Run(delegate {
                                    JoinWorkspaceButton.IsEnabled = true;
                                    JoinWorkspaceButton.Visibility = Visibility.Visible;
                                });
                            }
                        });
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
