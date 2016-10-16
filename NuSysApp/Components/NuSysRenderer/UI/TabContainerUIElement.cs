using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public class TabContainerUIElement<T> : RectangleUIElement where T : IEquatable<T>
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
        /// The tab that is currently selected in the tab container
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

        /// <summary>
        /// delegate for when the current tab is changed
        /// </summary>
        /// <param name="tabType"></param>
        public delegate void CurrentTabChangedHandler(T tabType);

        /// <summary>
        /// The color of the tabs in the tab container
        /// </summary>
        public Color TabColor { get; set; }

        /// <summary>
        /// Invoked whenever the current tab is changed
        /// </summary>
        public event CurrentTabChangedHandler OnCurrentTabChanged;

        /// <summary>
        /// The Tab container's _stackLayoutManager
        /// </summary>
        private StackLayoutManager _tabStackLayoutManager;

        /// <summary>
        /// The layout manager for the current page
        /// </summary>
        private StackLayoutManager _pageStackLayoutManager;

        private bool _tabsIsCloseable;

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

        public TabContainerUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // initialize the _tabList
            _tabList = new List<TabButtonUIElement<T>>();
            _tabStackLayoutManager = new StackLayoutManager();
            _pageStackLayoutManager = new StackLayoutManager();

            Page = new RectangleUIElement(this, Canvas);
            Page.Background = Colors.Red;
            _pageStackLayoutManager.AddElement(Page);
            AddChild(Page);

            // initialize defaults
            TabHeight = UIDefaults.TabHeight;
            TabColor = UIDefaults.TabColor;
            TabMaxWidth = UIDefaults.TabMaxWidth;
            BorderWidth = 0;
            TabsIsCloseable = UIDefaults.TabIsCloseable;

            Background = Colors.Yellow;
        }

        /// <summary>
        /// Sets the page to be displayed in the tab container
        /// </summary>
        /// <param name="newPage"></param>
        public void setPage(RectangleUIElement newPage)
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
        public void AddTab(T tab, string title = "")
        {
            // if any Tab in the tablist has the same tabType as the one we are trying to add
            // then return
            if (_tabList.Any(tabButton => IsEqual(tab, tabButton.Tab)))
            {
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

            // set the currently selected tab to the new tab
            CurrentlySelectedTab = button;
        }

        /// <summary>
        /// Initializes a new Tab and return a TabButtonUIElement
        /// </summary>
        /// <returns></returns>
        private TabButtonUIElement<T> InitializeNewTab(T tab, string title)
        {
            var button = new TabButtonUIElement<T>(this, Canvas, tab);
            button.Title = title;
            button.TitleColor = Colors.Black;
            button.Background = TabColor;
            button.Height = TabHeight;
            button.Width = Math.Min((Width - 2 * BorderWidth) / _tabList.Count, TabMaxWidth);
            button.IsCloseable = TabsIsCloseable;
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
            if (IsEqual(_tabList[index].Tab, CurrentlySelectedTab.Tab))
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
        /// Closes the TabContainer
        /// </summary>
        public void CloseTabContainer()
        {
            
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

            var index = _tabList.IndexOf(CurrentlySelectedTab);

            var lineWidth = 4f;

            var tabWidth = _tabStackLayoutManager.ItemWidth;

            // draw the line under the tabs up to the currently selected tab
            ds.DrawLine(new Vector2(BorderWidth, TabHeight + BorderWidth + lineWidth / 2), new Vector2(Math.Max(index, 0) * tabWidth + BorderWidth, TabHeight + BorderWidth + lineWidth / 2), Bordercolor, 3);
            // draw the line after the currently selected tab to the end of the list
            ds.DrawLine(new Vector2((index + 1) * tabWidth + BorderWidth, TabHeight + BorderWidth + lineWidth / 2), new Vector2(Width - BorderWidth, TabHeight + BorderWidth + lineWidth / 2), Bordercolor, 3);

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// Updates the layout of the tabs
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // arrange the tabs
            _tabStackLayoutManager.SetMargins(BorderWidth);
            _tabStackLayoutManager.ItemHeight = TabHeight;
            _tabStackLayoutManager.ItemWidth = Math.Min((Width - 2*BorderWidth)/_tabList.Count, TabMaxWidth);
            _tabStackLayoutManager.Width = Width;
            _tabStackLayoutManager.Height = Height;
            _tabStackLayoutManager.HorizontalAlignment = HorizontalAlignment.Left;
            _tabStackLayoutManager.VerticalAlignment = VerticalAlignment.Top;
            _tabStackLayoutManager.ArrangeItems();

            // arrange the page
            _pageStackLayoutManager.SetMargins(BorderWidth);
            _pageStackLayoutManager.TopMargin = TabHeight;
            _pageStackLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _pageStackLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _pageStackLayoutManager.Width = Width;
            _pageStackLayoutManager.Height = Height;
            _pageStackLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);
        }
    }
}
