using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using SharpDX.Direct2D1.Effects;
using NusysIntermediate;
using WinRTXamlToolkit.Tools;

namespace NuSysApp
{
    public class ChatBoxUIElement : ResizeableWindowUIElement
    {
        /// <summary>
        /// The scrollable rectangle the user types their messages into
        /// </summary>
        private ScrollableTextboxUIElement _typingRect;

        /// <summary>
        /// The rectangle that contains the textbox. this is useful because we want the border of the textbox to be visible at all times
        /// </summary>
        private RectangleUIElement _typingRectContainer;

        /// <summary>
        /// ScrollingCanvas that contains a vertically scrolling list of all the messages
        /// </summary>
        private ScrollingCanvas _readingRect;


        private float _newMessageYOffset;

        private TextboxUIElement _chatTitle;
        /// <summary>
        /// True if we want to scroll to the bottom of the page
        /// </summary>
        private bool _scrollToBottom;

        public ChatBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the min height and min width
            MinWidth = 200;
            MinHeight = 200;
            Width = 300;
            Height = 300;
            KeepAspectRatio = false;
            BorderWidth = 1;
            BorderColor = Constants.LIGHT_BLUE;
            BorderType  = BorderType.Outside;
            
            ShowClosable();
            IsSnappable = true;

            _typingRectContainer = new RectangleUIElement(this, ResourceCreator)
            {
                BorderColor = Constants.LIGHT_BLUE,
                BorderWidth = this.BorderWidth,
                Background = Colors.White,
                Height = 50,
                Width = Width
            };

            // instantiate the typing and reading rectangles
            _typingRect = new ScrollableTextboxUIElement(this, resourceCreator, true, true)
            {
                Background = Colors.White,
                Width = _typingRectContainer.Width,
                Height = _typingRectContainer.Height,
                PlaceHolderText = "Enter message here..."
            };
            _typingRectContainer.AddChild(_typingRect);
            AddChild(_typingRectContainer);

            _readingRect = new ScrollingCanvas(this, resourceCreator, ScrollingCanvas.ScrollOrientation.Vertical)
            {
                Background = Colors.White,
                Width = Width,
                Height = Height - _typingRectContainer.Height - TopBarHeight,
            };
            _readingRect.ScrollAreaSize = new Size(Width - _readingRect.VerticalScrollBarWidth, _readingRect.Height);
            AddChild(_readingRect);

            _chatTitle = new TextboxUIElement(this, Canvas)
            {
                Background = Colors.Transparent,
                TextColor = Constants.ALMOST_BLACK,
                BorderWidth =  0,
                FontSize = 20,
                FontFamily = UIDefaults.TitleFont,
                Text = "Chat",
                Height = 40,
                Width = 100
            };
            AddChild(_chatTitle);
            _chatTitle.Transform.LocalPosition = new Vector2(this.Transform.LocalX,
                this.Transform.LocalY - _chatTitle.Height);

            _typingRect.KeyPressed += _typingRect_KeyPressed;
        }

        public override void Dispose()
        {
            _typingRect.KeyPressed -= _typingRect_KeyPressed;

            base.Dispose();
        }

        private void _typingRect_KeyPressed(KeyArgs args)
        {
            if (args.Key == VirtualKey.Enter)
            {
                SendMessage(_typingRect.Text);
            }
        }

        /// <summary>
        /// locally send a chat request when the user hits the enter button
        /// </summary>
        /// <param name="text"></param>
        private async void SendMessage(string text)
        {
            if (CheckForChatbotChat(text))
            {
                _typingRect.ClearText();
                return;
            }

            var chatRequest = new ChatRequest(SessionController.Instance.NuSysNetworkSession.NetworkMembers[WaitingRoomView.UserID], text);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(chatRequest);
            if (chatRequest.WasSuccessful() == true)
            {
                _typingRect.ClearText();
                chatRequest.AddSuccesfullChatLocally();
            }
        }

