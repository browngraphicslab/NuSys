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
    public class UserBubbleContainerUIElement : RectangleUIElement
    {
        private Dictionary<string, ButtonUIElement> _userIds_toBubbles;
        private float buttonWidth = 50;
        private float buttonHeight = 50;
        private float buttonSpacing = 10;

        /// <summary>
        /// Rectangle used to display the user name when a button is tapped
        /// </summary>
        private TextboxUIElement _userNameRect;

        /// <summary>
        /// True if the container has already been loaded false otherwise
        /// </summary>
        private bool _loaded;

        /// <summary>
        ///  The user id of the current user name that is being displayed in the _userNameRect
        /// </summary>
        private string _currentUserNameDisplayed_userid;

        public UserBubbleContainerUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // instantiate a new _userBubbles list
            _userIds_toBubbles = new Dictionary<string, ButtonUIElement>();
            _userNameRect = new TextboxUIElement(this, Canvas)
            {
                Height = 20,
                Width = 100,
                Background = Colors.Transparent,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                IsVisible = false,
                IsHitTestVisible = false
            };
            AddChild(_userNameRect);
            Height = buttonHeight;

            Background = Colors.Transparent;
        }

        public override Task Load()
        {
            if (_loaded)
            {
                return base.Load(); 
            }

            // add all the curent users to the workspace
            foreach (var user in SessionController.Instance.NuSysNetworkSession.NetworkMembers.Values)
            {
                NewNetworkUser(user);
            }

            // add events so users are added and removed dynamically
            SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser += NewNetworkUser;
            SessionController.Instance.NuSysNetworkSession.OnNetworkUserDropped += DropNetworkUser;

            _loaded = true;
            return base.Load();
        }

        public override void Dispose()
        {
            SessionController.Instance.NuSysNetworkSession.OnNewNetworkUser -= NewNetworkUser;
            SessionController.Instance.NuSysNetworkSession.OnNetworkUserDropped -= DropNetworkUser;
            ClearUsers();
        
            base.Dispose();
        }

        private void DropNetworkUser(string userid)
        {
            Debug.Assert(_userIds_toBubbles.ContainsKey(userid));
            if (_userIds_toBubbles.ContainsKey(userid))
            {
                var bubble = _userIds_toBubbles[userid];
                RemoveChild(bubble);
                bubble.Tapped -= ShowUserNameOnBubbleTapped;
                _userIds_toBubbles.Remove(userid);
            }
            
        }

        private void NewNetworkUser(NetworkUser user)
        {
            // get the name of the user formatted correctly
            var name = user.DisplayName ?? user.UserID;
            name = name.Length == 0 ? "_" : name.TrimStart().Substring(0, 1).ToUpper();

            // instantiate the new userBubble
            var userBubble = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas))
            {
                Background = user.Color,
                ButtonText = name,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                ButtonTextColor = Colors.White,
                Width = buttonWidth,
                Height = buttonHeight
            };
            userBubble.Tapped += ShowUserNameOnBubbleTapped;
            AddChild(userBubble);
         
            // add the user bubble to the list
            _userIds_toBubbles.Add(user.UserID, userBubble);
        }

        /// <summary>
        /// Event fired whenever a user clicks on a user bubble, displays the name of the user above the bubble
        /// hides the name of the user if the button is tapped again
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ShowUserNameOnBubbleTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {

            var userbubble = interactiveBaseRenderItem as ButtonUIElement;
            Debug.Assert(userbubble != null);


            // set the color of the username to the background of the button that was clicked
            _userNameRect.TextColor = userbubble.Background;
            // move the rect so it is centered over the button that was tapped
            _userNameRect.Transform.LocalPosition = userbubble.Transform.LocalPosition -
                                                    new Vector2(_userNameRect.Width/2 - userbubble.Width/2, _userNameRect.Height + 5);
            // get the user id of the button that was selected
            var user_id = _userIds_toBubbles.Where(kv => kv.Value == userbubble).Select(kv => kv.Key).First();

            // hide the rectangle if the button clicked is already being displayed
            if (_currentUserNameDisplayed_userid == user_id)
            {
                _userNameRect.IsVisible = false;
                _currentUserNameDisplayed_userid = null;
                return;
            }

            // if the user id is a valid one then display the first ten characters of the name associated with the user id
            if (SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(user_id))
            {
                var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[user_id];
                var name = user.DisplayName ?? user.UserID;
                name = name.TrimStart();
                _userNameRect.Text = name.Length == 0 ? "_" : name.Substring(0, Math.Min(name.Length, 10 )).ToUpper();
            }
            _userNameRect.IsVisible = true;
            _currentUserNameDisplayed_userid = user_id;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            var position = new Vector2(0, 0);
            foreach (var user_button in _userIds_toBubbles)
            {
                var button = user_button.Value;
                button.Transform.LocalPosition = position;
                position += new Vector2(buttonSpacing + buttonWidth, 0);
            }

            Width = Math.Max(buttonWidth*_userIds_toBubbles.Count + buttonSpacing*_userIds_toBubbles.Count - 1, 0);

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Removes all the users from the user container
        /// </summary>
        public void ClearUsers()
        {
            foreach (var key in _userIds_toBubbles.Keys.ToArray())
            {
                DropNetworkUser(key); // make sure all removal logic is in DropNetworkUser, or Dispose, don't put it here or it won't always get called
            }
        }
    }
}
