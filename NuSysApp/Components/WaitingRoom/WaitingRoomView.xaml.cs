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
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
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
        public enum SortType { TitleAsc, TitleDesc, DateAsc, DateDesc, AccessAsc, AccessDesc}
        private SortType _currentSortType = SortType.TitleAsc;
        public enum FilterType { RecentlyUsed, Mine, Others, All}
        private FilterType _currentFilterType = FilterType.All;


        public FreeFormViewer _freeFormViewer;

        public static string InitialWorkspaceId { get; private set; }
        public static string ServerName { get; private set; }
        public static string UserID { get; private set; }

        public static string UserName { get; private set; }
        //public static string Password { get; private set; }
        public static string ServerSessionID { get; private set; }

        public static string HashedPass { get; private set; }

        public static bool IS_HUB = true;

        private static IEnumerable<ElementModel> _firstLoadList;
        private bool _loggedIn = false;
        private bool _isLoaded = false;
        private bool _isLoggingIn = false;
        //makes sure collection doesn't get added twice
        private bool _collectionAdded = false;

        private static string LoginCredentialsFilePath;

        //list of all collections
        private List<LibraryElementModel> _collectionList;

        //selected collection by user
        private LibraryElementModel _selectedCollection = null;


        private HashSet<string> _preloadedIDs = new HashSet<string>();

        public static WaitingRoomView Instance;//singleton now

        public WaitingRoomView() //TODO make this private, like an actual singleton
        {
            Instance = this;
            this.InitializeComponent();
            LoginCredentialsFilePath = StorageUtil.CreateFolderIfNotExists(KnownFolders.DocumentsLibrary, Constants.FolderNusysTemp).Result.Path + "\\LoginInfo.json";

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            //ServerName = TEST_LOCAL_BOOLEAN ? "localhost:54764" : "nusysrepo.azurewebsites.net";
            ServerName = NusysConstants.TEST_LOCAL_BOOLEAN ? "localhost:2776" : "nusysrepo.azurewebsites.net";
            //ServerName = "172.20.10.4:54764";
            //ServerName = "nusysrepo.azurewebsites.net";
            ServerNameText.Text = ServerName;
            //ServerNameText.TextChanged += delegate
            //{
            //    ServerName = ServerNameText.Text;

            //};

            //SlideOutLogin.Completed += SlideOutLoginComplete;
            //SlideOutNewUser.Completed += SlideOutLoginComplete;

            //AutoLogin();

            ellipse.Begin();
            _selectedCollection = null;

            // Every time a new collection is added by another user, the list of collections is refreshed by calling Init
            SessionController.Instance.ContentController.OnNewLibraryElement += ContentController_OnNewLibraryElememt;
            SessionController.Instance.ContentController.OnLibraryElementDelete += ContentControllerOnOnLibraryElementDelete;
        }

        private async void ContentControllerOnOnLibraryElementDelete(LibraryElementModel model)
        {
            if (model.Type != NusysConstants.ElementType.Collection)
                return;
            if (_preloadedIDs.Contains(model.LibraryElementId)) { 
                _preloadedIDs.Remove(model.LibraryElementId);
            }

            var libraryElement = SessionController.Instance.ContentController.GetLibraryElementModel(model.LibraryElementId);

            _collectionList.Remove(libraryElement);
            var result = List.Items.OfType<CollectionListBox>().Where(c => c.LibraryElementModel.LibraryElementId == model.LibraryElementId);
            if (result.Any())
            {
                List.Items.Remove(result.First());
            }

            await ApplyFilter(_currentFilterType);
            ApplySorting(_currentSortType);
        }

        /// <summary>
        /// This handler is responsible for refreshing the list of collections every time another user adds a new collection.
        /// </summary>
        /// <param name="model"></param>
        private async void ContentController_OnNewLibraryElememt(LibraryElementModel model)
        {
            if (model.Type != NusysConstants.ElementType.Collection)
                return;

            await UITask.Run(async delegate
            {
                if (!_preloadedIDs.Contains(model.LibraryElementId))
                {
                    _preloadedIDs.Add(model.LibraryElementId);
                }

                var libraryElement = SessionController.Instance.ContentController.GetLibraryElementModel(model.LibraryElementId);

                _collectionList.Add(libraryElement);
                List.Items.Add(new CollectionListBox(libraryElement, this));

                await ApplyFilter(_currentFilterType);
                ApplySorting(_currentSortType);
            });
        }


        /// <summary>
        /// initializes collection listview
        /// </summary>
        private async Task Init()
        {
            SessionController.Instance.ContentController.OnNewLibraryElement -= ContentController_OnNewLibraryElememt;//remove the habndler so it doesn't Init() forever
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
                        var i = new CollectionListBox(libraryElement, this);
                        all.Add(i);
                        _collectionList.Add(libraryElement);
                    }
                }

                foreach (var i in all)
                {
                    List?.Items.Add(i);
                }
                //set items in collectionlist alphabetically
                await ApplyFilter(FilterType.Mine);
                ApplySorting(SortType.TitleAsc);
                //makes sure collection doesn't get added twice
                _collectionAdded = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("not a valid server");
                // TODO: fix this
            }
            SessionController.Instance.ContentController.OnNewLibraryElement += ContentController_OnNewLibraryElememt; //re-add the handler so we start listening for new contents again
        }

        /// <summary>
        /// called when page is navigated to
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                var obj = e.Parameter;
                if (obj.GetType() == typeof(SessionView))
                {
                    var sessionview = obj as SessionView;
                    
                    _collectionList = new List<LibraryElementModel>();
                    Init();
                    NewWorkspaceButton.IsEnabled = true;
                    _loggedIn = true;

                    UITask.Run(delegate
                    {
                        JoinWorkspaceButton.Content = "Enter";
                        JoinWorkspaceButton.IsEnabled = true;
                        JoinWorkspaceButton.Visibility = Visibility.Visible;
                    });

                    login.Visibility = Visibility.Collapsed;
                    NewUser.Visibility = Visibility.Collapsed;
                    NuSysTitle.Visibility = Visibility.Collapsed;
                    workspace.Visibility = Visibility.Visible;

                    var userID = SessionController.Instance.LocalUserID;
                    if (userID.ToLower() != "rosemary" && userID.ToLower() != "rms" && userID.ToLower() != "gfxadmin")
                    {

                        foreach (var box in List.Items)
                        {
                            if ((box as CollectionListBox).MadeByRosemary)
                            {
                                List.Items.Remove(box);
                            }
                        }
                    }

                    SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser -= NewNetworkUser;
                    SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser += NewNetworkUser;
                    SessionController.Instance.NuSysNetworkSession.OnNetworkUserDropped -= DropNetworkUser;
                    SessionController.Instance.NuSysNetworkSession.OnNetworkUserDropped += DropNetworkUser;

                    foreach (var user in SessionController.Instance.NuSysNetworkSession.NetworkMembers.Values)
                    {
                        NewNetworkUser(user);
                    }
                }

            }
        }

        /// <summary>
        /// sets position of popup for new workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewWorkspacePopup.HorizontalOffset = this.ActualWidth / 2 - 250;
            NewWorkspacePopup.VerticalOffset = this.ActualHeight / 2 - 110;
            NewWorkspacePopup.IsOpen = true;
        }

        /// <summary>
        /// handler of pop up's close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClosePopupOnClick(object sender, RoutedEventArgs e)
        {
            NewWorkspacePopup.IsOpen = false;
        }

        private async void NewWorkspaceOnClick(object sender, RoutedEventArgs e)
        {
            var contentRequestArgs = new CreateNewContentRequestArgs();
            contentRequestArgs.LibraryElementArgs.Title = NewWorkspaceName.Text;
            contentRequestArgs.LibraryElementArgs.LibraryElementType = NusysConstants.ElementType.Collection;

            if (PublicButton.IsChecked.Value)
            {
                contentRequestArgs.LibraryElementArgs.AccessType = NusysConstants.AccessType.Public;
            }
            else if (PrivateButton.IsChecked.Value)
            {
                contentRequestArgs.LibraryElementArgs.AccessType = NusysConstants.AccessType.Private;
            }
            else
            {
                contentRequestArgs.LibraryElementArgs.AccessType = NusysConstants.AccessType.ReadOnly;
            }

            var request = new CreateNewContentRequest(contentRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
       //     Init();
            NewWorkspaceName.Text = "";
            NewWorkspacePopup.IsOpen = false;
        }

        public async void Join_Workspace_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCollection != null)
            {
                var id = _selectedCollection.LibraryElementId;

                Debug.Assert(!string.IsNullOrEmpty(id));

                var m = SessionController.Instance.ContentController.GetLibraryElementController(id).LibraryElementModel;
                
                InitialWorkspaceId = id;

                if ((m.AccessType == NusysConstants.AccessType.ReadOnly) && (m.Creator != UserID))
                {
                    // TODO: add back in
                   // this.Frame.Navigate(typeof(SessionView), m.AccessType);
                }
                else
                {
                    ShowWorkspace();
                    await xSessionView.Init();
                }
            }
        }

        public async Task ShowWaitingRoom()
        {
            await ApplyFilter(_currentFilterType);
            ApplySorting(_currentSortType);
            SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.Stop();
            xWaitingRoom.Visibility = Visibility.Visible;
            xSessionView.Visibility = Visibility.Collapsed;
        }

        public void ShowWorkspace()
        {
            xWaitingRoom.Visibility = Visibility.Collapsed;
            xSessionView.Visibility = Visibility.Visible;
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

        /// <summary>
        /// transitions to new user page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewUserPage(object sender, RoutedEventArgs e)
        {
            NewUser.Visibility = Visibility.Visible;
            NuSysTitle.Visibility = Visibility.Collapsed;
            SlideOutLogin.Begin();
            SlideInNewUser.Begin();
            login.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// handles back button back to login page from createnewuser page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackToLoginFromNew(object sender, RoutedEventArgs e)
        {
            login.Visibility = Visibility.Visible;
            SlideOutNewUser.Begin();
            SlideInLogin.Begin();
            NewUser.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// handles back button to login from workspace list page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackToLoginFromList(object sender, RoutedEventArgs e)
        {
            login.Visibility = Visibility.Visible;
            NuSysTitle.Visibility = Visibility.Visible;
            SlideOutWorkspace.Begin();
            SlideInLogin.Begin();
            workspace.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Adds user labels to the user stackpanel on the collection list screen.
        /// </summary>
        /// <param name="user"></param>
        private void NewNetworkUser(NetworkUser user)
        {
            UITask.Run(delegate
            {
                UserLabel b = new UserLabel(user);
                Users.Children.Add(b);
            });
        }

        public void ClearUsers()
        {
            Users?.Children?.Clear();
        }

        private async void NewUser_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isLoggingIn)
            {
                return;
            }
            bool valid = true;
            if (NewUsername.Text == "")
            {
                NewUserLoginText.Text = "Username required. ";
                valid = false;
            }
            //If new username has leading or ending white space, don't allow user creation.
            else if (NewUsername.Text != NewUsername.Text.Trim())
            {
                NewUserLoginText.Text = "Username cannot have spaces at start or end. ";
                valid = false;
            }

            if (NewDisplayName.Text == "")
            {
                if (valid == false)
                {
                    NewUserLoginText.Text += "Display name required.";
                }
                else
                {
                    NewUserLoginText.Text = "Display name required.";
                    valid = false;
                }
            }
            //If new username has leading or ending white space, don't allow user creation.
            else if (NewDisplayName.Text != NewDisplayName.Text.Trim())
            {
                if (valid == false)
                {
                    NewUserLoginText.Text += "Display cannot have spaces at start or end.";
                }
                else
                {
                    NewUserLoginText.Text = "Display cannot hav espaces at start or end.";
                    valid = false;
                }
            }

            if (valid == true)
            {
                // to prevent multiple logins we must block logins, the call to allow more logins is after the server sends back and says that 
                // the login was incorrect
                _isLoggingIn = true;
                var username = Convert.ToBase64String(Encrypt(NewUsername.Text));
                var password = Convert.ToBase64String(Encrypt(NewPassword.Password));
                var displayName = NewDisplayName.Text;
                Login(username, password, true, displayName);
            }

        }

        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isLoggingIn)
            {
                return;
            }
            // to prevent multiple logins we must block logins, the call to allow more logins is after the server sends back and says that 
            // the login was incorrect
            _isLoggingIn = true;
            var username = Convert.ToBase64String(Encrypt(usernameInput.Text));
            var password = Convert.ToBase64String(Encrypt(passwordInput.Password));

            Login(username, password, false);
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
                SelectedCollectionTitle.Text = _selectedCollection.Title;
                CreatorText.Text = SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary.ContainsKey(_selectedCollection.Creator) ? SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[_selectedCollection.Creator] : "...";
                LastEditedText.Text = _selectedCollection.LastEditedTimestamp;
                CreateDateText.Text = _selectedCollection.Timestamp;

                //set tags in window if it has any
                if (_selectedCollection.Keywords != null)
                {
                    TagsText.Text = "";
                    foreach (var tag in _selectedCollection.Keywords)
                    {
                        if (TagsText.Text == "")
                        {
                            TagsText.Text = TagsText.Text + tag.Text;
                        }
                        else
                        {
                            TagsText.Text = TagsText.Text + ", " + tag.Text;
                        }
                        
                    }
                }
                else
                {
                    TagsText.Text = "None";
                }

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


        public static async Task<Tuple<bool,string>> AttemptLogin(string username, string password, string displayname, bool createNewUser)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            var cred = new Dictionary<string, string>();

            //cred["user"] = Convert.ToBase64String(Encrypt(usernameInput.Text));


            cred["user"] = username;
            cred["pass"] = password;

            UserName = username;
            if (createNewUser)
            {
                cred["display_name"] = displayname ?? "MIRANDA PUT THE DISLPAY NAME HEEERRE";
                cred["new_user"] = "";
            }
            var url = (NusysConstants.TEST_LOCAL_BOOLEAN ? "http://" : "https://") + ServerName + "/api/nusyslogin/";
            var client = new HttpClient(
             new HttpClientHandler
             {
                 ClientCertificateOptions = ClientCertificateOption.Automatic
             });

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
                    HashedPass = cred["pass"];
                    UserID = userID;
                }
                serverSessionId = dict.ContainsKey("server_session_id") ? dict["server_session_id"] : "";
                if (!validCredentials && dict.ContainsKey("error_message"))
                {

                    return new Tuple<bool, string>(false, dict["error_message"]);
                }
            }
            catch (Exception boolParsException)
            {
                Debug.WriteLine("error parsing bool and serverSessionId returned from server");
                validCredentials = false;
                serverSessionId = null;
                return new Tuple<bool, string>(false, "error parsing bool and serverSessionId returned from server");
            }
            ServerSessionID = serverSessionId;
            return new Tuple<bool, string>(true, null);
        }

        private async void Login(string username, string password, bool createNewUser, string displayname = null)
        {
            try
            {
                var tuple = await AttemptLogin(username, password, displayname, createNewUser);

                if (tuple.Item1)
                {
                    try
                    {
                        await SessionController.Instance.NuSysNetworkSession.Init();
                        SessionController.Instance.LocalUserID = UserID;

                        loggedInText.Text = "Logged In!";
                        NewUserLoginText.Text = "Logged In!";
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
                        if (createNewUser)
                        {
                            SlideOutNewUser.Begin();
                            SlideInWorkspace.Begin();
                        }
                        else
                        {
                            SlideOutLogin.Begin();
                            SlideInWorkspace.Begin();
                        }

                        login.Visibility = Visibility.Collapsed;
                        NewUser.Visibility = Visibility.Collapsed;
                        NuSysTitle.Visibility = Visibility.Collapsed;


                        if (UserID.ToLower() != "rosemary" && UserID.ToLower() != "rms" &&
                            UserID.ToLower() != "gfxadmin")
                        {

                            foreach (var box in List.Items)
                            {
                                if ((box as CollectionListBox).MadeByRosemary)
                                {
                                    List.Items.Remove(box);
                                }
                            }
                        }

                        //add active users to list of users in corner


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
                                    Debug.WriteLine(
                                        " this shouldn't ever happen.  trent was too lazy to do error hadnling");
                                }
                            }

                            SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser += NewNetworkUser;
                            SessionController.Instance.NuSysNetworkSession.OnNetworkUserDropped += DropNetworkUser;

                            foreach (var user in SessionController.Instance.NuSysNetworkSession.NetworkMembers.Values)
                            {
                                NewNetworkUser(user);
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
                            }
                        });
                    }
                    catch (ServerClient.IncomingDataReaderException loginException)
                    {
                        loggedInText.Text = "Log in failed!";
                        NewUserLoginText.Text = "Log in failed!";
                        //     throw new Exception("Your account is probably already logged in");
                    }
                }
                else
                {
                    loggedInText.Text = tuple.Item2;
                    NewUserLoginText.Text = tuple.Item2;
                    //We stop blocking the login becausethere is an error in logging in
                    _isLoggingIn = false;

                }

            }
            catch (HttpRequestException h)
            {
                Debug.WriteLine("cannot connect to server");
            }

        }

        /// <summary>
        /// event handler for when the nusysNetworkSession drop a network user.  
        /// </summary>
        /// <param name="userId"></param>
        private void DropNetworkUser(string userId)
        {
            UITask.Run(delegate
            {
                foreach (var child in Users.Children)
                {
                    var label = child as UserLabel;
                    Debug.Assert(label != null);
                    if (label.UserId == userId)
                    {
                        Users.Children.Remove(child);
                        break;
                    }
                }
            });
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
            var sortTypeStr = ((Button)sender).Name;
            switch (sortTypeStr)
            {
                case "TitleHeader":
                    _currentSortType = _currentSortType == SortType.TitleAsc ? SortType.TitleDesc : SortType.TitleAsc;
                    break;
                case "AccessHeader":
                    _currentSortType = _currentSortType == SortType.AccessAsc ? SortType.AccessDesc : SortType.AccessAsc;
                    break;
                case "DateHeader":
                    _currentSortType = _currentSortType == SortType.DateAsc ? SortType.DateDesc : SortType.DateAsc;
                    break;
            }
            ApplySorting(_currentSortType);
        }

        private async void MyWorkspacesButton_OnClick(object sender, RoutedEventArgs e)
        {
            await ApplyFilter(FilterType.Mine);
        }

        private async void OtherWorkspacesButton_OnClick(object sender, RoutedEventArgs e)
        {
            await ApplyFilter(FilterType.Others);
        }
        private async void AllWorkspacesButton_OnClick(object sender, RoutedEventArgs e)
        {
            await ApplyFilter(FilterType.All);
        }

        public void SetSelectedCollection(LibraryElementModel m)
        {
            _selectedCollection = m;
        }

        /// <summary>
        /// event handler called whenever the recently used button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecentlyUsedButton_Onclick(object sender, RoutedEventArgs e)
        {
           ApplyFilter(FilterType.RecentlyUsed);
        }

        private void ApplySorting(SortType sorttype)
        {
            _currentSortType = sorttype;
            var collections = List?.Items.ToList();
            List?.Items?.Clear();

            switch (sorttype)
            {
                case SortType.TitleAsc:
                case SortType.TitleDesc:
                    //sort by title string comparison and reverse if necessary, and make sure you set the reverse bool again

                    if (sorttype == SortType.TitleAsc)
                    {
                        collections.Sort(
                            (a, b) => (a as CollectionListBox).Title.CompareTo((b as CollectionListBox).Title));
                    }
                    else
                    {
                        collections.Sort((b, a) => (a as CollectionListBox).Title.CompareTo((b as CollectionListBox).Title));
                    }
                    break;
                case SortType.AccessAsc:
                case SortType.AccessDesc:

                    if (sorttype == SortType.AccessAsc)
                    {
                        collections.Sort(
                            (a, b) => (a as CollectionListBox).Access.CompareTo((b as CollectionListBox).Access));
                    }
                    else
                    {
                        collections.Sort((b, a) => (a as CollectionListBox).Access.CompareTo((b as CollectionListBox).Access));
                    }

                    break;
                case SortType.DateAsc:
                case SortType.DateDesc:
                    if (sorttype == SortType.DateAsc)
                    {
                        collections.Sort(
                            (a, b) => (a as CollectionListBox).Date.CompareTo((b as CollectionListBox).Date));
                    }
                    else
                    {
                        collections.Sort((b, a) => (a as CollectionListBox).Date.CompareTo((b as CollectionListBox).Date));
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

        private async Task ApplyFilter(FilterType type)
        {
            _currentFilterType = type;
            switch (type)
            {
                case FilterType.RecentlyUsed:
                    await Task.Run(async delegate
                    {
                        var request = new GetLastUsedCollectionsRequest(new GetLastUsedCollectionsServerRequestArgs() { UserId = UserID });
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

                        Debug.Assert(request.WasSuccessful() == true);

                        var idHashSet = new HashSet<string>(request.GetReturnedModels().Select(model => model.CollectionId));

                        await UITask.Run(async delegate
                        {
                            foreach (var item in List.Items.ToList())
                            {
                                var box = item as CollectionListBox;
                                if (box == null)
                                {
                                    continue;
                                }
                                if (!idHashSet.Contains(box.ID))
                                {
                                    List.Items.Remove(box);
                                }
                            }
                        });
                    });
                    break;
                    case FilterType.All:
                    var othercollections = new List<CollectionListBox>();
                    foreach (var i in _collectionList)
                    {
                        var listbox = new CollectionListBox(i, this);
                        othercollections.Add(listbox);
                    }
                    //set items in collectionlist alphabetically
                    List?.Items?.Clear();
                    othercollections.Sort((a, b) => a.Title.CompareTo(b.Title));
                    foreach (var i in othercollections)
                    {
                        List?.Items?.Add(i);
                    }
                    _collectionAdded = true;
                    //next time title is clicked, it will reverse the list

                    break;

                    case FilterType.Others:
                        var othercollections1 = new List<CollectionListBox>();
                        foreach (var i in _collectionList)
                        {
                            if (i.Creator != UserID)
                            {
                                var listbox = new CollectionListBox(i, this);
                                othercollections1.Add(listbox);
                            }
                        }
                        //set items in collectionlist alphabetically
                        List?.Items?.Clear();
                        othercollections1.Sort((a, b) => a.Title.CompareTo(b.Title));
                        foreach (var i in othercollections1)
                        {
                            List?.Items?.Add(i);
                        }
                        _collectionAdded = true;
                        //next time title is clicked, it will reverse the list
           
                    break;
                    case FilterType.Mine:
                    var mycollections = new List<CollectionListBox>();
                    foreach (var i in _collectionList)
                    {
                        if (i.Creator == UserID)
                        {
                            var listbox = new CollectionListBox(i, this);
                            mycollections.Add(listbox);
                        }
                    }
                    //set items in collectionlist alphabetically
                    List?.Items?.Clear();
                    mycollections.Sort((a, b) => a.Title.CompareTo(b.Title));
                    foreach (var i in mycollections)
                    {
                        List?.Items?.Add(i);
                    }
                    _collectionAdded = true;
                    //next time title is clicked, it will reverse the list
       
                    break;
            }   
        }
    }
}