        /// <summary>
        /// private method to parse text before making a chat reuqest.
        /// Should check to see if there was a special message sent.  
        /// This should be refactored better and elsewhere later.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool CheckForChatbotChat(string text)
        {
            var tokenized = text.Trim().Replace("  ", " ").ToLower().Split(' ');
            if (!tokenized.Any() || tokenized.Count() < 2)
            {
                return false;
            }
            if (tokenized[0] == "join")
            {
                var name = tokenized[1];
                var id = SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary.Where(kvp => kvp.Value.ToLower().Split(' ').FirstOrDefault() == name).Select(kvp => kvp.Key).FirstOrDefault();
                if (id != null && SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(id) && id != WaitingRoomView.UserID)
                {
                    StaticServerCalls.JoinCollaborator(id);
                    return true;
                }
                return false;
            }
            if (tokenized[0] == "invite")
            {
                var name = tokenized[1];
                var id = SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary.Where(kvp => kvp.Value.ToLower().Split(' ').FirstOrDefault() == name).Select(kvp => kvp.Key).FirstOrDefault();
                if (id != null && SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(id) && id != WaitingRoomView.UserID)
                {
                    StaticServerCalls.InviteCollaboratorToCollection(id);
                    return true;
                }
                return false;
            }
            return false;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // resize so the typing rect is smaller than the reading rect
            _typingRectContainer.Height = 50;
            _typingRectContainer.Width = Width;
            _typingRect.Width = _typingRectContainer.Width - 2* _typingRectContainer.BorderWidth;
            _typingRect.Height = _typingRectContainer.Height - 2* _typingRectContainer.BorderWidth;
            _readingRect.Height = Height - _typingRectContainer.Height - TopBarHeight;
            _readingRect.Width = Width;

            _readingRect.Transform.LocalPosition = new Vector2(0, TopBarHeight);
            _typingRectContainer.Transform.LocalPosition = new Vector2(0, TopBarHeight + _readingRect.Height);
            _typingRect.Transform.LocalPosition = new Vector2(BorderWidth, BorderWidth);


            var yOffset = 0f;
            foreach (var element in _readingRect.Elements.ToArray())
            {
                element.Width = Width;
                element.Transform.LocalPosition = new Vector2(0, yOffset);
                yOffset += element.Height;
            }

            _newMessageYOffset = yOffset;

            UpdateScrollAreaSize();

            base.Update(parentLocalToScreenTransform);


            if (_scrollToBottom)
            {
                _readingRect.Scrollto(ScrollingCanvas.ScrollTo.Bottom);
                _scrollToBottom = false;
            }

        }

        /// <summary>
        /// adds functional chat message that will call the callback function when clicked.  
        /// The callback function MUST remove itself from the sender's tapped event or else we have a memory leak
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="chatMessage"></param>
        /// <param name="callback"></param>
        public void AddFunctionalChat(NetworkUser user, string chatMessage, PointerHandler callback)
        {
            Debug.Assert(user != null);

            int numberToLeaveFunctional = 3;

            //linq statement to clear the callback of all existing functional chats
            _readingRect.Elements.OfType<FunctionalDynamicTextboxUIElement>().Reverse().Skip(numberToLeaveFunctional-1).ForEach(i => i.ClearCallback());

            var headerGrid = GetHeaderGrid(user);
            var messageBox = GetMessageBox(chatMessage);
            messageBox.Callback = callback;
            AddChat(headerGrid, messageBox);

            // if the chat box is currently hidden or the chat was sent by the current user
            // then scroll the chat down
            if (!IsVisible || user.UserID == WaitingRoomView.UserID)
            {
                _scrollToBottom = true;
            }
        }


        /// <summary>
        /// Adds a chat message to the ChatBox, called be the requests, and takes care of adding the current user's chats
        /// </summary>
        /// <param name="user"></param>
        /// <param name="chatMessage"></param>
        public void AddChat(NetworkUser user, string chatMessage)
        {
            Debug.Assert(user != null);
            var headerGrid = GetHeaderGrid(user);
            var messageBox = GetMessageBox(chatMessage);
            AddChat(headerGrid, messageBox);

            // if the chat box is currently hidden or the chat was sent by the current user
            // then scroll the chat down
            if (!IsVisible || user.UserID == WaitingRoomView.UserID)
            {
                _scrollToBottom = true;
            }

        }

        /// <summary>
        /// private method to add a chat using the two chat components, the header grid and the message box.
        /// </summary>
        /// <param name="headerGrid"></param>
        /// <param name="messageBox"></param>
        private void AddChat(GridLayoutManager headerGrid, DynamicTextboxUIElement messageBox)
        {

            // add the element to the scrolling canvas
            _readingRect.AddElement(headerGrid, new Vector2(0, _newMessageYOffset));

            // increment the message y offset by the height of the header box
            _newMessageYOffset += headerGrid.Height;

            // add the element to the scroling canvas
            _readingRect.AddElement(messageBox, new Vector2(0, _newMessageYOffset));

            // increment the message y offset by the height of the messageBox box
            _newMessageYOffset += messageBox.Height;
        }

