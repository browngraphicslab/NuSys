using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class TabContainerUIElement<T> : RectangleUIElement where T : IComparable<T>
    {
        /// <summary>
        /// List of TabButtons that are currently being shown by the TabContainer
        /// </summary>
        private List<TabButtonUIElement<T>> _tabList;

        /// <summary>
        /// The tab that is currently selected in the tab container
        /// </summary>
        public TabButtonUIElement<T> CurrentlySelectedTab { get; private set; } //todo make the setter method do something

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
        /// Invoked whenever the current tab is changed
        /// </summary>
        public event CurrentTabChangedHandler OnCurrentTabChanged;


        public TabContainerUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
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

            // add the handlers for the button getting selected and closed
            button.OnSelected += Button_OnSelected;
            button.OnClosed += Button_OnClosed;

            // and the button as a child
            AddChild(button);
        }

        /// <summary>
        /// Initializes a new Tab and return a TabButtonUIElement
        /// </summary>
        /// <returns></returns>
        private TabButtonUIElement<T> InitializeNewTab(T tab, string title)
        {
            var button = new TabButtonUIElement<T>(this, Canvas, new RectangleUIElement(this, Canvas), tab);
            button.ButtonText = title;
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
                // otherwise set the CurrentlySelectedTab to the tab after the one that was just removed
                else
                {
                    CurrentlySelectedTab = _tabList[index];
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

            // fire the event for when the tab changes
            OnCurrentTabChanged?.Invoke(tabType);
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws the tab container onto the screen
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            var tabWidth = Math.Min((Width - 2 * BorderWidth) /_tabList.Count, TabMaxWidth);
            var tabOffset = BorderWidth;

            // save the old transform
            var orgTransform = ds.Transform;

            // set the new transform to local to screen
            ds.Transform = Transform.LocalToScreenMatrix;

            foreach (var tab in _tabList)
            {
                tab.Width = tabWidth;
                tab.Height = TabHeight;
                tab.Transform.LocalPosition = new Vector2(tabOffset, 0);
                tabOffset += tabWidth;
            }

            // draw the background and the border and the tabs
            base.Draw(ds);

            ds.Transform = orgTransform;
        }
    }
}
