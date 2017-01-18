using System;
using Microsoft.Graphics.Canvas;
using System.Numerics;
using Windows.UI;

namespace NuSysApp
{
    /// <summary>
    /// This class is used to create FlyoutPopup chains that have a hierarchy. For example, if I want to create a FlyoutPopup that when clicked
    /// creates a FlyoutPopup to its side, I would first call AddHeadFlyoutPopup, then AddFlyoutPopup, passing in the flyoutitem you want to branch
    /// off of.
    /// 
    /// The main benefit to using this class is not having to keep track of each individual FlyoutPopup as a child, and instead add a single
    /// FlyoutPopupGroup as a child. No need to worry about the transforms of each child.
    /// </summary>
    public class FlyoutPopupGroup : RectangleUIElement
    {
        /// <summary>
        /// The source of the flyoutpopup group. For example, if a header item triggers the creation of a flyoutpopupgroup, 
        /// the headeritem is its source. 
        /// 
        /// </summary>
        public BaseInteractiveUIElement Source { set; get; }

        public FlyoutPopupGroup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BaseInteractiveUIElement source) : base(parent, resourceCreator)
        {
            Source = source;
            Background = Colors.Transparent;
            OnChildFocusLost += FlyoutPopupGroup_OnChildFocusLost;
        }
        /// <summary>
        /// When a single flyoutpopup's focus is lost, make sure to dismiss every single popup
        /// </summary>
        /// <param name="item"></param>
        private void FlyoutPopupGroup_OnChildFocusLost(BaseRenderItem item)
        {
            DismissAllPopups();
        }

        /// <summary>
        /// Adds the main flyoutpopup
        /// </summary>
        /// <returns></returns>
        public FlyoutPopup AddHeadFlyoutPopup()
        {
            var newPopup = new FlyoutPopup(this, ResourceCreator);
            AddChild(newPopup);
            return newPopup;
        }
        /// <summary>
        /// Adds a FlyoutPopup to the right hand side of the flyoutitem of the popup passed in
        /// </summary>
        /// <param name="popup"></param>
        /// <param name="flyoutItem"></param>
        /// <returns></returns>
        public FlyoutPopup AddFlyoutPopup(ButtonUIElement flyoutItem)
        {
            var parent = flyoutItem.Parent as FlyoutPopup;
            parent.Dismissable = false; //Makes sure that the parent flyoutpopup is not dismissed

            var newPopup = new FlyoutPopup(this, ResourceCreator);
            AddChild(newPopup);
            newPopup.Transform.LocalPosition = new Vector2(flyoutItem.Transform.LocalX + flyoutItem.Width, flyoutItem.Transform.LocalY);
            return newPopup;
        }

        /// <summary>
        /// Calls dismiss on every Popup
        /// </summary>
        public void DismissAllPopups()
        {
            foreach (var child in GetChildren())
            {
                var popup = child as PopupUIElement;
                popup.Dismissable = true;
                popup?.DismissPopup();
            }
        }

        public override void Dispose()
        {
            OnChildFocusLost -= FlyoutPopupGroup_OnChildFocusLost;
            base.Dispose();
        }
    }
}