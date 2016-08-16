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

        

        public static bool IS_HUB = false;

        private static IEnumerable<ElementModel> _firstLoadList;
        private bool _loggedIn = false;
        private bool _isLoaded = false;

        //makes sure collection doesn't get added twice
        private bool _collectionAdded = false;

        private static string LoginCredentialsFilePath;

        //list of all collections
        private List<LibraryElementModel> _collectionList;

        //selected collection by user
        private LibraryElementModel _selectedCollection = null;

        //sort booleans for reverse
        private bool _titleReverse;
        private bool _dateReverse;
        private bool _accessReverse;

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
            ServerName = NusysConstants.TEST_LOCAL_BOOLEAN ? "localhost:2685" : "nusysrepo.azurewebsites.net";
            //ServerName = "172.20.10.4:54764";
            //ServerName = "nusysrepo.azurewebsites.net";
            ServerNameText.Text = ServerName;
            ServerNameText.TextChanged += delegate
            {
                ServerName = ServerNameText.Text;

            };
            
            SlideOutLogin.Completed += SlideOutLoginComplete;

            //AutoLogin();

            _selectedCollection = null;
            _titleReverse = false;
            _dateReverse = false;
            _accessReverse = false;
        }

        private void SlideOutLoginComplete(object sender, object e)
        {
            login.Visibility = Visibility.Collapsed;
            NuSysTitle.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// initializes collection listview
        /// </summary>
        private async void Init()
        {

            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            try
            {
                var all = new List<CollectionListBox>();
                var libraryElements = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                foreach (var libraryElement in libraryElements)
                {
                    if (SessionController.Instance.ContentController.GetLibraryElementModel(libraryElement.LibraryElementId) == null)
                    {
                        SessionController.Instance.ContentController.Add(libraryElement);
                    }
                    //if the libraryelement is of type collection, make a collectionlistbox for it and also add to the collectionlist
                    if (libraryElement.Type == NusysConstants.ElementType.Collection)
                    {
                        var i = new CollectionListBox(libraryElement);
                        all.Add(i);
                        _collectionList.Add(libraryElement);
                    }
                }
                //set items in collectionlist alphabetically
                List?.Items?.Clear();
                all.Sort((a, b) => a.Title.CompareTo(b.Title));
                foreach (var i in all)
                {
                    List?.Items.Add(i);
                }
                //makes sure collection doesn't get added twice
                _collectionAdded = true;
                //next time title is clicked, it will reverse the list
                _titleReverse = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("not a valid server");
                // TODO: fix this
            }


        }

        private void NewButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewWorkspacePopup.HorizontalOffset = this.ActualWidth/2 - 250;
            NewWorkspacePopup.VerticalOffset = this.ActualHeight/2 - 110;
            NewWorkspacePopup.IsOpen = true;
        }

        private void ClosePopupOnClick(object sender, RoutedEventArgs e)
        {
            NewWorkspacePopup.IsOpen = false;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SessionView));
        }

        private async void NewWorkspaceOnClick(object sender, RoutedEventArgs e)
        {
            var name = NewWorkspaceName.Text;
            var props = new Message();
            props[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = NusysConstants.ElementType.Collection.ToString();
            props[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = name;
            var request = new CreateNewContentRequest(NusysConstants.ContentType.Text, null, props);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            Init();
            NewWorkspaceName.Text = "";
            NewWorkspacePopup.IsOpen = false;
        }
        private async void Join_Workspace_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCollection != null)
            {
                var isReadOnly = true;
                SessionController.Instance.ContentController.OnNewContent -= ContentControllerOnOnNewContent;
                var id = _selectedCollection.LibraryElementId;
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

        /// <summary>
        /// changes the window of preview information based on the selected collection.
        /// also sets selected collection from the list. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (List.SelectedItem != null)
            {
                var item = List.SelectedItem;
                var id = ((CollectionListBox)item).ID;
                //set selected collection
                _selectedCollection =
                    SessionController.Instance.ContentController.GetLibraryElementController(id).LibraryElementModel;
                //set properties in preview window
                CreatorText.Text = _selectedCollection.Creator;
                LastEditedText.Text = _selectedCollection.LastEditedTimestamp;
                CreateDateText.Text = _selectedCollection.Timestamp;

                SearchBox.Text = "";

                PreviewPanel.Visibility = Visibility.Collapsed;
                DetailPanel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// autosuggest for collection list searchbox.
        /// comes up with list of collections based on the characters entered in the search text area,
        /// and sets this list as the searchbox's item source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_Suggest(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //turn collectionlist into a list of strings so we can compare titles to text entered
                var titlelist = new List<string>();
                foreach (LibraryElementModel m in _collectionList)
                {
                    titlelist.Add(m.Title);
                }
                //filters collections for suggestion list based on text already entered
                var filteredCollections = titlelist.Where(t => t.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant()));
                SearchBox.ItemsSource = filteredCollections;
            }


        }

        /// <summary>
        /// turns a submitted query into the selected collection
        /// if submitted using the enter key or the queryicon, it will check the text in the box itself.
        /// if a user clicks on a suggestion from the list it will check the text in that selection.
        /// also highlights selected collection on the listview. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Searched(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            var selected = new List<LibraryElementModel>();
            //suggestion is chosen from list
            if (e.ChosenSuggestion != null)
            {
                selected = _collectionList.Where(s => s.Title == e.ChosenSuggestion).ToList();
            }
            //suggestion is submitted through query icon or enter key
            else
            {
                selected = _collectionList.Where(s => s.Title == sender.Text).ToList();
            }
            //set selected collection and highlight it in the listview
            if (selected.Count == 1)
            {
                _selectedCollection = selected[0];
                foreach (CollectionListBox i in List.Items)
                {
                    if (i.Title == _selectedCollection.Title)
                    {
                        List.SelectedItem = i;
                    }
                }
                List.ScrollIntoView(List.SelectedItem);
                PreviewPanel.Visibility = Visibility.Collapsed;
                DetailPanel.Visibility = Visibility.Visible;
            }
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
                var url = (NusysConstants.TEST_LOCAL_BOOLEAN ? "http://" : "https://") + ServerName + "/api/nusyslogin/";
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
                    throw new Exception("error trying to parse timestamp too long");
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
                        _collectionList = new List<LibraryElementModel>();
                        Init();
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

                            foreach (var box in List.Items)
                            {
                                if ((box as CollectionListBox).MadeByRosemary)
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

        /// <summary>
        /// Sort for collectionlist
        /// for now not sorting by current users - could sort it by how many users are on the workspace potentially? idk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortList_OnClick(object sender, RoutedEventArgs e)
        {
            var collections = List?.Items.ToList();
            List?.Items?.Clear();
            var sorttype = ((Button) sender).Name;
            
            switch (sorttype)
            {
                case "TitleHeader":
                    //sort by title string comparison and reverse if necessary, and make sure you set the reverse bool again
                    collections.Sort((a, b) => (a as CollectionListBox).Title.CompareTo((b as CollectionListBox).Title));
                    if (_titleReverse)
                    {
                        collections.Reverse();
                        _titleReverse = false;
                    }
                    else
                    {
                        _titleReverse = true;
                    }
                    break;
                case "AccessHeader":
                    collections.Sort(
                        (a, b) => (a as CollectionListBox).Access.CompareTo((b as CollectionListBox).Access));
                    if (_accessReverse)
                    {
                        collections.Reverse();
                        _accessReverse = false;
                    }
                    else
                    {
                        _accessReverse = true;
                    }
                    break;
                case "DateHeader":
                    collections.Sort((a, b) => (a as CollectionListBox).Date.CompareTo((b as CollectionListBox).Date));
                    if (_dateReverse)
                    {
                        collections.Reverse();
                        _dateReverse = false;
                    }
                    else
                    {
                        _dateReverse = true;
                    }
                    break;
                default:

                    break;
            }

            foreach (var i in collections)
            {
                List?.Items?.Add(i);
            }
        }
    }
}
