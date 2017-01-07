using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class TabContainerUIElement<T> : RectangleUIElement where T : IComparable<T>
    {
        /// <summary>
        /// List of TabButtons that are currently being shown by the TabContainer
        /// </summary>
        private List<TabButtonUIElement<T>> _tabList;

        /// <summary>
        /// private helper variable for public property CurrentlySelectedTab
        /// </summary>
        private TabButtonUIElement<T> _currentlySelectedTab;

        /// <summary>
        /// The tab that is currently selected in the tab container, when this property is set the OnCurrentTabChanged event is fired.
        /// This could cause an infinite loop.
        /// </summary>
        public TabButtonUIElement<T> CurrentlySelectedTab
        {
            get { return _currentlySelectedTab; }
            set
            {
                _currentlySelectedTab = value;

                // fire the event for when the tab changes
                OnCurrentTabChanged?.Invoke(_currentlySelectedTab.Tab);
            }
        } 

        /// <summary>
        /// The height of the tabs in the tab container
        /// </summary>
        public float TabHeight { get; set; }

        /// <summary>
        /// The maximum width of the tabs in the tab container
        /// </summary>
        public float TabMaxWidth { get; set; }

        public float TabSpacing { get; set; }

        public float TabBarHeight { get; set; }

        public HorizontalAlignment TabHorizontalAlignment { get; set; }

        public VerticalAlignment TabVerticalAlignment { get; set; }

        public Color TabBarBackground { get; set; }

        public Color TabBarBorderColor { get; set; }

        public float TabBarBorderWidth { get; set; }

        /// <summary>
        /// Fired when all the tabs in the tab container are closed
        /// </summary>
        public event EventHandler TabContainerClosed;

        /// <summary>
        /// delegate for when the current tab is changed
        /// </summary>
        /// <param name="tabType"></param>
        public delegate void CurrentTabChangedHandler(T tabType);

        /// <summary>
        /// Invoked whenever the current tab is changed
        /// </summary>
        public event CurrentTabChangedHandler OnCurrentTabChanged;

        /// <summary>
        /// delegate for when a tab is removed
        /// </summary>
        /// <param name="tabType"></param>
        public delegate void OnTabRemovedHandler(T tabType);

        /// <summary>
        /// Invoked whenever a tab is removed from the tab container
        /// </summary>
        public event OnTabRemovedHandler OnTabRemoved;

        /// <summary>
        /// The color of the tabs in the tab container
        /// </summary>
        public Color TabColor { get; set; }

        /// <summary>
        /// The Tab container's _stackLayoutManager
        /// </summary>
        private StackLayoutManager _tabStackLayoutManager;

        /// <summary>
        /// The layout manager for the current page
        /// </summary>
        private StackLayoutManager _pageStackLayoutManager;

        /// <summary>
        /// helper variable for public property TabsIsCloseable
        /// </summary>
        private bool _tabsIsCloseable;

        private Color _titleColor;

        public Color TitleColor
        {
            get { return _titleColor;}
            set { _titleColor = value; }
        }

        /// <summary>
        /// True if the tabs are closeable, false otherwise
        /// </summary>
        public bool TabsIsCloseable
        {
            get { return _tabsIsCloseable; }
            set
            {
                _tabsIsCloseable = value;
                foreach (var tab in _tabList)
                {
                    tab.IsCloseable = _tabsIsCloseable;
                }
            }
        }

        /// <summary>
        /// The current page to display in the tab container
        /// </summary>
        public RectangleUIElement Page { get; set; }

        private RectangleUIElement _tabBar;

        public CanvasHorizontalAlignment TabTextAlignment { get; set; }

        public TabContainerUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // initialize the _tabList and the layout managers
            _tabList = new List<TabButtonUIElement<T>>();
            _tabStackLayoutManager = new StackLayoutManager();
            _pageStackLayoutManager = new StackLayoutManager();

            // initialize the page for the tab container
            Page = new RectangleUIElement(this, Canvas);
            Page.Background = Constants.LIGHT_BLUE;
            _pageStackLayoutManager.AddElement(Page);
            AddChild(Page);

            _tabBar = new RectangleUIElement(this,Canvas);
            AddChild(_tabBar);

            // initialize defaults for the ui
            TabHeight = UIDefaults.TabHeight;
            TabColor = UIDefaults.TabColor;
            TabMaxWidth = UIDefaults.TabMaxWidth;
            TabBarHeight = UIDefaults.TabBarHeight;
            TabBarBackground = UIDefaults.TabBarBackground;
            TabBarBorderWidth = UIDefaults.TabBarBorderWidth;
            TabSpacing = UIDefaults.TabSpacing;
            TabHorizontalAlignment = UIDefaults.TabHorizontalAlignment;
            TabVerticalAlignment = UIDefaults.TabVerticalAlignment;
            TabTextAlignment = UIDefaults.TabTextAlignment;
            TitleColor = UIDefaults.TitleColor;
            BorderWidth = 0;
            TabsIsCloseable = UIDefaults.TabIsCloseable;
        }

        /// <summary>
        /// Sets the page to be displayed in the tab container, this takes care of adding the child so do not do it separately
        /// </summary>
        /// <param name="newPage"></param>
        public void SetPage(RectangleUIElement newPage)
        {
            if (Page != null)
            {
                RemoveChild(Page);
                _pageStackLayoutManager.Remove(Page);
            }

            Page = newPage;
            _pageStackLayoutManager.AddElement(Page);
            AddChild(Page);
        }

        /// <summary>
        /// Adds a new tab to the tab container. If there is already a tab of tab
        /// it does nothing. Optional title argument.
        /// </summary>
        /// <param name="tabType"></param>
        /// <param name="showTab">True if you want to display the tab, false if you just want to add it as a tab and not display it</param>
        public void AddTab(T tab, string title = "", bool showTab = true)
        {
            // if any Tab in the tablist has the same tabType as the one we are trying to add
            // then return
            var equivalentTab = _tabList.FirstOrDefault(e => IsEqual(e.Tab, tab));

            if (equivalentTab != null)
            {
                if (showTab)
                {
                    CurrentlySelectedTab = equivalentTab;
                }
                return;
            }

            // add the new button to the tablist
            var button = InitializeNewTab(tab, title);
            _tabList.Add(button);
            _tabStackLayoutManager.AddElement(button);

            // add the handlers for the button getting selected and closed
            button.OnSelected += Button_OnSelected;
            button.OnClosed += Button_OnClosed;

            // and the button as a child
            AddChild(button);
            button.Load();

            if (showTab)
            {
                // set the currently selected tab to the new tab
                CurrentlySelectedTab = button;
            }
            
        }

        /// <summary>
        /// Initializes a new Tab and return a TabButtonUIElement
        /// </summary>
        /// <returns></returns>
        private TabButtonUIElement<T> InitializeNewTab(T tab, string title)
        {
            var button = new TabButtonUIElement<T>(this, Canvas, tab)
            {
                Title = title,
                TitleColor = TitleColor,
                Background = TabColor,
                Height = TabHeight,
                Width = Math.Min((Width - 2*BorderWidth)/_tabList.Count, TabMaxWidth),
                IsCloseable = TabsIsCloseable,
                TextAlignment = TabTextAlignment
            };
            return button;
        }

        /// <summary>
        /// Removes the given tab from the tablist
        /// </summary>
        /// <param name="tabType"></param>
        public void RemoveTab(T tabType)
        {
            // get the tab which is going to be removed from the list of tabs
            var tabToBeRemoved = _tabList.FirstOrDefault(tabButton => IsEqual(tabType, tabButton.Tab));
            Debug.Assert(tabToBeRemoved != null);

            // get the index of the tab that is going to be removed
            var index = _tabList.IndexOf(tabToBeRemoved);

            // if the tabToBeRemoved is the CurrentlySelectedTab
            if (CurrentlySelectedTab != null && IsEqual(_tabList[index].Tab, CurrentlySelectedTab.Tab))
            {
                // remove it
                _tabList.RemoveAt(index);

                // if there are no tabs left then close the tab container
                if (_tabList.Count == 0)
                {
                    CloseTabContainer();
                }
                // otherwise set the CurrentlySelectedTab to the tab after the one that was just removed unless the tab was the list in the list
                else
                {
                    // check if the tab was the last in the list
                    if (_tabList.Count == index)
                    {
                        CurrentlySelectedTab = _tabList[index - 1];
                    }
                    else
                    {
                        // if it isn't set the tab to the tab after the one that was just removed
                        CurrentlySelectedTab = _tabList[index];

                    }
                }

            }
            // otherwise the tab is not the CurrentlySelectedTab so simply remove it
            else
            {
                _tabList.Remove(tabToBeRemoved);
            }

            // take care of removing the added event handlers
            tabToBeRemoved.OnSelected -= Button_OnSelected;
            tabToBeRemoved.OnClosed -= Button_OnClosed;

            // remove the tab as a child
            RemoveChild(tabToBeRemoved);
            _tabStackLayoutManager.Remove(tabToBeRemoved);
            OnTabRemoved?.Invoke(tabType);
        }

        /// <summary>
        /// Removes a Tab from the tablist when the tab is closed by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tabType"></param>
        private void Button_OnClosed(T tabType)
        {
            // remove the tab from the tablist
            RemoveTab(tabType);
        }

        private void Button_OnSelected(T tabType)
        {
            // get the tab which just got selected
            var tabToBeSelected = _tabList.FirstOrDefault(tabButton => IsEqual(tabType, tabButton.Tab));
            Debug.Assert(tabToBeSelected != null);

            if (CurrentlySelectedTab == tabToBeSelected)
            {
                return;
            }

            // set the currently selected tab to the tab which was selected
            CurrentlySelectedTab = tabToBeSelected;
        }

        /// <summary>
        /// Determines if two items that implement IComparable T are equal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsEqual<T>(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        /// <summary>
        /// Closes the TabContainer. Called when the tab container is empty
        /// </summary>
        public void CloseTabContainer()
        {
            TabContainerClosed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clear all the tabs from the tab container
        /// </summary>
        public void ClearTabs()
        {
            while (_tabList.Count != 0)
            {
                RemoveTab(_tabList[0].Tab);
            }
        }

        /// <summary>
        /// Draws the tab container onto the screen
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // save the old transform
            var orgTransform = ds.Transform;

            // set the new transform to local to screen
            ds.Transform = Transform.LocalToScreenMatrix;

            // draw the background and the border and the tabs
            base.Draw(ds);

            //var index = _tabList.IndexOf(CurrentlySelectedTab);

            //var lineWidth = 4f;

            //var tabWidth = _tabStackLayoutManager.ItemWidth;

            //// draw the line under the tabs up to the currently selected tab
            //ds.DrawLine(new Vector2(BorderWidth, TabHeight + BorderWidth + lineWidth / 2), new Vector2(Math.Max(index, 0) * tabWidth + BorderWidth, TabHeight + BorderWidth + lineWidth / 2), Bordercolor, 3);
            //// draw the line after the currently selected tab to the end of the list
            //ds.DrawLine(new Vector2((index + 1) * tabWidth + BorderWidth, TabHeight + BorderWidth + lineWidth / 2), new Vector2(Width - BorderWidth, TabHeight + BorderWidth + lineWidth / 2), Bordercolor, 3);

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Updates the layout of the tabs
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _tabBar.Height = TabBarHeight;
            _tabBar.Width = Width - 2*BorderWidth;
            _tabBar.Transform.LocalPosition = new Vector2(BorderWidth);
            _tabBar.Background = TabBarBackground;
            _tabBar.Bordercolor = TabBarBorderColor;
            _tabBar.BorderWidth = TabBarBorderWidth;

            // arrange the tabs
            _tabStackLayoutManager.SetMargins(BorderWidth);
            _tabStackLayoutManager.ItemHeight = TabHeight;
            _tabStackLayoutManager.ItemWidth = Math.Min((Width - 2*BorderWidth)/_tabList.Count, TabMaxWidth);
            _tabStackLayoutManager.Spacing = TabSpacing;
            _tabStackLayoutManager.LeftMargin = TabSpacing;
            _tabStackLayoutManager.RightMargin = TabSpacing;
            _tabStackLayoutManager.SetSize(Width, TabBarHeight);
            _tabStackLayoutManager.HorizontalAlignment = TabHorizontalAlignment;
            _tabStackLayoutManager.VerticalAlignment = TabVerticalAlignment;
            _tabStackLayoutManager.ArrangeItems();

            // arrange the page
            _pageStackLayoutManager.SetMargins(BorderWidth);
            _pageStackLayoutManager.TopMargin = TabBarHeight;
            _pageStackLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _pageStackLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _pageStackLayoutManager.SetSize(Width, Height);
            _pageStackLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);
        }
    }
}
