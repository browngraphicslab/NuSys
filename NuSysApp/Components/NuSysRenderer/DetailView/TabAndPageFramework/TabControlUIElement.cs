using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;

namespace NuSysApp.Components.NuSysRenderer.DetailView.TabAndPageFramework
{
    class TabControlUIElement : RectangleUIElement
    {
        protected class TabButtonUIElement
        {
            public TabPageUIElement TabPage;

            public TabButtonUIElement(TabPageUIElement tabPage)
            {
                TabPage = tabPage;
            }
        }

        /// <summary>
        /// List of tabs in the TabControl
        /// </summary>
        protected List<TabPageUIElement> _tabs = new List<TabPageUIElement>();

        protected Dictionary<TabPageUIElement, TabButtonUIElement> _tabDict = new Dictionary<TabPageUIElement, TabButtonUIElement>();
        protected List<TabButtonUIElement> _tabButtons = new List<TabButtonUIElement>();

        public TabPageUIElement SelectedTab
        {
            get; set;
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
            _tabs.Add(newTab);
            TabButtonUIElement button = new TabButtonUIElement(newTab);
            _tabButtons.Add(button);
            _tabDict.Add(newTab, button);
            if(select)
            {
                SelectedTab = newTab;
            }
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
            _tabButtons.Remove(_tabDict[t]);
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

    }
}
