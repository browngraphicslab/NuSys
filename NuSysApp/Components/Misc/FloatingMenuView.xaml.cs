using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using SharpDX.Direct2D1;
using Image = Windows.UI.Xaml.Controls.Image;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public enum Options
    {
        Idle,
        MainSelect,
            SelectNode,
            SelectMarquee,
        MainSearch,
        MainPen,
            PenGlobalInk,
            PenErase,
            PenHighlight,
        MainAdd,
            AddTextNode,
            AddInkNode,
            AddMedia,
            AddWeb,
            AddAudioCapture,
            AddRecord,
            AddBucket,
            AddVideo,
        MainSaveLoad,
            Load,
            Save,
        MainMisc,
            MiscLoad,
            MiscSave,
            MiscPin,
            MiscUsers
    }

    public sealed partial class FloatingMenuView : UserControl
    {
        public event OnModeChangeHandler ModeChange;
        public delegate void OnModeChangeHandler(Options mode, bool isFixed);
        private Dictionary<Tuple<FloatingMenuButtonView, int>, Tuple<Storyboard, string>> _storyboards;
        private FreeFormViewer _freeFormViewer;
        private FrameworkElement _dragItem;
        private ElementType _elementType;

        /// <summary>
        /// Maps all buttons to its corresponding enum entry.
        /// </summary>
        private readonly BiDictionary<FloatingMenuButtonView, Options> _buttons;

        
        /// <summary>
        /// Holds a reference to the currently active button
        /// </summary>
        private FloatingMenuButtonView _activeButton;

        /// <summary>
        /// Maps each main menu button to its current active submenu button
        /// </summary>
        private Dictionary<FloatingMenuButtonView, FloatingMenuButtonView> _activeSubMenuButtons;

        public FloatingMenuView()
        {
            DataContext = new FloatingMenuViewModel();
            this.InitializeComponent();

            _buttons = new BiDictionary<FloatingMenuButtonView, Options>();
            _buttons[btnSelectNode] = Options.SelectNode;
            //_buttons[btnMarqueeSelect] = Options.SelectMarquee;

 
            //  _buttons[btnHighlight] = Options.PenHighlight;    



            _buttons[btnAddNode] = Options.MainAdd;
            _buttons[btnNewNode] = Options.AddTextNode;
            _buttons[btnLibrary] = Options.AddMedia;

            

            _buttons[btnExport] = Options.Save;

   


            pinWindow.setFloatingMenu(this);
            bucketWindow.setFloatingMenu(this);
            userWindow.setFloatingMenu(this);

            _storyboards = new Dictionary<Tuple<FloatingMenuButtonView, int>, Tuple<Storyboard, string>>();

           // _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnPen, 0), new Tuple<Storyboard, string>(slidein, "SubMenuPen"));
           // _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnPen, 1), new Tuple<Storyboard, string>(slideout, "SubMenuPen"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnAddNode, 0), new Tuple<Storyboard, string>(slidein, "SubMenuNodes"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnAddNode, 1), new Tuple<Storyboard, string>(slideout, "SubMenuNodes"));



            _activeSubMenuButtons = new Dictionary<FloatingMenuButtonView, FloatingMenuButtonView>();
            _activeSubMenuButtons[btnAddNode] = btnNewNode;



            // Register tap listeners
            foreach (var btn in _buttons.Keys)
            {
                btn.IsRightTapEnabled = true;
                btn.RightTapped += OnBtnRightTapped;
                btn.Tapped += OnBtnTapped;
                
            }

            Loaded += delegate
            {                // Set Selection state as active on start-up
                _activeButton = btnSelectNode;
                SetActive(Options.SelectNode);
            };     

            pinWindow.DataContext = new PinWindowViewModel();


            btnLibrary.ManipulationMode = ManipulationModes.All;
            btnAddNode.ManipulationMode = ManipulationModes.All;

            btnAddNode.ManipulationStarting += BtnAddNodeOnManipulationStarting;
            btnAddNode.ManipulationStarted += BtnAddNodeOnManipulationStarted;
            btnAddNode.ManipulationDelta += BtnAddNodeOnManipulationDelta;
            btnAddNode.ManipulationCompleted += BtnAddNodeOnManipulationCompleted;

            btnLibrary.ManipulationStarting += BtnAddNodeOnManipulationStarting;
            btnLibrary.ManipulationStarted += BtnAddNodeOnManipulationStarted;
            btnLibrary.ManipulationDelta += BtnAddNodeOnManipulationDelta;
            btnLibrary.ManipulationCompleted += BtnAddNodeOnManipulationCompleted;

            var lib =  new LibraryView(new LibraryBucketViewModel(), new LibraryElementPropertiesWindow());
            Canvas.SetLeft(lib, 100);
            Canvas.SetTop(lib, 100);
            xWrapper.Children.Add(lib);
        }

        private async void BtnAddNodeOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs args)
        {
            xWrapper.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(args.Position.X, args.Position.Y, 300, 300));
            await AddNode(new Point(r.X, r.Y), new Size(r.Width, r.Height), _elementType);

        }

        private void BtnAddNodeOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX += args.Delta.Translation.X;
            t.TranslateY += args.Delta.Translation.Y;
        }

        private void BtnAddNodeOnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            _dragItem.Opacity = 0.5;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX += args.Position.X - _dragItem.ActualWidth / 2;
            t.TranslateY += args.Position.Y - _dragItem.ActualHeight / 2;
        }

        private async void BtnAddNodeOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs args)
        {
            _elementType = sender == btnAddNode ? ElementType.Text : ElementType.Image;

            args.Container = xWrapper;
            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            var img = new Image();
            img.Opacity = 0;
            var t = new CompositeTransform();

            img.RenderTransform = new CompositeTransform();
            img.Source = bmp;
            _dragItem = img;

            xWrapper.Children.Add(_dragItem);
        }

        private async Task AddNode(Point pos, Size size, ElementType elementType, object data = null)
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;
            var p = pos;

            var dict = new Message();
            Dictionary<string, object> metadata;
            if (elementType == ElementType.Document || elementType == ElementType.Word || elementType == ElementType.Powerpoint || elementType == ElementType.Image || elementType == ElementType.PDF || elementType == ElementType.Video)
            {
                var storageFile = await FileManager.PromptUserForFile(Constants.AllFileTypes);
                if (storageFile == null) return;

                var fileType = storageFile.FileType.ToLower();
                dict["title"] = storageFile.DisplayName;


                var token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);

                try
                {
                    //       CheckFileType(fileType); TODO readd
                }
                catch (Exception e)
                {
                    Debug.WriteLine("The file format you selected is currently unsupported");
                    return;
                }

                if (Constants.ImageFileTypes.Contains(fileType))
                {
                    elementType = ElementType.Image;

                    data = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(storageFile));
                }

                if (Constants.WordFileTypes.Contains(fileType))
                {
                    metadata = new Dictionary<string, object>();
                    metadata["FilePath"] = storageFile.Path;
                    metadata["Token"] = token.Trim();

                    dict["metadata"] = metadata;

                    elementType = ElementType.Word;

                    //data = File.ReadAllBytes(storageFile.Path);
                }

                if (Constants.PowerpointFileTypes.Contains(fileType))
                {
                    metadata = new Dictionary<string, object>();
                    metadata["FilePath"] = storageFile.Path;
                    metadata["Token"] = token.Trim();

                    dict["metadata"] = metadata;

                    elementType = ElementType.Powerpoint;

                    //data = File.ReadAllBytes(storageFile.Path);
                }

                if (Constants.PdfFileTypes.Contains(fileType))
                {
                    elementType = ElementType.PDF;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    data = Convert.ToBase64String(fileBytes);
                }
                if (Constants.VideoFileTypes.Contains(fileType))
                {
                    elementType = ElementType.Video;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    data = Convert.ToBase64String(fileBytes);
                }
                if (Constants.AudioFileTypes.Contains(fileType))
                {
                    elementType = ElementType.Audio;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    data = Convert.ToBase64String(fileBytes);
                }
            }
            var contentId = SessionController.Instance.GenerateId();

            metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;
            metadata["node_type"] = elementType + "Node";

            dict = new Message();
            dict["width"] = size.Width.ToString();
            dict["height"] = size.Height.ToString();
            dict["nodeType"] = elementType.ToString();
            dict["x"] = p.X;
            dict["y"] = p.Y;
            dict["contentId"] = contentId;
            dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
            dict["metadata"] = metadata;
            dict["autoCreate"] = true;

            var request = new NewElementRequest(dict);
            
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, data == null ? "" : data.ToString(), elementType, dict.ContainsKey("title") ? dict["title"].ToString() : null));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            //await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(new NewContentSystemRequest(contentId, data == null ? "" : data.ToString()), NetworkClient.PacketType.TCP, null, true);

            // TOOD: refresh library

            vm.ClearSelection();
            //   vm.ClearMultiSelection();
        }


        /// <summary>
        /// Allows an option to be selected by passing in an option, rather than having to click a button.
        /// </summary>
        /// <param name="option">The option to switch to.</param>
        /// <param name="isFixed">Whether or not the menu should stay in this mode,</param>
        public void SetActive(Options option, bool isFixed = false)
        {
            SetActive(_buttons.GetKeyByValue(option), option, isFixed);
        }

        public void Reset()
        {
            btnAddNode.Icon = btnNewNode.Icon;
            SetActive(Options.SelectNode);
        }

        public void SetActive(FloatingMenuButtonView btnToActivate, Options option, bool isFixed = false)
        {
            // Close everything that's open
            CloseAllSubMenus();

            // Try to find a corresponding storyboard to play
            Tuple<Storyboard, string> val;
            var hasAnim = _storyboards.TryGetValue(new Tuple<FloatingMenuButtonView, int>(btnToActivate, 1), out val);
            if (hasAnim) {
                val.Item1.Stop();
                Storyboard.SetTargetName(val.Item1, val.Item2);
                val.Item1.Begin();
            }

            // Dectivate currently active buttons
            _activeButton.Active = false;
            if (_activeButton.ParentButton != null)
            {
                // Also deactive its parent button if exists
                _activeButton.ParentButton.Active = false;
            }

            // Activate tapped button
            if (btnToActivate.ParentButton != null)
            {
                // A submenu button was tapped
                btnToActivate.Active = btnToActivate.IsMode;
                btnToActivate.ParentButton.Active = true;
                _activeSubMenuButtons[btnToActivate.ParentButton].Active = false;
                _activeSubMenuButtons[btnToActivate.ParentButton] = btnToActivate;
            } else
            {
                // A mainmenu button was clicked
                if (_activeSubMenuButtons.ContainsKey(btnToActivate))
                {
                    var activeSubMenuButton = _activeSubMenuButtons[btnToActivate];
                    activeSubMenuButton.Active = activeSubMenuButton.IsMode;
                    btnToActivate.Active = true;

                    // activate previsouly selected submenu mode
                    if (activeSubMenuButton.IsMode)
                        option = _buttons[activeSubMenuButton];
                }
                else
                {
                    option = _buttons[btnToActivate];
                }

            }

            _activeButton = btnToActivate;

            // Let all listeners know.
            ModeChange?.Invoke(option, isFixed);
        }

        public void CloseAllSubMenus()
        {
           
        }
        /*
        private void ExpandTapped(object sender, TappedRoutedEventArgs e)
        {
            CloseAllSubMenus();
        
            if (ExpandImage.Visibility == Visibility.Visible)
            {
                // expand menu
                bucketWindow.IsHitTestVisible = false;
                pinWindow.IsHitTestVisible = false;
                expand.Begin();
                CollapseImage.Visibility = Visibility.Visible;
                ExpandImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                // collpase menu
                bucketWindow.IsHitTestVisible = false;
                pinWindow.IsHitTestVisible = false;
                collapse.Begin();
                CollapseImage.Visibility = Visibility.Collapsed;
                ExpandImage.Visibility = Visibility.Visible;
            }
           // e.Handled = true;
        }
        */
        private void OnBtnTapped(object sender, TappedRoutedEventArgs e)
        {
            var btn = (FloatingMenuButtonView)sender;
            SetActive(btn, _buttons[btn], false);
            //e.Handled = true;
        }

        private void OnBtnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var btn = (FloatingMenuButtonView)sender;
            SetActive(btn, _buttons[btn], true);
           // e.Handled = true;
        }    

        public SessionView SessionView
        {
            get;set;
        }
    }
}