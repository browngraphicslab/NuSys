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
using SharpDX.Direct2D1.Effects;

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

        public ChatBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the min height and min width
            MinWidth = 200;
            MinHeight = 200;
            Width = 300;
            Height = 300;
            KeepAspectRatio = false;
            BorderWidth = 3;
            Bordercolor = Constants.LIGHT_BLUE;
            BorderType  = BorderType.Outside;
            

            _typingRectContainer = new RectangleUIElement(this, ResourceCreator)
            {
                Bordercolor = Constants.LIGHT_BLUE,
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
                Height = _typingRectContainer.Height
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
        }


        /// <summary>
        /// Adds a chat message to the ChatBox, called be the requests, and takes care of adding the current user's chats
        /// as well as other user's chats, as well as updating the number of unseen chats
        /// </summary>
        /// <param name="user"></param>
        /// <param name="chatMessage"></param>
        public void AddChat(NetworkUser user, string chatMessage)
        {

            var headerGrid = new GridLayoutManager(this, Canvas)
            {
                Width = (float) _readingRect.ScrollAreaSize.Width,
                Height = 25,
                Background = Colors.White
            };

            headerGrid.AddColumns(new List<float> {0.3f,0.7f, 0.1f});
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
            // add the element to the scrolling canvas
            _readingRect.AddElement(headerGrid, new Vector2(0, _newMessageYOffset));

            // increment the message y offset by the height of the header box
            _newMessageYOffset += headerGrid.Height;

            // add a new message box to the caht window with the background the same as the user's color
            var messageBox = new DynamicTextboxUIElement(this, Canvas)
            {
                Background =  Colors.White,
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
