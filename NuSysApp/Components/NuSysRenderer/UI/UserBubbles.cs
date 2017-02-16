using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    /// <summary>
    /// Class which represents the rectangle area to the right of the node which houses the bubbles which show
    /// which users are editing the node
    /// 
    /// NOTE: hacky class. do not use in areas other than the node.
    /// </summary>
    public class UserBubbles : RectangleUIElement
    {
        /// <summary>
        /// Maps a userId to the circular button representing that user
        /// </summary>
        private Dictionary<string, ButtonUIElement> _bubbles;

        /// <summary>
        /// public accessor for _bubbles;
        /// </summary>
        public Dictionary<string, ButtonUIElement> Bubbles
        {
            get { return _bubbles; }
        }

        public UserBubbles(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

            IsHitTestVisible = false;
            BorderWidth = 0;
            Background = Colors.Transparent;

            _bubbles = new Dictionary<string, ButtonUIElement>();

            //MIRANDA, uncomment to test
            //InstantiateBubble(SessionController.Instance.NuSysNetworkSession.NetworkMembers.Keys.ToList().First());
        }

        /// <summary>
        /// Adds a bubble to the list
        /// </summary>
        /// <param name="userId">User that this bubble should represent</param>
        public void InstantiateBubble(string userId)
        {
            var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userId];
            var displayName = SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[userId];

            var bubble = new Bubble(Parent,Canvas,this,user);

            // Sets button text to be the first letter in the display name
            bubble.ButtonText = displayName[0].ToString();
            bubble.ButtonTextColor = Colors.White;
            bubble.Background = user.Color;

            // ellipses location may be set from center

            AddChild(bubble);
            _bubbles[userId] = bubble;
        }

        /// <summary>
        /// User is no longer editing node, so remove bubble from list
        /// </summary>
        /// <param name="userId">User to remove bubble of</param>
        public void RemoveBubble(string userId)
        {
            if (!_bubbles.ContainsKey(userId))
            {
                return;
            }
            var bubble = _bubbles[userId];
            RemoveChild(bubble);
        }

        /// <summary>
        /// Private class which is the actual bubble being displayed.  Kinda ignores all the interactive UI element stuff
        /// </summary>
        private class Bubble : ButtonUIElement
        {
            private CanvasTextFormat _textFormat;
            private UserBubbles _bubbles;
            private NetworkUser _user;
            public Bubble(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, UserBubbles bubbles, NetworkUser user) : base(parent, resourceCreator)
            {
                _bubbles = bubbles;
                _textFormat = new CanvasTextFormat
                {
                    HorizontalAlignment = CanvasHorizontalAlignment.Center,
                    VerticalAlignment = CanvasVerticalAlignment.Center
                };
                _user = user;
            }

            public override void Draw(CanvasDrawingSession ds)
            {
                var origRadius = 12;
                var old = ds.Transform;
                ds.Transform = Matrix3x2.Identity;
                var p = Transform.LocalToScreenMatrix.Translation;
                var radius = (float)(origRadius * Math.Min(Math.Max(origRadius * Transform.LocalToScreenMatrix.M11, .0000001), 1));
                p.X += (float)(Transform.LocalX + (origRadius * 1.1 ) - (origRadius - radius));
                var index = _bubbles.Bubbles.Values.ToList().IndexOf(this);

                p.Y += (float)( radius*index * 2.25 + radius);

                var color = _user?.Color ?? Colors.Aqua;

                ds.FillEllipse(p, radius,radius,color);
                var rect = new Rect(p.X - radius, p.Y - radius, (double)radius * 2, (double)radius * 2);
                _textFormat.FontSize = (float) (radius*UIDefaults.ButtonTextSize*SessionController.Instance.SessionSettings.TextScale/origRadius);
                ds.DrawText(_user?.DisplayName?.ToUpper()?.Substring(0,1) ?? "?", rect, Colors.Black,_textFormat);
                ds.Transform = old;
            }
        }
    }
}
