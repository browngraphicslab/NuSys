using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.UI;

namespace NuSysApp
{
    /// <summary>
    /// UIElement to manage a set of TabPageUIElements as a list of tabs
    /// </summary>
    class TabControlUIElement : RectangleUIElement
    {
        /// <summary>
        /// Extension of button class that keeps track of what TabPage and TabControl it corresponds to.
        /// Used for the buttons that allow changing tabs.
        /// </summary>
        protected class TabButtonUIElement : ButtonUIElement
        {
            /// <summary>
            /// The TabPage that this button corresponds to 
            /// </summary>
            public TabPageUIElement TabPage;
            /// <summary>
            /// The TabControl that this button is in
            /// </summary>
            public TabControlUIElement TabControl;

            /// <summary>
            /// Whether or not this tab is selected Selected property
            /// </summary>
            public bool Selected {
                get; set;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="resourceCreator"></param>
            /// <param name="tabControl">The TabControl this button is in</param>
            /// <param name="tabPage">The TabPage this button corresponds to</param>
            /// <param name="shape"></param>
            public TabButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, TabControlUIElement tabControl, TabPageUIElement tabPage, BaseInteractiveUIElement shape = null) : base(parent, resourceCreator, shape)
            {
                TabPage = tabPage;
                TabControl = tabControl;
                Selected = false;
            }

            public override void Draw(CanvasDrawingSession ds)
            {
                SelectedBackground = TabControl.TabSelectedColor;
                Background = Selected ? TabPage.Background : TabControl.TabColor;
                ButtonText = TabPage.Name;
                base.Draw(ds);
            }
        }

        /// <summary>
        /// List of tabs in the TabControl
        /// </summary>
        private List<TabPageUIElement> _tabs = new List<TabPageUIElement>();
        /// <summary>
        /// List of tabs whose buttons are shown in the main bar
        /// </summary>
        private List<TabPageUIElement> _shownTabs = new List<TabPageUIElement>(); 
        /// <summary>
        /// ListView containing the tabs that don't fit in the top bar and are kept in the list on the side.
        /// </summary>
        private ListViewUIElementContainer<TabPageUIElement> _overflowList;

        private bool _needsShownUpdate = true;

        private ButtonUIElement _overflowButton;
        private float _overflowListWidth = 200;

        /// <summary>
        /// Map from each TabPage to the button it corresponds to
        /// </summary>
        private Dictionary<TabPageUIElement, TabButtonUIElement> _tabDict = new Dictionary<TabPageUIElement, TabButtonUIElement>();
        /// <summary>
        /// List of buttons in the TabControl
        /// </summary>
        private List<TabButtonUIElement> _tabButtons = new List<TabButtonUIElement>();

        /// <summary>
        /// LayoutManager to lay out the tab buttons
        /// </summary>
        private StackLayoutManager _buttonLayoutManager = new StackLayoutManager();

        /// <summary>
        /// Helper variable for the SelectedTab property
        /// </summary>
        private TabPageUIElement _selectedTab = null;
        /// <summary>
        /// The tab that is currently selected, if null then no tab is selected
        /// </summary>
        public TabPageUIElement SelectedTab
        {
            get
            {
                return _selectedTab;
            }
            set
            {
                if (_selectedTab != null)
                {
                    _selectedTab.IsVisible = false;
                    _tabDict[_selectedTab].Selected = false;
                }
                _selectedTab = value;
                if (_selectedTab != null)
                {
                    _selectedTab.IsVisible = true;
                    _tabDict[_selectedTab].Selected = true;
                }
            }
        }
        
