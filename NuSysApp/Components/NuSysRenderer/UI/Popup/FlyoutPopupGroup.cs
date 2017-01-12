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

        public FlyoutPopupGroup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, FlyoutPopup head = null) : base(parent, resourceCreator)
        {
            Background = Colors.Transparent;
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
        public FlyoutPopup AddFlyoutPopup(FlyoutPopup popup, ButtonUIElement flyoutItem)
        {
            var newPopup = new FlyoutPopup(this, ResourceCreator);

            AddChild(newPopup);
            newPopup.Transform.LocalPosition = new Vector2(popup.Width, popup.FlyoutItems.IndexOf(flyoutItem) * popup.FlyoutItemHeight);
            newPopup.ParentPopup = popup;
            return newPopup;
        }

        /// <summary>
        /// Calls dismiss on every Popup
        /// </summary>
        private void DismissAllPopups()
        {
            foreach (var child in _children)
            {
                var popup = child as PopupUIElement;
                popup?.DismissPopup();
            }
        }

    }
}