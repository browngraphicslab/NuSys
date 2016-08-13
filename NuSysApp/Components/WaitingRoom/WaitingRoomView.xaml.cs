using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using Windows.UI.Xaml.Input;
using Newtonsoft.Json.Linq;
using NusysIntermediate;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NuSysApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WaitingRoomView : Page
    {
        public FreeFormViewer _freeFormViewer;
        
        public static string InitialWorkspaceId { get; set; }
        public static string ServerName { get; private set; }
        public static string UserName { get; private set; }
        //public static string Password { get; private set; }
        public static string ServerSessionID { get; private set; }

        public static bool TEST_LOCAL_BOOLEAN = false;

        public static bool IS_HUB = false;

        private static IEnumerable<ElementModel> _firstLoadList;
        private bool _loggedIn = false;
        private bool _isLoaded = false;

        private bool _collectionAdded = false;

        private static string LoginCredentialsFilePath;

        private HashSet<string> _preloadedIDs = new HashSet<string>();
        public WaitingRoomView()
        {
            this.InitializeComponent();
            LoginCredentialsFilePath = StorageUtil.CreateFolderIfNotExists(KnownFolders.DocumentsLibrary, Constants.FolderNusysTemp).Result.Path + "\\LoginInfo.json";
            //waitingroomanimation.Begin();

            //TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = true;
            //  App.TelemetryClient.Context.Device.

            App.TelemetryClient.InstrumentationKey = "8f830614-4100-43cd-a0c9-5b94ada7b3f6";
            App.TelemetryClient.Context.InstrumentationKey = "8f830614-4100-43cd-a0c9-5b94ada7b3f6";

            App.TelemetryClient.TrackEvent("woo", new Dictionary<string, string>());

            //  Telemetry.Init();
            //  Telemetry.TrackEvent("startup");

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            //ServerName = TEST_LOCAL_BOOLEAN ? "localhost:54764" : "nusysrepo.azurewebsites.net";
            ServerName = TEST_LOCAL_BOOLEAN ? "localhost:2685" : "nusysrepo.azurewebsites.net";
            //ServerName = "172.20.10.4:54764";
            //ServerName = "nusysrepo.azurewebsites.net";
            ServerNameText.Text = ServerName;
            ServerNameText.TextChanged += delegate
            {
                ServerName = ServerNameText.Text;
                //Init();
            };

            //Init();

            SlideOutLogin.Completed += SlideOutLoginComplete;

            //AutoLogin();
        }

        private void SlideOutLoginComplete(object sender, object e)
        {
            login.Visibility = Visibility.Collapsed;
        }

        private async void Init()
        {

            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            try
            {
                var all = new List<CollectionTextBox>();
                var libraryElements = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                foreach (var libraryElement in libraryElements)
                {
                    if (SessionController.Instance.ContentController.GetLibraryElementModel(libraryElement.LibraryElementId) == null)
                    {
                        SessionController.Instance.ContentController.Add(libraryElement);
                    }
                    if (libraryElement.Type == NusysConstants.ElementType.Collection)
                    {
                        var box = new CollectionTextBox(libraryElement.Title, libraryElement.LibraryElementId);
                        all.Add(box);
                    }
                }


                List?.Items?.Clear();
                all.Sort((a, b) => a.Text.CompareTo(b.Text));
                foreach (var i in all)
                {
                    List.Items.Add(i);
                }
                _collectionAdded = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("not a valid server");
                // TODO: fix this
            }


        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SessionView));
        }
        private async void NewWorkspaceOnClick(object sender, RoutedEventArgs e)
        {
            var name = NewWorkspaceName.Text;
            var props = new Message();
            props[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = NusysConstants.ElementType.Collection;
            props[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = name;
            var request = new CreateNewContentRequest(NusysConstants.ContentType.Text, null, props);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            Init();

        }
        private async void Join_Workspace_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItems.Count == 1)
            {
                SessionController.Instance.ContentController.OnNewContent -= ContentControllerOnOnNewContent;

                var item = List.SelectedItems.First();
                var id = ((CollectionTextBox) item).ID;
                var collectionRequest = new GetEntireWorkspaceRequest(id ?? "test");
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(collectionRequest);
                foreach (var content in collectionRequest.GetReturnedContentDataModels())
                {
                    SessionController.Instance.ContentController.AddContentDataModel(content);
                }
                _firstLoadList = collectionRequest.GetReturnedElementModels();
                InitialWorkspaceId = id;
                this.Frame.Navigate(typeof(SessionView));
                
            }
        }

        public static IEnumerable<ElementModel> GetFirstLoadList()
        {
            if (_firstLoadList == null)
            {
                return new List<ElementModel>();
            }
            var l = new List<ElementModel>(_firstLoadList);
            _firstLoadList = null;
            return l;
        }
        private async void NewUser_OnClick(object sender, RoutedEventArgs e)
        {
            var username = usernameInput.Text;
            var password = Convert.ToBase64String(Encrypt(passwordInput.Password));
            Login(username, password, true);
        }
        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            var username = usernameInput.Text;
            var password = Convert.ToBase64String(Encrypt(passwordInput.Password));
            Login(username,password,false);
        }

        private async void AutoLogin()
        {
            Debug.WriteLine("fix this");
            Task.Run(async delegate
            {
                if (File.Exists(LoginCredentialsFilePath))
                {
                    UITask.Run(async delegate
                    {
                        Tuple<string, string> creds = this.GetLoginCredentials();
                        Login(creds.Item1, creds.Item2, false);
                    });
                }
            });
        }

        private Tuple<string, string> GetLoginCredentials()
        {
            return Task.Run(async delegate
            {
                JObject o1 = JObject.Parse(File.ReadAllText(LoginCredentialsFilePath));
                var username = o1.GetValue("Username").ToString();
                var password = o1.GetValue("Password").ToString();



                Tuple<string, string> credentials = new Tuple<string, string>(username, password);
                return credentials;
            }).Result;
        }

        private void SaveLoginInfo(string username, string password)
        {
            JObject loginCredentials = new JObject(new JProperty("Username", username), new JProperty("Password", password));
            File.WriteAllText(LoginCredentialsFilePath, loginCredentials.ToString());
        }

        private async void Login(string username, string password, bool createNewUser)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                var cred = new Dictionary<string, string>();

                //cred["user"] = Convert.ToBase64String(Encrypt(usernameInput.Text));


                cred["user"] = username;
                cred["pass"] = password;
                if (createNewUser)
                {
                    cred["new_user"] = "";
                }
                var url = (TEST_LOCAL_BOOLEAN ? "http://" : "https://") + ServerName + "/api/nusyslogin/";
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
                var text = JsonConvert.SerializeObject(cred, settings);
                var response = await client.PostAsync(new Uri(url), new StringContent(text, Encoding.UTF8, "application/xml"));
                using (var content = response.Content)
                {
                    data = await content.ReadAsStringAsync();
                }
                bool validCredentials;
                string serverSessionId;
                string userID = "";
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(data);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(doc.ChildNodes[0].InnerText);
                    validCredentials = bool.Parse(dict["valid"]);
                    if (dict.ContainsKey("user_id"))
                    {
                        userID = dict["user_id"].ToString();
                    }
                    serverSessionId = dict.ContainsKey("server_session_id") ? dict["server_session_id"] : "";
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
                        SessionController.Instance.LocalUserID = userID;

                        SessionController.Instance.ContentController.OnNewContent += ContentControllerOnOnNewContent;

                        loggedInText.Text = "Logged In!";

                        NewWorkspaceButton.IsEnabled = true;
                        _loggedIn = true;
                        if (_isLoaded)
                        {
                            UITask.Run(delegate
                            {
                                JoinWorkspaceButton.Content = "Enter";
                                JoinWorkspaceButton.IsEnabled = true;
                                JoinWorkspaceButton.Visibility = Visibility.Visible;
                            });
                        }
                        LoginButton.IsEnabled = false;
                        SlideOutLogin.Begin();
                        SlideInWorkspace.Begin();

                        UserName = userID;
                        if (userID.ToLower() != "rosemary" && userID.ToLower()!= "rms" && userID.ToLower() != "gfxadmin")
                        {
                            foreach(var box in List.Items)
                            {
                                if((box as CollectionTextBox).MadeByRosemary)
                                {
                                    List.Items.Remove(box);
                                }
                            }
                        }

                        await Task.Run(async delegate
                        { 
                            var models = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                            foreach (var model in models)
                            {
                                try
                                {
                                    SessionController.Instance.ContentController.Add(model);
                                }
                                catch (NullReferenceException e)
                                {
                                    Debug.WriteLine(" this shouldn't ever happen.  trent was too lazy to do error hadnling");
                                }
                            }
                            _isLoaded = true;
                            if (_loggedIn)
                            {
                                UITask.Run(delegate {
                                    JoinWorkspaceButton.IsEnabled = true;
                                    JoinWorkspaceButton.Content = "Enter";
                                    JoinWorkspaceButton.Visibility = Visibility.Visible;
                                });
                                if (!File.Exists(LoginCredentialsFilePath))
                                {
                                    this.SaveLoginInfo(cred["user"], cred["pass"]);
                                }
                            }
                        });
                    }
                    catch (ServerClient.IncomingDataReaderException loginException)
                    {
                        loggedInText.Text = "Log in failed!";
                        //     throw new Exception("Your account is probably already logged in");
                    }
                }
                else
                {
                    loggedInText.Text = "Log in failed!";
                    /*
                    if (!createNewUser) { 
                        Login(true);
                    }*/
                }

            }
            catch (HttpRequestException h)
            {
                Debug.WriteLine("cannot connect to server");
            }

        }

        private void ContentControllerOnOnNewContent(LibraryElementModel element)
        {
            if (element.Type == NusysConstants.ElementType.Collection && !_preloadedIDs.Contains(element.LibraryElementId))
            {
                var items = new List<CollectionTextBox>();
 
                UITask.Run(delegate
                {
                    var box = new CollectionTextBox(element.Title ?? "", element.LibraryElementId);
                    if (_collectionAdded == false) { List.Items.Add(box); }
                    _collectionAdded = false;
                });

                _preloadedIDs.Add(element.LibraryElementId);
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
            public bool MadeByRosemary = false;

            public CollectionTextBox(string text, string Id) : base()
            {
                ID = Id;
                IsEnabled = false;
                Background = new SolidColorBrush(Colors.Transparent);
                base.Text = text;
            }
        }
    }
}
