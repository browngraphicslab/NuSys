using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public enum Options
    {
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
        private WorkspaceView _workspaceView;

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
            _buttons[btnSelect] = Options.MainSelect;
            _buttons[btnSelectNode] = Options.SelectNode;
            //_buttons[btnMarqueeSelect] = Options.SelectMarquee;

            _buttons[btnPen] = Options.PenGlobalInk;
            _buttons[btnGlobalInk] = Options.PenGlobalInk;
            _buttons[btnInkErase] = Options.PenErase;
            //  _buttons[btnHighlight] = Options.PenHighlight;    

            _buttons[btnSearch] = Options.MainSearch;

            _buttons[btnAdd] = Options.MainAdd;
            _buttons[btnNewNode] = Options.AddTextNode;
            _buttons[btnNewMedia] = Options.AddMedia;
            _buttons[btnNewAudioCapture] = Options.AddAudioCapture;
            _buttons[btnNewRecordNode] = Options.AddRecord;
            _buttons[btnNewWebNode] = Options.AddWeb;
            _buttons[btnBucket] = Options.AddBucket;
            _buttons[btnVideo] = Options.AddVideo;
            
            _buttons[btnSaveLoad] = Options.MainSaveLoad;
            _buttons[btnLoad] = Options.Load;
            _buttons[btnExport] = Options.Save;
            _buttons[btnPin] = Options.MiscPin;
            _buttons[btnMisc] = Options.MainMisc;
            _buttons[btnUsers] = Options.MiscUsers;

            pinWindow.setFloatingMenu(this);
            bucketWindow.setFloatingMenu(this);
            userWindow.setFloatingMenu(this);

            _storyboards = new Dictionary<Tuple<FloatingMenuButtonView, int>, Tuple<Storyboard, string>>();
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnSelect, 0), new Tuple<Storyboard, string>(slidein, "SubMenuSelect"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnSelect, 1), new Tuple<Storyboard, string>(slideout, "SubMenuSelect"));
           // _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnPen, 0), new Tuple<Storyboard, string>(slidein, "SubMenuPen"));
           // _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnPen, 1), new Tuple<Storyboard, string>(slideout, "SubMenuPen"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnAdd, 0), new Tuple<Storyboard, string>(slidein, "SubMenuNodes"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnAdd, 1), new Tuple<Storyboard, string>(slideout, "SubMenuNodes"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnSaveLoad, 0), new Tuple<Storyboard, string>(slidein, "SubMenuSaveLoad"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnSaveLoad, 1), new Tuple<Storyboard, string>(slideout, "SubMenuSaveLoad"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnMisc, 0), new Tuple<Storyboard, string>(slidein, "SubMenuAdditional"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnMisc, 1), new Tuple<Storyboard, string>(slideout, "SubMenuAdditional"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnPin, 0), new Tuple<Storyboard, string>(windowClose, "pinWindow"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnPin, 1), new Tuple<Storyboard, string>(windowOpen, "pinWindow"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnBucket, 0), new Tuple<Storyboard, string>(windowClose, "bucketWindow"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnBucket, 1), new Tuple<Storyboard, string>(windowOpen, "bucketWindow"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnUsers, 0), new Tuple<Storyboard, string>(windowClose, "userWindow"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnUsers, 1), new Tuple<Storyboard, string>(windowOpen, "userWindow"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnSearch, 0), new Tuple<Storyboard, string>(windowClose, "searchWindow"));
            _storyboards.Add(new Tuple<FloatingMenuButtonView, int>(btnSearch, 1), new Tuple<Storyboard, string>(windowOpen, "searchWindow"));


            _activeSubMenuButtons = new Dictionary<FloatingMenuButtonView, FloatingMenuButtonView>();
            _activeSubMenuButtons[btnSelect] = btnSelectNode;
            _activeSubMenuButtons[btnPen] = btnGlobalInk;
            _activeSubMenuButtons[btnAdd] = btnNewNode;
            _activeSubMenuButtons[btnSaveLoad] = btnSaveLoad;


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
            btnAdd.Icon = btnNewNode.Icon;
            btnSelect.Icon = btnSelectNode.Icon;
            btnSaveLoad.Icon = btnExport.Icon;
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
            foreach (var key in _storyboards.Keys)
            {
                if (key.Item2 == 0) {
                    var t = _storyboards[key];
                    t.Item1.Stop();
                    Storyboard.SetTargetName(t.Item1, t.Item2);
                    t.Item1.Begin();
                }
            }
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