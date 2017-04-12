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
    class TabControlUIElement : RectangleUIElement
    {
        protected class TabButtonUIElement : ButtonUIElement
        {
            public TabPageUIElement TabPage;

            private bool _selected = false;
            public bool Selected {
                get
                {
                    return _selected;
                }
                set
                {
                    _selected = value;
                }
            }

            public TabButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, TabPageUIElement tabPage, BaseInteractiveUIElement shape = null) : base(parent, resourceCreator, shape)
            {
                TabPage = tabPage;
                SelectedBackground = Colors.Green;
            }

            public override void Draw(CanvasDrawingSession ds)
            {
                if (Selected)
                {
                    Background = TabPage.Background;
                }
                else
                {
                    Background = Colors.Yellow;
                }
                Height = 50;
                ButtonText = TabPage.Name;
                base.Draw(ds);
            }
        }

        /// <summary>
        /// List of tabs in the TabControl
        /// </summary>
        protected List<TabPageUIElement> _tabs = new List<TabPageUIElement>();

        protected Dictionary<TabPageUIElement, TabButtonUIElement> _tabDict = new Dictionary<TabPageUIElement, TabButtonUIElement>();
        protected List<TabButtonUIElement> _tabButtons = new List<TabButtonUIElement>();

        protected StackLayoutManager _buttonLayoutManager = new StackLayoutManager();

        private TabPageUIElement _selectedTab = null;
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

        public int TabButtonHeight
        {
            get; set;
        } = 50;

        public int TabButtonWidth
        {
            get; set;
        } = 100;

        /// <summary>
        /// Default constructor for TabControl
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public TabControlUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }

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

        public TabPageUIElement this[string name]
        {
            get
            {
                return GetTabWithName(name);
            }
        }

        public TabPageUIElement GetTabWithName(string name)
        {
            return _tabs.Find((TabPageUIElement t) => t.Name.Equals(name));
        }

        public int GetTabIndexWithName(string name)
        {
            return _tabs.FindIndex((TabPageUIElement t) => t.Name.Equals(name));
        }

        public void AddTab(TabPageUIElement newTab, bool select = false)
        {
            newTab.Parent = this;
            _tabs.Add(newTab);
            TabButtonUIElement button = new TabButtonUIElement(this, ResourceCreator, newTab);
            _tabButtons.Add(button);
            AddChild(button);
            _buttonLayoutManager.AddElement(button);
            button.Tapped += TabButton_Tapped;
            _tabDict.Add(newTab, button);
            newTab.Transform.LocalY = TabButtonHeight;
            newTab.IsVisible = false;
            AddChild(newTab);
            if(select || SelectedTab == null)
            {
                SelectedTab = newTab;
            }
        }

        private void TabButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            TabButtonUIElement button = item as TabButtonUIElement;
            if(button == null)
            {
                return;
            }
            SelectedTab = button.TabPage;
        }

        public void RemoveTab(TabPageUIElement tab)
        {
            RemoveTabAt(_tabs.IndexOf(tab));
        }

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

        public void RemoveTabWithName(string name)
        {
            TabPageUIElement t = GetTabWithName(name);
            RemoveTab(t);
        }

        public void Clear()
        {
            _tabs.Clear();
            SelectedTab = null;
        }

        public void SelectTab(TabPageUIElement tab)
        {
            SelectedTab = tab;
        }

        public void SelectTabAt(int index)
        {
            SelectedIndex = index;
        }

        public void SelectTabWithName(string name)
        {
            SelectedTab = GetTabWithName(name);
        }

        public void DeselectTab(TabPageUIElement tab)
        {
            DeselectTabAt(_tabs.IndexOf(tab));
        }

        public void DeselectTabAt(int index)
        {
            SelectedIndex = (index + 1) % _tabs.Count;
        }

        public void DeselectTabWithName(string name)
        {
            DeselectTab(GetTabWithName(name));
        }

        public Rect GetTabLocalBounds(int index)
        {
            return _tabs[index].GetLocalBounds();
        }

        public Rect GetTabScreenBounds(int index)
        {
            return _tabs[index].GetScreenBounds();
        }

        public void SwapTabs(int index1, int index2)
        {
            TabPageUIElement t = _tabs[index1];
            _tabs[index1] = _tabs[index2];
            _tabs[index2] = t;
        }

        public void MoveTab(int oldIndex, int newIndex)
        {
            TabPageUIElement t = _tabs[oldIndex];
            _tabs.RemoveAt(oldIndex);
            _tabs.Insert(newIndex, t);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            _buttonLayoutManager.ItemWidth = TabButtonWidth;
            _buttonLayoutManager.ItemHeight = TabButtonHeight;
            _buttonLayoutManager.Height = TabButtonHeight;
            _buttonLayoutManager.Width = Width;
            _buttonLayoutManager.ArrangeItems();

            if (SelectedTab != null)
            {
                SelectedTab.Width = Width;
                SelectedTab.Height = Height - TabButtonHeight;
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