        /// <summary>
        /// private method to get the message textbox from the given chat message.
        /// The message can be null.
        /// 
        /// </summary>
        /// <param name="chatMessage"></param>
        /// <returns></returns>
        private FunctionalDynamicTextboxUIElement GetMessageBox(string chatMessage)
        {
            // add a new message box to the caht window with the background the same as the user's color
            var messageBox = new FunctionalDynamicTextboxUIElement(this, Canvas)
            {
                Background = Colors.White,
                Text = chatMessage ?? "",
                Width = (float)_readingRect.ScrollAreaSize.Width
            };
            // load the messageBox this is required for all dynamic text boxes
            messageBox.Load();
            return messageBox;;
        }

        /// <summary>
        /// private method to get a header grid for a user at the current time
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private GridLayoutManager GetHeaderGrid(NetworkUser user)
        {
            var headerGrid = new GridLayoutManager(this, Canvas)
            {
                Width = (float)_readingRect.ScrollAreaSize.Width,
                Height = 25,
                Background = Colors.White
            };

            headerGrid.AddColumns(new List<float> { 0.3f, 0.7f, 0.1f });
            headerGrid.AddRows(new List<float> { 1 });

            var nameBox = new DynamicTextboxUIElement(this, Canvas)
            {
                Background = Colors.White,
                FontWeight = FontWeights.ExtraBold,
                Text = $"{user.DisplayName}",
                FontSize = 15,
                TextColor = user.Color,
                Width = (float)_readingRect.ScrollAreaSize.Width * 0.3f
            };

            var timeBox = new DynamicTextboxUIElement(this, Canvas)
            {
                FontSize = 12,
                Background = Colors.White,
                FontWeight = FontWeights.Normal,
                Text = $"{DateTime.Now.ToString("h:mm tt")}",
                TextColor = Constants.ALMOST_BLACK,
                Width = (float)_readingRect.ScrollAreaSize.Width * 0.7f
            };

            nameBox.Load();
            timeBox.Load();

            headerGrid.AddElement(nameBox, 0, 0, HorizontalAlignment.Left, VerticalAlignment.Stretch);
            headerGrid.AddElement(timeBox, 0, 1, HorizontalAlignment.Right, VerticalAlignment.Stretch);

            return headerGrid;
        }

        private void UpdateScrollAreaSize()
        {
            _readingRect.ScrollAreaSize = new Size(Width - _readingRect.VerticalScrollBarWidth, Math.Max(_readingRect.Height, _newMessageYOffset - _readingRect.HorizontalScrollBarHeight));

        }

        /// <summary>
        /// private class extending DynamicTextboxUIElement used to add custom, self-removing click handlers
        /// </summary>
        private class FunctionalDynamicTextboxUIElement : DynamicTextboxUIElement
        {
            /// <summary>
            /// the private version of Callback
            /// </summary>
            private PointerHandler _callback;

            /// <summary>
            /// Constructor is the same as the base class.
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="resourceCreator"></param>
            public FunctionalDynamicTextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator){}

            /// <summary>
            /// the callback function to call after being clicked
            /// </summary>
            public PointerHandler Callback {
                get
                {
                    return _callback;
                }
                set
                {
                    _callback = value;
                    Tapped -= OnTapped;
                    Tapped += OnTapped;
                }
            }

            /// <summary>
            /// method to call to remoev the callback from this functional textbox if one exitst
            /// </summary>
            public void ClearCallback()
            {
                Tapped -= OnTapped;
                _callback = null;
            }

            /// <summary>
            /// event handler called whenever this class is tapped;
            /// </summary>
            /// <param name="item"></param>
            /// <param name="pointer"></param>
            private void OnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
            {
                Tapped -= OnTapped;
                _callback?.Invoke(item,pointer);
            }

            /// <summary>
            /// override dispose method simply removes the tapped handler
            /// </summary>
            public override void Dispose()
            {
                ClearCallback();
                base.Dispose();
            }
        }
    }
}
