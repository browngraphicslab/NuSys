using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.UI;

namespace NuSysApp.Components.NuSysRenderer.DetailView.TabAndPageFramework
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
            }

            public override void Draw(CanvasDrawingSession ds)
            {
                if(Selected)
                {
                    Background = Colors.Blue;
                } else
                {
                    Background = Colors.Yellow;
                }
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

        private TabPageUIElement _selectedTab;
        public TabPageUIElement SelectedTab
        {
            get
            {
                return _selectedTab;
            }
            set
            {
                RemoveChild(_selectedTab);
                _tabDict[_selectedTab].Selected = false;
                _selectedTab = value;
                AddChild(_selectedTab);
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

        void AddTab(TabPageUIElement newTab, bool select = false)
        {
            newTab.Parent = this;
            _tabs.Add(newTab);
            TabButtonUIElement button = new TabButtonUIElement(this, ResourceCreator, newTab);
            _tabButtons.Add(button);
            AddChild(button);
            _buttonLayoutManager.AddElement(button);
            button.Tapped += TabButton_Tapped;
            _tabDict.Add(newTab, button);
            if(select)
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

        void RemoveTab(TabPageUIElement tab)
        {
            RemoveTabAt(_tabs.IndexOf(tab));
        }

        void RemoveTabAt(int index)
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
            _tabs.RemoveAt(index);
            _tabDict.Remove(t);
        }

        void RemoveTabWithName(string name)
        {
            TabPageUIElement t = GetTabWithName(name);
            RemoveTab(t);
        }

        void Clear()
        {
            _tabs.Clear();
            SelectedTab = null;
        }

        void SelectTab(TabPageUIElement tab)
        {
            SelectedTab = tab;
        }

        void SelectTabAt(int index)
        {
            SelectedIndex = index;
        }

        void SelectTabWithName(string name)
        {
            SelectedTab = GetTabWithName(name);
        }

        void DeselectTab(TabPageUIElement tab)
        {
            DeselectTabAt(_tabs.IndexOf(tab));
        }

        void DeselectTabAt(int index)
        {
            SelectedIndex = (index + 1) % _tabs.Count;
        }

        void DeselectTabWithName(string name)
        {
            DeselectTab(GetTabWithName(name));
        }

        Rect GetTabLocalBounds(int index)
        {
            return _tabs[index].GetLocalBounds();
        }
        
        Rect GetTabScreenBounds(int index)
        {
            return _tabs[index].GetScreenBounds();
        }

        void SwapTabs(int index1, int index2)
        {
            TabPageUIElement t = _tabs[index1];
            _tabs[index1] = _tabs[index2];
            _tabs[index2] = t;
        }

        void MoveTab(int oldIndex, int newIndex)
        {
            TabPageUIElement t = _tabs[oldIndex];
            _tabs.RemoveAt(oldIndex);
            _tabs.Insert(newIndex, t);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            _buttonLayoutManager.ItemWidth = 100;
            _buttonLayoutManager.Height = 200;
            _buttonLayoutManager.Width = Width;
            _buttonLayoutManager.ArrangeItems();

            SelectedTab.Transform.LocalPosition = new System.Numerics.Vector2(0, 200);
            SelectedTab.Transform.Size = new Size(Width, Height - 200);
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
