using System;
using System.Collections.Generic;
using System.Linq;
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
            /// Helper variable for the Selected property
            /// </summary>
            private bool _selected = false;
            public bool Selected {
                get
                {
                    return _selected;
                }
                set
                {
                    _selected = value;
                    if (value)
                    {
                        Background = TabPage.Background;
                    }
                    else
                    {
                        Background = TabControl.TabColor;
                    }
                }
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
                SelectedBackground = Colors.Green;
                Selected = false;
            }

            public override void Draw(CanvasDrawingSession ds)
            {
                ButtonText = TabPage.Name;
                base.Draw(ds);
            }
        }

        /// <summary>
        /// List of tabs in the TabControl
        /// </summary>
        protected List<TabPageUIElement> _tabs = new List<TabPageUIElement>();

        /// <summary>
        /// Map from each TabPage to the button it corresponds to
        /// </summary>
        protected Dictionary<TabPageUIElement, TabButtonUIElement> _tabDict = new Dictionary<TabPageUIElement, TabButtonUIElement>();
        /// <summary>
        /// List of buttons in the TabControl
        /// </summary>
        protected List<TabButtonUIElement> _tabButtons = new List<TabButtonUIElement>();

        /// <summary>
        /// LayoutManager to lay out the tab buttons
        /// </summary>
        protected StackLayoutManager _buttonLayoutManager = new StackLayoutManager();

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
                _selectedTab.IsVisible = true;
                _tabDict[_selectedTab].Selected = true;
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

        /// <summary>
        /// The height of each tab button
        /// </summary>
        public int TabHeight
        {
            get; set;
        } = 50;

        /// <summary>
        /// The width of each tab button
        /// </summary>
        public int TabWidth
        {
            get; set;
        } = 100;

        /// <summary>
        /// The color of each tab button when it is not selected.
        /// When it is selected it is the color of the page it corresponds to.
        /// </summary>
        public Color TabColor
        {
            get; set;
        } = Colors.MediumBlue;

        /// <summary>
        /// How to align the tab buttons
        /// </summary>
        public HorizontalAlignment TabAlignment
        {
            get; set;
        } = HorizontalAlignment.Left;

        /// <summary>
        /// The spacing in between each tab button
        /// </summary>
        public int TabSpacing
        {
            get; set;
        } = 0;

        /// <summary>
        /// Default constructor for TabControl
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public TabControlUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
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
                return _tabs[index];
            }
            set
            {
                _tabs[index] = value;
            }
        }

        /// <summary>
        /// Dictionary like access notation to access different tabs in the TabControl
        /// </summary>
        /// <param name="name">The name of the tab to access</param>
        /// <returns></returns>
        public TabPageUIElement this[string name]
        {
            get
            {
                return GetTabWithName(name);
            }
        }

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
            TabButtonUIElement button = new TabButtonUIElement(this, ResourceCreator, this, newTab);
            _tabButtons.Add(button);
            AddChild(button);
            _buttonLayoutManager.AddElement(button);
            button.Tapped += TabButton_Tapped;
            _tabDict.Add(newTab, button);
            newTab.Transform.LocalY = TabHeight;
            newTab.IsVisible = false;
            AddChild(newTab);
            if(select || SelectedTab == null)
            {
                SelectedTab = newTab;
            }
        }

        /// <summary>
        /// Event handler for tab buttons being tapped in order to change the selected tab
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void TabButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            TabButtonUIElement button = item as TabButtonUIElement;
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
            RemoveTabAt(_tabs.IndexOf(tab));
        }

        /// <summary>
        /// Remove the tab at the given index
        /// </summary>
        /// <param name="index">Index of tab to be removed</param>
        public void RemoveTabAt(int index)
        {
            if(SelectedIndex == index)
            {
                DeselectTabAt(index);
                if (SelectedIndex == index)
                {
                    SelectedTab = null;
                }
            }
            TabPageUIElement t = _tabs[index];
            TabButtonUIElement button = _tabDict[t];
            _tabButtons.Remove(button);
            RemoveChild(button);
            _buttonLayoutManager.Remove(button);
            button.Tapped -= TabButton_Tapped;
            RemoveChild(_tabs[index]);
            _tabs.RemoveAt(index);
            _tabDict.Remove(t);
        }

        /// <summary>
        /// Remove tab with given name if one exists in the TabControl
        /// </summary>
        /// <param name="name"></param>
        public void RemoveTabWithName(string name)
        {
            TabPageUIElement t = GetTabWithName(name);
            RemoveTab(t);
        }

        /// <summary>
        /// Remove all tabs from the TabControl
        /// </summary>
        public void Clear()
        {
            _tabs.Clear();
            SelectedTab = null;
        }

        /// <summary>
        /// Set the given tab to be selected. If tab is not contained in the TabControl, does nothing
        /// </summary>
        /// <param name="tab">The tab to set to be the selected tab</param>
        public void SelectTab(TabPageUIElement tab)
        {
            if (_tabs.Contains(tab))
            {
                SelectedTab = tab;
            }
        }

        /// <summary>
        /// Sets the tab at index to be selected
        /// </summary>
        /// <param name="index">The index of the tab to set to be the selected tab</param>
        public void SelectTabAt(int index)
        {
            SelectedIndex = index;
        }

        /// <summary>
        /// Sets the tab with the given name to be the selected tab.
        /// If no tab in the TabControl has the given name, does nothing
        /// </summary>
        /// <param name="name">The name of the tab to set to be the selected tab</param>
        public void SelectTabWithName(string name)
        {
            TabPageUIElement tp = GetTabWithName(name);
            if(tp != null)
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
            DeselectTabAt(GetTabIndexWithName(name));
        }

        /// <summary>
        /// Returns the local bounds of the tab at the given index
        /// </summary>
        /// <param name="index">The index of the tab to get the local bounds of</param>
        /// <returns>The local bounds of the tab at index</returns>
        public Rect GetTabLocalBounds(int index)
        {
            return _tabs[index].GetLocalBounds();
        }

        /// <summary>
        /// Returns the screen bounds of the tab at the given index
        /// </summary>
        /// <param name="index">The index of the tab to get the screen bounds of</param>
        /// <returns>The screen bounds of the tab at index</returns>
        public Rect GetTabScreenBounds(int index)
        {
            return _tabs[index].GetScreenBounds();
        }

        /// <summary>
        /// Swaps the location of the tabs at the given indices
        /// </summary>
        /// <param name="index1">The index of the first tab to swap</param>
        /// <param name="index2">The index of the second tab to swap</param>
        public void SwapTabs(int index1, int index2)
        {
            TabPageUIElement t = _tabs[index1];
            _tabs[index1] = _tabs[index2];
            _tabs[index2] = t;
        }

        /// <summary>
        /// Insert the tab at index oldIndex at index newIndex
        /// </summary>
        /// <param name="oldIndex">The index of the tab to move</param>
        /// <param name="newIndex">The index to insert the moved tab at</param>
        public void MoveTab(int oldIndex, int newIndex)
        {
            TabPageUIElement t = _tabs[oldIndex];
            _tabs.RemoveAt(oldIndex);
            _tabs.Insert(newIndex, t);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            _buttonLayoutManager.ItemWidth = TabWidth;
            _buttonLayoutManager.ItemHeight = TabHeight;
            _buttonLayoutManager.Height = TabHeight;
            _buttonLayoutManager.Width = Width;
            _buttonLayoutManager.HorizontalAlignment = TabAlignment;
            _buttonLayoutManager.Spacing = TabSpacing;
            _buttonLayoutManager.ArrangeItems();

            if (SelectedTab != null)
            {
                SelectedTab.Width = Width;
                SelectedTab.Height = Height - TabHeight;
            }
            base.Draw(ds);
        }

        public override void Dispose()
        {
            foreach(TabButtonUIElement button in _tabButtons)
            {
                button.Tapped -= TabButton_Tapped;
            }

            base.Dispose();
        }

    }
}
