using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
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
        /// Flag set to true if we need to reposition the text of the selected bubble
        /// </summary>
        private bool _bubblePositionsChanged;

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
            if (_userIds_toBubbles.ContainsKey(userid))
            {
                var bubble = _userIds_toBubbles[userid];
                RemoveChild(bubble);
                bubble.Tapped -= ShowUserNameOnBubbleTapped;
                bubble.RightTapped -= UserBubbleOnRightOrDoubleTapped;

                _userIds_toBubbles.Remove(userid);
                if (_currentUserNameDisplayed_userid == userid)
                {
                    _userNameRect.IsVisible = false;
                }
            }
            _bubblePositionsChanged = true;
        }
        private void NewNetworkUser(NetworkUser user)
        {
            if (_userIds_toBubbles.ContainsKey(user.UserID))
            {
                return;
            }
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
            userBubble.RightTapped += UserBubbleOnRightOrDoubleTapped;
            
            AddChild(userBubble);
         
            // add the user bubble to the list
            _userIds_toBubbles.Add(user.UserID, userBubble);

            _bubblePositionsChanged = true;
        }

        private void CenterUserNameRect()
        {
            if(_currentUserNameDisplayed_userid == null)
            {
                return;
            }
            if (!_userIds_toBubbles.ContainsKey(_currentUserNameDisplayed_userid)){
                return;
            }

            var userbubble = _userIds_toBubbles[_currentUserNameDisplayed_userid];
            _userNameRect.Transform.LocalPosition = userbubble.Transform.LocalPosition -
                                        new Vector2(_userNameRect.Width / 2 - userbubble.Width / 2,
                                            _userNameRect.Height + 5);

        }

        /// <summary>
        /// if the bubble is right tapped, then a flyout will show that will allow you to either invite the user to join your session 
        /// or allow you to join the user's session.
        /// </summary>
        /// <param name="sender"></param>
        private void UserBubbleOnRightOrDoubleTapped(ButtonUIElement sender)
        {
            ShowInviteJoinPopup(sender, sender.Transform.LocalX, sender.Transform.LocalX);
        }

        /// <summary>
        /// Event fired whenever a user clicks on a user bubble, displays the name of the user above the bubble
        /// hides the name of the user if the button is tapped again
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ShowUserNameOnBubbleTapped(ButtonUIElement sender)
        {
            // set the color of the username to the background of the button that was clicked
            _userNameRect.TextColor = sender.Background;
            // move the rect so it is centered over the button that was tapped
            _userNameRect.Transform.LocalPosition = sender.Transform.LocalPosition -
                                                    new Vector2(_userNameRect.Width / 2 - sender.Width / 2,
                                                        _userNameRect.Height + 5);
            // get the user id of the button that was selected
            var user_id =
                _userIds_toBubbles.Where(kv => kv.Value == sender).Select(kv => kv.Key).FirstOrDefault();

            Debug.Assert(user_id != null);
            if (user_id == null)
            {
                return;
            }

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
                _userNameRect.Text = name.Length == 0 ? "_" : name.Substring(0, Math.Min(name.Length, 10)).ToUpper();
            }
            _userNameRect.IsVisible = true;
            _currentUserNameDisplayed_userid = user_id;
        }

        /// <summary>
        /// private method to make the user buble invite and join flyouts appear.
        /// Pass is the user buble instance as well as the x and y coordinates of the popup.
        /// The coordinates should be obtainiable throuhg a canvas pointer activating this method
        /// </summary>
        /// <param name="bubble"></param>
        /// <param name="xCoordinate"></param>
        /// <param name="yCoordinate"></param>
        private void ShowInviteJoinPopup(ButtonUIElement bubble, float xCoordinate, float yCoordinate)
        {
            _currentUserNameDisplayed_userid =
                _userIds_toBubbles.Where(kv => kv.Value == bubble).Select(kv => kv.Key).FirstOrDefault();
            Debug.Assert(_currentUserNameDisplayed_userid != null);

            if (_currentUserNameDisplayed_userid != null && _currentUserNameDisplayed_userid != WaitingRoomView.UserID)
            {
                var flyout = new FlyoutPopup(SessionController.Instance.NuSessionView, Canvas);
                flyout.AddFlyoutItem("Join", JoinOnTappedEvent, Canvas);
                flyout.AddFlyoutItem("Invite", InviteOnTappedEvent, Canvas);
                SessionController.Instance.NuSessionView.AddChild(flyout);
                flyout.Transform.LocalPosition = new Vector2(xCoordinate, yCoordinate - flyout.FlyoutItemHeight * 2);
            }
        }

        /// <summary>
        /// user will invite the user they clicked on to join their session, and then it clears the current user displayed.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void InviteOnTappedEvent(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            StaticServerCalls.InviteCollaboratorToCollection(_currentUserNameDisplayed_userid);
            _currentUserNameDisplayed_userid = null;
            var flyoutParent = item.Parent as FlyoutPopup;
            flyoutParent.DismissPopup();
        }

        /// <summary>
        /// user will join the session of the user bubble they clicked on
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void JoinOnTappedEvent(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            StaticServerCalls.JoinCollaborator(_currentUserNameDisplayed_userid);
            _currentUserNameDisplayed_userid = null;
            var flyoutParent = item.Parent as FlyoutPopup;
            flyoutParent.DismissPopup();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            var position = new Vector2(0, 0);
            foreach (var user_button in _userIds_toBubbles.ToArray())
            {
                var button = user_button.Value;
                button.Transform.LocalPosition = position;

                position += new Vector2(buttonSpacing + buttonWidth, 0);
            }

            Width = Math.Max(buttonWidth*_userIds_toBubbles.Count + buttonSpacing*_userIds_toBubbles.Count - 1, 0);

            if (_bubblePositionsChanged)
            {
                CenterUserNameRect();
                _bubblePositionsChanged = false;
            }
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
