using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using NuSysAp;

namespace NuSysApp
{
    public class ChatBoxUIElement : ResizeableWindowUIElement
    {
        /// <summary>
        /// The scrollable rectangle the user types their messages into
        /// </summary>
        private ScrollableTextboxUIElement _typingRect;

        /// <summary>
        /// ScrollingCanvas that contains a vertically scrolling list of all the messages
        /// </summary>
        private ScrollingCanvas _readingRect;


        private float _newMessageYOffset;

        public ChatBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the min height and min width
            MinWidth = 200;
            MinHeight = 200;
            Width = 300;
            Height = 300;
            KeepAspectRatio = false;


            // instantiate the typing and reading rectangles
            _typingRect = new ScrollableTextboxUIElement(this, resourceCreator, true, true)
            {
                BorderWidth = 3,
                Bordercolor = Colors.DarkGray,
                Width = Width,
                Height = 50
            };
            AddChild(_typingRect);

            _readingRect = new ScrollingCanvas(this, resourceCreator, ScrollingCanvas.ScrollOrientation.Vertical)
            {
                Background = Colors.Beige,
                Width = Width,
                Height = Height - _typingRect.Height - TopBarHeight,
            };
            _readingRect.ScrollAreaSize = new Size(Width - _readingRect.VerticalScrollBarWidth, _readingRect.Height);
            AddChild(_readingRect);

            _typingRect.KeyPressed += _typingRect_KeyPressed;
        }

        public override void Dispose()
        {
            _typingRect.KeyPressed -= _typingRect_KeyPressed;

            base.Dispose();
        }

        private void _typingRect_KeyPressed(Windows.UI.Core.KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Enter)
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
            
            var searchRequest = new WebSearchRequest(new WebSearchRequestArgs() {SearchString = text});
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(searchRequest);
            var s = searchRequest.WasSuccessful();
            
            //web search parser testing

            var chatRequest = new ChatRequest(SessionController.Instance.NuSysNetworkSession.NetworkMembers[WaitingRoomView.UserID], text);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(chatRequest);
            if (chatRequest.WasSuccessful() == true)
            {
                _typingRect.ClearText();
                chatRequest.AddSuccesfullChatLocally();
            }
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // resize so the typing rect is smaller than the reading rect
            _typingRect.Height = 50;
            _typingRect.Width = Width;
            _readingRect.Height = Height - _typingRect.Height - TopBarHeight;
            _readingRect.Width = Width;
            _readingRect.Transform.LocalPosition = new Vector2(_readingRect.Transform.LocalPosition.X, TopBarHeight);
            _typingRect.Transform.LocalPosition = new Vector2(_typingRect.Transform.LocalPosition.X, TopBarHeight + _readingRect.Height);

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
        }


        /// <summary>
        /// Adds a chat message to the ChatBox, called be the requests, and takes care of adding the current user's chats
        /// as well as other user's chats, as well as updating the number of unseen chats
        /// </summary>
        /// <param name="user"></param>
        /// <param name="chatMessage"></param>
        public void AddChat(NetworkUser user, string chatMessage)
        {
            var headerBox = new DynamicTextboxUIElement(this, Canvas)
            {
                FontWeight = FontWeights.Bold,
                Text = $"{user.DisplayName}  :  {DateTime.Now.ToString("h:mm:ss tt")}",
                Background = user.Color,
                Width = (float) _readingRect.ScrollAreaSize.Width
            };
            headerBox.Load();

            // add the element to the scrolling canvas
            _readingRect.AddElement(headerBox, new Vector2(0, _newMessageYOffset));

            // increment the message y offset by the height of the header box
            _newMessageYOffset += headerBox.Height;

            // add a new message box to the caht window with the background the same as the user's color
            var messageBox = new DynamicTextboxUIElement(this, Canvas)
            {
                Background = user.Color,
                Text = chatMessage,
                Width = (float) _readingRect.ScrollAreaSize.Width
            };
            // load the messageBox this is required for all dynamic text boxes
            messageBox.Load();

            // add the element to the scroling canvas
            _readingRect.AddElement(messageBox, new Vector2(0, _newMessageYOffset));

            // increase the y offset by the new messages height
            _newMessageYOffset += messageBox.Height;

            // set the scroll area size so that it can contain the new message
            UpdateScrollAreaSize();
        }

        private void UpdateScrollAreaSize()
        {
            _readingRect.ScrollAreaSize = new Size(Width - _readingRect.VerticalScrollBarWidth, Math.Max(_readingRect.Height, _newMessageYOffset - _readingRect.HorizontalScrollBarHeight));

        }
    }
}
