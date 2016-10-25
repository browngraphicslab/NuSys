using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Windows.UI;

namespace NuSysApp
{
    /// <summary>
    /// Class which represents the rectangle area to the right of the node which houses the bubbles which show
    /// which users are editing the node
    /// </summary>
    public class UserBubbles : RectangleUIElement
    {
        /// <summary>
        /// Maps a userId to the circular button representing that user
        /// </summary>
        private Dictionary<string, ButtonUIElement> Bubbles;

        // Manages the vertical layout of the bubbles
        private StackLayoutManager _bubbleLayoutManager;

        public UserBubbles(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            base.Width = 50;

            base.BorderWidth = 0;

            Bubbles = new Dictionary<string, ButtonUIElement>();
            _bubbleLayoutManager = new StackLayoutManager(StackAlignment.Vertical);

        }

        /// <summary>
        /// Adds a bubble to the list
        /// </summary>
        /// <param name="userId">User that this bubble should represent</param>
        public void InstantiateBubble(string userId)
        {
            var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userId];
            var displayName = SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[userId];

            var bubble = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas));

            // Sets button text to be the first letter in the display name
            bubble.ButtonText = displayName[0].ToString();
            bubble.ButtonTextColor = Colors.White;
            bubble.Background = user.Color;

            // ellipses location may be set from center

            AddChild(bubble);
            Bubbles[userId] = bubble;
            _bubbleLayoutManager.AddElement(bubble);
        }

        /// <summary>
        /// User is no longer editing node, so remove bubble from list
        /// </summary>
        /// <param name="userId">User to remove bubble of</param>
        public void RemoveBubble(string userId)
        {
            var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userId];
            var displayName = SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[userId];

            var bubble = Bubbles[userId];
            RemoveChild(bubble);
            _bubbleLayoutManager.Remove(bubble);
        }

        /// <summary>
        /// Ensure bubbles stay in the correct location relative to the node
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {

            _bubbleLayoutManager.SetSize(Width, Height);
            _bubbleLayoutManager.VerticalAlignment = VerticalAlignment.Bottom;
            _bubbleLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _bubbleLayoutManager.ItemWidth = Width - 10;
            _bubbleLayoutManager.ItemHeight = _bubbleLayoutManager.ItemWidth;
            _bubbleLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);
        }

    }
}