        /// <summary>
        /// The index of the tab that is currently selected
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return SelectedTab == null ? -1 : _tabs.IndexOf(SelectedTab);
            }
            set
            {
                SelectedTab = _tabs[value];
            }
        }

        public override float Width
        {
            get { return base.Width; }
            set
            {
                if(value != base.Width)
                {
                    MarkDirty();
                }
                base.Width = value;
            }
        }

        /// <summary>
        /// The height of each tab button
        /// </summary>
        public int TabHeight
        {
            get; set;
        } = 50;

        private int _tabWidth = 100;

        /// <summary>
        /// The width of each tab button
        /// </summary>
        public int TabWidth
        {
            get { return _tabWidth; }
            set
            {
                if(_tabWidth != value)
                {
                    MarkDirty();
                }
                _tabWidth = value;
            }
        }

        /// <summary>
        /// The color of each tab button when it is not selected.
        /// When it is selected it is the color of the page it corresponds to.
        /// </summary>
        public Color TabColor
        {
            get; set;
        } = Colors.MediumBlue;

        /// <summary>
        /// The color of each tab button when it is being selected.
        /// </summary>
        public Color TabSelectedColor
        {
            get; set;
        } = Colors.Blue;

        /// <summary>
        /// How to align the tab buttons
        /// </summary>
        public HorizontalAlignment TabAlignment
        {
            get; set;
        } = HorizontalAlignment.Left;


        private int _tabSpacing = 0;
        /// <summary>
        /// The spacing in between each tab button
        /// </summary>
        public int TabSpacing
        {
            get { return _tabSpacing; }
            set
            {
                if(_tabSpacing != value)
                {
                    MarkDirty();
                }
                _tabSpacing = value;
            }
        }

        /// <summary>
        /// Default constructor for TabControl
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public TabControlUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _overflowList = new ListViewUIElementContainer<TabPageUIElement>(this, Canvas)
            {
                ShowHeader = false,
                IsVisible = false,
                Height = 500
            };
            _overflowList.RowTapped += _overflowList_RowTapped;
            AddChild(_overflowList);
            var column = new ListTextColumn<TabPageUIElement>
            {
                ColumnFunction = page => page.Name,
                RelativeWidth = 1f
            };
            _overflowList.AddColumn(column);

            _overflowButton = new NuSysApp.ButtonUIElement(this, Canvas) {ButtonText = "+"};
            _overflowButton.Tapped += _overflowButton_Tapped;
            _overflowList.Width = _overflowListWidth;
            AddChild(_overflowButton);
        }

        private void _overflowList_RowTapped(TabPageUIElement item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            SelectedTab = item;
            MoveTab(_tabs.IndexOf(item), 0);
            _overflowList.IsVisible = false;
        }

        private void _overflowButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_overflowList.GetItems().Count > 0)
            {
                _overflowList.IsVisible = !_overflowList.IsVisible;
            }
        }

        /// <summary>
        /// Array like access notation to access different tabs in the TabControl
        /// </summary>
        /// <param name="index">The index of the tab to access</param>
        /// <returns></returns>
        public TabPageUIElement this[int index]
        {
            get
            {
                if (index < 0 || index >= _tabs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return _tabs[index];
            }
            set
            {
                if(index < 0 || index >= _tabs.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                _tabs[index] = value;
            }
        }

        /// <summary>
        /// Dictionary like access notation to access different tabs in the TabControl
        /// </summary>
        /// <param name="name">The name of the tab to access</param>
        /// <returns></returns>
        public TabPageUIElement this[string name] => GetTabWithName(name);

        /// <summary>
        /// Returns the tab with the given name, or null if it doesn't exist
        /// </summary>
        /// <param name="name">The name of the tab to get</param>
        /// <returns></returns>
        public TabPageUIElement GetTabWithName(string name)
        {
            return _tabs.Find((TabPageUIElement t) => t.Name.Equals(name));
        }

        /// <summary>
        /// Returns the index of the tab with the given name, or -1 if it doesn't exist
        /// </summary>
        /// <param name="name">The name of the tab to find the index of</param>
        /// <returns></returns>
        public int GetTabIndexWithName(string name)
        {
            return _tabs.FindIndex((TabPageUIElement t) => t.Name.Equals(name));
        }

        /// <summary>
        /// Add the given tab to the TabControl
        /// </summary>
        /// <param name="newTab">The TabPage to add to the TabControl</param>
        /// <param name="select">If true, set the given tab to the selected tab as well as adding it;
        /// otherwise just add the tab</param>
        public void AddTab(TabPageUIElement newTab, bool select = false)
        {
            newTab.Parent = this;
            _tabs.Add(newTab);
            
            var button = new TabButtonUIElement(this, ResourceCreator, this, newTab);
            _tabButtons.Add(button);
            AddChild(button);
            button.Tapped += TabButton_Tapped;
            _tabDict.Add(newTab, button);
            AddChild(newTab);
            newTab.IsVisible = false;
            if (select || SelectedTab == null)
            {
                SelectedTab = newTab;
            }
            if (_shownTabs.Count*TabWidth > Width)
            {
                _overflowList.AddItem(newTab);
            }
            else
            {
                _shownTabs.Add(newTab);
            }
            _overflowList.AddItem(newTab);
            SendToFront(_overflowList);
            MarkDirty();
        }

        /// <summary>
        /// Event handler for tab buttons being tapped in order to change the selected tab
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void TabButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var button = item as TabButtonUIElement;
            if(button == null)
            {
                return;
            }
            SelectedTab = button.TabPage;
        }

        /// <summary>
        /// Remove given tab if it is contained in the TabControl
        /// </summary>
        /// <param name="tab">Tab to remove</param>
        public void RemoveTab(TabPageUIElement tab)
        {
            var index = _tabs.IndexOf(tab);
            if(index == -1)
            {
                throw new ArgumentException("TabControl does not contain the given tab", nameof(tab));
            }
            RemoveTabAt(index);
        }

        /// <summary>
        /// Remove the tab at the given index
        /// </summary>
        /// <param name="index">Index of tab to be removed</param>
        public void RemoveTabAt(int index)
        {
            if(index < 0 || index >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(index));
            }
            if (SelectedIndex == index)
            {
                DeselectTabAt(index);
                if (SelectedIndex == index)
                {
                    SelectedTab = null;
                }
            }
            var t = _tabs[index];
            _overflowList.RemoveItem(t);
            if(_overflowList.GetItems().Count == 0)
            {
                _overflowList.IsVisible = false;
            }
            var button = _tabDict[t];
            _tabButtons.Remove(button);
            RemoveChild(button);
            _buttonLayoutManager.Remove(button);
            button.Tapped -= TabButton_Tapped;
            RemoveChild(_tabs[index]);
            _tabs.RemoveAt(index);
            _tabDict.Remove(t);
            MarkDirty();
        }

        /// <summary>
        /// Remove tab with given name if one exists in the TabControl
        /// </summary>
        /// <param name="name"></param>
        public void RemoveTabWithName(string name)
        {
            var t = GetTabWithName(name);
            if(t == null)
            {
                throw new ArgumentException("TabControl does not contain tab with given name", nameof(name));
            }
            RemoveTab(t);
        }

        /// <summary>
        /// Remove all tabs from the TabControl
        /// </summary>
        public void Clear()
        {
            _tabs.Clear();
            SelectedTab = null;
            MarkDirty();
        }

        /// <summary>
        /// Set the given tab to be selected. If tab is not contained in the TabControl, throws an exception
        /// </summary>
        /// <param name="tab">The tab to set to be the selected tab</param>
        public void SelectTab(TabPageUIElement tab)
        {
            if (_tabs.Contains(tab))
            {
                SelectedTab = tab;
            } else
            {
                throw new ArgumentException("TabControl does not contain given tab", nameof(tab));
            }
        }

        /// <summary>
        /// Sets the tab at index to be selected
        /// </summary>
        /// <param name="index">The index of the tab to set to be the selected tab</param>
        public void SelectTabAt(int index)
        {
            if(index < 0 || index >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(index));
            }
            SelectedIndex = index;
        }

        /// <summary>
        /// Sets the tab with the given name to be the selected tab.
        /// If no tab in the TabControl has the given name, does nothing
        /// </summary>
        /// <param name="name">The name of the tab to set to be the selected tab</param>
        public void SelectTabWithName(string name)
        {
            var tp = GetTabWithName(name);
            if(tp == null)
            {
                throw new ArgumentException("TabControl does not contain tab with given name", nameof(name));
            } else
            {
                SelectedTab = tp;
            }
        }

        /// <summary>
        /// If tab is the currently selected tab, deselect it by selecting the next tab
        /// </summary>
        /// <param name="tab">The tab to deselect</param>
        public void DeselectTab(TabPageUIElement tab)
        {
            if(!_tabs.Contains(tab))
            {
                throw new ArgumentException("TabControl does not contain given tab", nameof(tab));
            }
            if (SelectedTab == tab)
            {
                DeselectTabAt(_tabs.IndexOf(tab));
            }
        }

        /// <summary>
        /// If the tab at index is the currently selected tab, deselect it by selecting the next tab
        /// </summary>
        /// <param name="index">The index of the tab to deselect</param>
        public void DeselectTabAt(int index)
        {
            if (index < 0 || index >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(index));
            }
            if (SelectedIndex == index)
            {
                SelectedIndex = (index + 1) % _tabs.Count;
            }
        }

        /// <summary>
        /// If the tab with name name is the currently selected tab, deselect it by selecting the next tab
        /// </summary>
        /// <param name="name">The name of the tab to deselect</param>
        public void DeselectTabWithName(string name)
        {
            var index = GetTabIndexWithName(name);
            if(index == -1)
            {
                throw new ArgumentException("TabControl does not contain tab with given name", nameof(name));
            }
            DeselectTabAt(index);
        }

        /// <summary>
        /// Returns the local bounds of the tab at the given index
        /// </summary>
        /// <param name="index">The index of the tab to get the local bounds of</param>
        /// <returns>The local bounds of the tab at index</returns>
        public Rect GetTabLocalBounds(int index)
        {
            if (index < 0 || index >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(index));
            }
            return _tabs[index].GetLocalBounds();
        }

        /// <summary>
        /// Returns the screen bounds of the tab at the given index
        /// </summary>
        /// <param name="index">The index of the tab to get the screen bounds of</param>
        /// <returns>The screen bounds of the tab at index</returns>
        public Rect GetTabScreenBounds(int index)
        {
            if (index < 0 || index >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(index));
            }
            return _tabs[index].GetScreenBounds();
        }

        /// <summary>
        /// Swaps the location of the tabs at the given indices
        /// </summary>
        /// <param name="index1">The index of the first tab to swap</param>
        /// <param name="index2">The index of the second tab to swap</param>
        public void SwapTabs(int index1, int index2)
        {
            if(index1 < 0 || index1 >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(index1));
            }
            if(index2 < 0 || index2 >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(index2));
            }
            var t = _tabs[index1];
            _tabs[index1] = _tabs[index2];
            _tabs[index2] = t;
            var b = _tabButtons[index1];
            _tabButtons[index1] = _tabButtons[index2];
            _tabButtons[index2] = b;
            MarkDirty();
        }

        /// <summary>
        /// Insert the tab at index oldIndex at index newIndex
        /// </summary>
        /// <param name="oldIndex">The index of the tab to move</param>
        /// <param name="newIndex">The index to insert the moved tab at</param>
        public void MoveTab(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(oldIndex));
            }
            if (newIndex < 0 || newIndex >= _tabs.Count)
            {
                throw new ArgumentException("Given index is out of bounds", nameof(newIndex));
            }
            var t = _tabs[oldIndex];
            _tabs.RemoveAt(oldIndex);
            _tabs.Insert(newIndex, t);
            var b = _tabButtons[oldIndex];
            _tabButtons.RemoveAt(oldIndex);
            _tabButtons.Insert(newIndex, b);
            MarkDirty();
        }

        /// <summary>
        /// Update which tabs are in the shown tabs list and which are in the overflow tab list
        /// </summary>
        private void UpdateShownTabs()
        {
            _shownTabs.Clear();
            _overflowList.ClearItems();
            _buttonLayoutManager = new StackLayoutManager();
            bool hasOverflow = false;
            foreach (var tab in _tabs)
            {
                var c = _shownTabs.Count;
                if ((c + 1) * TabWidth + c * TabSpacing < Width - TabHeight - 5)
                {
                    _shownTabs.Add(tab);
                    var b = _tabDict[tab];
                    b.IsVisible = true;
                    _buttonLayoutManager.AddElement(b);
                }
                else
                {
                    _overflowList.AddItem(tab);
                    _tabDict[tab].IsVisible = false;
                    hasOverflow = true;
                }
            }
            _overflowButton.IsVisible = hasOverflow;
        }

        /// <summary>
        /// Mark that something that affects tabs and the overflow list has changed and we need to recalculate 
        /// which tabs are in the overflow list and which are in the main list
        /// </summary>
        private void MarkDirty()
        {
            _needsShownUpdate = true;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_needsShownUpdate)
            {
                _needsShownUpdate = false;
                UpdateShownTabs();
            }

            _buttonLayoutManager.ItemWidth = TabWidth;
            _buttonLayoutManager.ItemHeight = TabHeight;
            _buttonLayoutManager.Height = TabHeight;
            _buttonLayoutManager.Width = Width - TabHeight - 5;
            _buttonLayoutManager.HorizontalAlignment = TabAlignment;
            _buttonLayoutManager.Spacing = TabSpacing;
            _buttonLayoutManager.ArrangeItems();

            _overflowButton.Width = TabHeight;
            _overflowButton.Height = TabHeight;
            _overflowButton.Transform.LocalX = Width - TabHeight;
            _overflowButton.Background = TabColor;

            _overflowList.Transform.LocalX = Width - _overflowListWidth;
            _overflowList.Transform.LocalY = TabHeight;

            if (SelectedTab != null)
            {
                SelectedTab.Transform.LocalY = TabHeight;
                SelectedTab.Width = Width;
                SelectedTab.Height = Height - TabHeight;
            }

            base.Update(parentLocalToScreenTransform);
        }

        public override void Dispose()
        {
            foreach(var button in _tabButtons)
            {
                button.Tapped -= TabButton_Tapped;
            }
            _overflowButton.Tapped -= _overflowButton_Tapped;
            _overflowList.RowTapped -= _overflowList_RowTapped;

            base.Dispose();
        }

    }
}
