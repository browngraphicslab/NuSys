using System;
using Microsoft.Graphics.Canvas;
using System.Numerics;
using Windows.UI;

namespace NuSysApp
{
    public class FlyoutPopupGroup : RectangleUIElement
    {
        public FlyoutPopupGroup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, FlyoutPopup head = null) : base(parent, resourceCreator)
        {
            Background = Colors.Transparent;
        }


        public FlyoutPopup AddHeadFlyoutPopup()
        {
            var newPopup = new FlyoutPopup(this, ResourceCreator);
            AddChild(newPopup);
            return newPopup;
        }

        public FlyoutPopup AddFlyoutPopup(FlyoutPopup popup, ButtonUIElement flyoutItem)
        {
            var newPopup = new FlyoutPopup(this, ResourceCreator);

            AddChild(newPopup);
            newPopup.Transform.LocalPosition = new Vector2(popup.Width, popup.FlyoutItems.IndexOf(flyoutItem) * popup.FlyoutItemHeight);
            newPopup.ParentPopup = popup;
            return newPopup;
        }


        private void DismissAllPopups()
        {
            foreach (var child in _children)
            {
                var popup = child as PopupUIElement;
                popup.DismissPopup();
            }
        }

    }
}