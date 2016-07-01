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
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using Windows.UI.Xaml.Input;
using Newtonsoft.Json.Linq;

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

        public static bool TEST_LOCAL_BOOLEAN = true;

        public static bool IS_HUB = false;

        private static IEnumerable<Message> _firstLoadList;
        private bool _loggedIn = false;
        private bool _isLoaded = false;

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

            ServerName = TEST_LOCAL_BOOLEAN ? "localhost:54764" : "nusysrepo.azurewebsites.net";
            //ServerName = "172.20.10.4:54764";
            //ServerName = "nusysrepo.azurewebsites.net";
            ServerNameText.Text = ServerName;
            ServerNameText.TextChanged += delegate
            {
                ServerName = ServerNameText.Text;
                Init();
            };

            Init();

            SlideOutLogin.Completed += SlideOutLoginComplete;

            AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(Prresseed), true);
            AutoLogin();
        }

        private void SlideOutLoginComplete(object sender, object e)
        {
            login.Visibility = Visibility.Collapsed;
        }

        private void Prresseed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("asdasds");
        }

        private async void Init()
        {
            Keyword k = new Keyword("test");
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
                list.Sort();
                List?.Items?.Clear();
                var ii = new List<CollectionTextBox>();
                foreach (var s in list)
                {
                    Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(s, settings);
                    var box = new CollectionTextBox();
                    box.ID = dict.ContainsKey("id") ? (string)dict["id"] : null;//todo do error handinling since this shouldnt be null
                    if (dict.ContainsKey("creator_user_id"))
                    {
                        var creator = dict["creator_user_id"].ToString().ToLower();
                        if(creator == "rms" || creator == "rosemary" || creator == "gfxadmin")
                        {
                            box.MadeByRosemary = true;
                        }
                    }
                    if (dict.ContainsKey("title") && dict["title"] != null && dict["title"] != "")
                        box.Text = (string)dict["title"];
                    else
                        box.Text = "Unnamed Collection";
                    //List.Items.Add(box);
                    ii.Add(box);
                    _preloadedIDs.Add(box.ID);
                }

                ii.Sort((a, b) => a.Text.CompareTo(b.Text));
                foreach (var i in ii)
                {
                    List.Items.Add(i);
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
            this.Frame.Navigate(typeof(SessionView));
        }
        private async void NewWorkspaceOnClick(object sender, RoutedEventArgs e)
        {
            var name = NewWorkspaceName.Text;
            var request = new CreateNewLibraryElementRequest(SessionController.Instance.GenerateId(), null, ElementType.Collection, name);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            await Task.Delay(1000);
            Init();
        }
        private async void Join_Workspace_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItems.Count == 1)
            {
                SessionController.Instance.ContentController.OnNewContent -= ContentControllerOnOnNewContent;

                var item = List.SelectedItems.First();
                var id = ((CollectionTextBox)item).ID;
                _firstLoadList = await SessionController.Instance.NuSysNetworkSession.GetCollectionAsElementMessages(id);
                InitialWorkspaceId = id;
                this.Frame.Navigate(typeof(SessionView));
            }
        }

        public static IEnumerable<Message> GetFirstLoadList()
        {
            if (_firstLoadList == null)
            {
                return new List<Message>();
            }
            var l = new List<Message>(_firstLoadList);
            _firstLoadList = null;
            return l;
        }
        private async void NewUser_OnClick(object sender, RoutedEventArgs e)
        {
            Login(true);
        }
        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            Login(false);
        }

        private async void AutoLogin()
        {
            Task.Run(async delegate
            {
                if (File.Exists(LoginCredentialsFilePath))
                {
                    await UITask.Run(async delegate { 
                    Tuple<string, string> creds = this.GetLoginCredentials();

                    try
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                        };
                        var cred = new Dictionary<string, string>();

                        //cred["user"] = Convert.ToBase64String(Encrypt(usernameInput.Text));


                        cred["user"] = creds.Item1;
                        cred["pass"] = creds.Item2;
                        var url = (TEST_LOCAL_BOOLEAN ? "http://" : "https://") + ServerName + "/api/login/";
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
                        var response =
                            await
                                client.PostAsync(new Uri(url), new StringContent(text, Encoding.UTF8, "application/xml"));
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
                            var dict =
                                JsonConvert.DeserializeObject<Dictionary<string, string>>(doc.ChildNodes[0].InnerText);
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

                                SessionController.Instance.ContentController.OnNewContent +=
                                    ContentControllerOnOnNewContent;

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
                                await UITask.Run(async delegate
                                {
                                    UserName = userID;
                                    if (userID.ToLower() != "rosemary" && userID.ToLower() != "rms" &&
                                        userID.ToLower() != "gfxadmin")
                                    {
                                        foreach (var box in List.Items)
                                        {
                                            if ((box as CollectionTextBox).MadeByRosemary)
                                            {
                                                List.Items.Remove(box);
                                            }
                                        }
                                    }
                                });
                                await Task.Run(async delegate
                                {
                                    var dictionaries =
                                        await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                                    foreach (var kvp in dictionaries)
                                    {
                                        var id = (string) kvp.Value["id"];
                                        //var element = new LibraryElementModel(kvp.Value);

                                        bool favorited = false;
                                        var metadata =
                                            new Dictionary<string, MetadataEntry>();
                                        var dict = kvp.Value;
                                        var message = new Message(dict);
                                        string title = null;
                                        ElementType type = ElementType.Text;
                                        string timestamp = "";
                                        string creator = null;
                                        string serverUrl = null;
                                        if (dict.ContainsKey("library_element_creation_timestamp"))
                                        {
                                            timestamp = dict["library_element_creation_timestamp"].ToString();
                                        }
                                        if (dict.ContainsKey("favorited") &&
                                            bool.Parse(dict["favorited"].ToString()) == true)
                                        {
                                            favorited = true;
                                        }
                                        if (dict.ContainsKey("metadata"))
                                        {

                                            if (dict["metadata"] != null)
                                            {
                                                metadata =
                                                    JsonConvert
                                                        .DeserializeObject<Dictionary<string, MetadataEntry>>(
                                                            dict["metadata"].ToString());
                                            }

                                        }

                                        if (dict.ContainsKey("creator_user_id"))
                                        {
                                            creator = dict["creator_user_id"].ToString();
                                        }
                                        if (dict.ContainsKey("title"))
                                        {
                                            title = (string) dict["title"]; // title
                                        }
                                        if (dict.ContainsKey("server_url"))
                                        {
                                            serverUrl = dict["server_url"].ToString();
                                        }
                                        if (dict.ContainsKey("type"))
                                        {
                                            try
                                            {
                                                type =
                                                    (ElementType)
                                                        Enum.Parse(typeof(ElementType), (string) dict["type"], true);
                                            }
                                            catch (Exception ex)
                                            {
                                                continue;
                                            }
                                        }

                                        LibraryElementModel element;
                                        if (type == ElementType.Collection)
                                        {
                                            element = new CollectionLibraryElementModel(id, metadata, title, favorited);
                                        }
                                        else
                                        {
                                            element = new LibraryElementModel(id, type, metadata, title, favorited);
                                        }
                                        element.UnPack(message);
                                        element.Creator = creator;
                                        element.Timestamp = timestamp;
                                        element.ServerUrl = serverUrl;
                                        if (SessionController.Instance.ContentController.GetContent(id) == null)
                                        {
                                            SessionController.Instance.ContentController.Add(element);
                                        }
                                        
                                        

                                    }
                                    _isLoaded = true;
                                    if (_loggedIn)
                                    {
                                        UITask.Run(delegate
                                        {
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
                })
                ;
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

        private async void Login(bool createNewUser)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                var cred = new Dictionary<string, string>();

                //cred["user"] = Convert.ToBase64String(Encrypt(usernameInput.Text));


                cred["user"] = usernameInput.Text;
                cred["pass"] = Convert.ToBase64String(Encrypt(passwordInput.Password));
                if (createNewUser)
                {
                    cred["new_user"] = "";
                }
                var url = (TEST_LOCAL_BOOLEAN ? "http://" : "https://") + ServerName + "/api/login/";
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
                            var dictionaries = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                            foreach (var kvp in dictionaries)
                            {
                                var id = (string)kvp.Value["id"];
                                //var element = new LibraryElementModel(kvp.Value);

                                bool favorited = false;
                                var metadata = new Dictionary<string, MetadataEntry>();
                                var dict = kvp.Value;
                                var message = new Message(dict);
                                string title = null;
                                ElementType type = ElementType.Text;
                                string timestamp = "";
                                string creator = null;
                                string serverUrl = null;
                                if (dict.ContainsKey("library_element_creation_timestamp"))
                                {
                                    timestamp = dict["library_element_creation_timestamp"].ToString();
                                }
                                if (dict.ContainsKey("favorited") && bool.Parse(dict["favorited"].ToString()) == true)
                                {
                                    favorited = true;
                                }
                                if (dict.ContainsKey("metadata"))
                                {

                                    if (dict["metadata"] != null)
                                    {
                                        metadata = JsonConvert.DeserializeObject<Dictionary<string, MetadataEntry>>(dict["metadata"].ToString());
                                    }

                                }

                                if (dict.ContainsKey("creator_user_id"))
                                {
                                    creator = dict["creator_user_id"].ToString();
                                }
                                if (dict.ContainsKey("title"))
                                {
                                    title = (string)dict["title"]; // title
                                }
                                if (dict.ContainsKey("server_url"))
                                {
                                    serverUrl = dict["server_url"].ToString();
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
                                    element = new CollectionLibraryElementModel(id, metadata, title, favorited);
                                }
                                else if (type == ElementType.Link)
                                {
                                    Color color;
                                    if (dict.ContainsKey("color"))
                                    {
                                        string hexColor = dict["color"] as string;
                                        byte a = byte.Parse(hexColor.Substring(1, 2), NumberStyles.HexNumber);
                                        byte r = byte.Parse(hexColor.Substring(3, 2), NumberStyles.HexNumber);
                                        byte g = byte.Parse(hexColor.Substring(5, 2), NumberStyles.HexNumber);
                                        byte b = byte.Parse(hexColor.Substring(7, 2), NumberStyles.HexNumber);
                                        Color.FromArgb(a, r, g, b);
                                    }
                                    color = Colors.Tomato;
                                    if (dict.ContainsKey("id1") && dict.ContainsKey("id2"))
                                    {
                                        element = new LinkLibraryElementModel(dict["id1"] as string,
                                            dict["id2"] as string, id, color, type, metadata, title, favorited);
                                    }
                                    else
                                    {
                                        element = null;
                                    }
                                }
                                else
                                {
                                    element = new LibraryElementModel(id, type, metadata, title, favorited);
                                }
                                if (element != null)
                                {
                                    element.UnPack(message);
                                    element.Creator = creator;
                                    element.Timestamp = timestamp;
                                    element.ServerUrl = serverUrl;
                                    if (SessionController.Instance.ContentController.GetContent(id) == null)
                                    {
                                        SessionController.Instance.ContentController.Add(element);
                                    }
                                    if (type == ElementType.Link)
                                    {
                                        SessionController.Instance.LinkController.AddLink(id);
                                    }
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
            if (element.Type == ElementType.Collection && !_preloadedIDs.Contains(element.LibraryElementId))
            {
                UITask.Run(delegate
                {
                    var box = new CollectionTextBox();
                    box.ID = element.LibraryElementId;
                    box.Text = element.Title ?? "";
                    List.Items.Add(box);
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

            public CollectionTextBox() : base()
            {
                IsEnabled = false;
                Background = new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
