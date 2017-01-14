using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class DetailViewTextContent : RectangleUIElement, ILockable
    {
        /// <summary>
        /// enum to represent the possible UI states.
        /// </summary>
        private enum DetailViewTextDisplay
        {
            Markdown,
            Plaintext,
            MarkdownAndPlaintext
        }

        /// <summary>
        /// the current state of the detail view text viewer.
        /// This should only be set through the SetTextDisplayState method.
        /// </summary>
        private DetailViewTextDisplay _displayState = DetailViewTextDisplay.MarkdownAndPlaintext;

        /// <summary>
        /// The main textbox we write in
        /// </summary>
        private ScrollableTextboxUIElement _mainTextBox;

        /// <summary>
        /// The layout manager for the main textbox
        /// </summary>
        private StackLayoutManager _mainTextboxLayoutManager;

        /// <summary>
        /// The library element controller for the text associated with this detail view page
        /// </summary>
        private LibraryElementController _controller;

        /// <summary>
        ///This is for the IIlockable interface.  This will be the ID of the content Data model.
        /// </summary>
        public string Id
        {
            get { return _controller?.ContentDataController?.ContentDataModel?.ContentId; }
        }

        /// <summary>
        /// overlay that will appear if someone is editing the text, and someone else tries to edit it.
        /// should appear when text box is focused.
        /// </summary>
        private RectangleUIElement _overlay;

        /// <summary>
        /// if someone is editing the textbox, this will show.
        /// </summary>
        private TextboxUIElement _isEditing;

        /// <summary>
        /// button to toggle th current display state of this content
        /// </summary>
        private ButtonUIElement _toggleDisplayButton;

        /// <summary>
        /// the markdown viewing box;
        /// </summary>
        private MarkdownConvertingTextbox _markdownBox;

        public DetailViewTextContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;
            this.Register(false);

            //create the markdown box
            _markdownBox = new MarkdownConvertingTextbox(this, resourceCreator)
            {
                BorderWidth = 1,
                BorderColor = Constants.DARK_BLUE,
                Background = Colors.White,
                Text = _controller.ContentDataController.ContentDataModel.Data
            };
            AddChild(_markdownBox);

            // create the main textbox
            _mainTextBox = new ScrollableTextboxUIElement(this, resourceCreator, true, true)
            {
                BorderWidth = 1,
                BorderColor = Constants.DARK_BLUE,
                Background = Colors.White,
                PlaceHolderText = "Enter text here...",
                Text = _controller.ContentDataController.ContentDataModel.Data
            };
            AddChild(_mainTextBox);

            // overlay
            _overlay = new RectangleUIElement(this, resourceCreator)
            {
                Background = Constants.LIGHT_BLUE_TRANSLUCENT,
                IsFocusable = false
            };
            AddChild(_overlay);
            _overlay.IsVisible = false;

            //button to toggle display
            _toggleDisplayButton = new RectangleButtonUIElement(this, resourceCreator, text: "Toggle Display");
            _toggleDisplayButton.Tapped += ToggleDisplayButtonOnTapped;
            AddChild(_toggleDisplayButton);

            // is editing text box
            _isEditing = new TextboxUIElement(this, resourceCreator)
            {
                Text = "Someone is editing.",
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                Background = Colors.Transparent,
                TextColor = Constants.RED
            };
            AddChild(_isEditing);
            _isEditing.IsVisible = false;

            HideOverlay();

            // add the main textbox to a new layout manager
            _mainTextboxLayoutManager = new StackLayoutManager()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                StackAlignment = StackAlignment.Horizontal
            };
            _mainTextboxLayoutManager.AddElement(_markdownBox);
            _mainTextboxLayoutManager.AddElement(_mainTextBox);

            // event for when the controllers text changes
            _controller.ContentDataController.ContentDataUpdated += LibraryElementControllerOnContentChanged;
            _mainTextBox.TextChanged += _mainTextBox_TextChanged;

            _mainTextBox.OnFocusGained += MainTextBoxOnOnFocusGained;
            _mainTextBox.OnFocusLost += MainTextBoxOnOnFocusLost;
        }

        /// <summary>
        /// event handler for whenever the toggle display button is pressed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ToggleDisplayButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ToggleState();
            IsDirty = true;
        }

        /// <summary>
        /// this override method should simply load the markdown box
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            await _markdownBox.Load();
            await base.Load();
        }

        private void MainTextBoxOnOnFocusLost(BaseRenderItem item)
        {
            if (this.HasLock())
            {
                this.ReturnLock();
            }
            HideOverlay();
        }

        private void HideOverlay()
        {
            _overlay.IsVisible = false;
            _mainTextBox.IsHitTestVisible = true;
        }

        private void ShowOverlay()
        {
            _overlay.IsVisible = true;
            _mainTextBox.IsHitTestVisible = false;
            SessionController.Instance.FocusManager.ClearFocus();
            _mainTextBox.OnFocusLost -= MainTextBoxOnOnFocusLost;
            _mainTextBox.LostFocus();
            _mainTextBox.OnFocusLost += MainTextBoxOnOnFocusLost;
        }

        private void MainTextBoxOnOnFocusGained(BaseRenderItem item)
        {
            this.GetLock();
            if (this.GetLockOwner() != null)
            {
                if (!this.GetLockOwner().IsLocalUser())
                {
                    ShowOverlay();
                }
            }
            else
            {
                HideOverlay();
            }
        }

        /// <summary>
        /// Event handler fired whenever the editable text box updates its text
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void _mainTextBox_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            _controller.ContentDataController.ContentDataUpdated -= LibraryElementControllerOnContentChanged;
            _controller.ContentDataController.SetData(text);
            _controller.ContentDataController.ContentDataUpdated += LibraryElementControllerOnContentChanged;

            _markdownBox.Text = text;
        }

        /// <summary>
        /// Event handler fired whenever the content's text is changed, most likely from another client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibraryElementControllerOnContentChanged(object sender, string contentData)
        {
            _mainTextBox.TextChanged -= _mainTextBox_TextChanged;
            _mainTextBox.Text = contentData;
            _mainTextBox.TextChanged += _mainTextBox_TextChanged;

            _markdownBox.Text = contentData;
        }

        public override void Dispose()
        {
            _controller.ContentDataController.ContentDataUpdated -= LibraryElementControllerOnContentChanged;
            _mainTextBox.TextChanged -= _mainTextBox_TextChanged;
            _mainTextBox.OnFocusGained -= MainTextBoxOnOnFocusGained;
            _mainTextBox.OnFocusLost -= MainTextBoxOnOnFocusLost;
            this.UnRegister();
            base.Dispose();
        }

        /// <summary>
        /// Method to set the current display state of the text content.
        /// It will update the stack layout manager
        /// </summary>
        /// <param name="state"></param>
        private void SetTextDisplayState(DetailViewTextDisplay state)
        {
            if (state == _displayState)
            {
                return;
            }
            switch (_displayState) //switch statement to remoev the old elements from the layout manager
            {
                case DetailViewTextDisplay.Markdown:
                    _mainTextboxLayoutManager.Remove(_markdownBox);
                    break;
                case DetailViewTextDisplay.MarkdownAndPlaintext:
                    _mainTextboxLayoutManager.Remove(_markdownBox);
                    _mainTextboxLayoutManager.Remove(_mainTextBox);
                    break;
                case DetailViewTextDisplay.Plaintext:
                    _mainTextboxLayoutManager.Remove(_mainTextBox);
                    break;
            }
            _displayState = state;
            switch (_displayState) //switch statement to remoev the old elements from the layout manager
            {
                case DetailViewTextDisplay.Markdown:
                    _mainTextboxLayoutManager.AddElement(_markdownBox);
                    break;
                case DetailViewTextDisplay.MarkdownAndPlaintext:
                    _mainTextboxLayoutManager.AddElement(_markdownBox);
                    _mainTextboxLayoutManager.AddElement(_mainTextBox);
                    break;
                case DetailViewTextDisplay.Plaintext:
                    _mainTextboxLayoutManager.AddElement(_mainTextBox);
                    break;
            }
            _markdownBox.IsVisible = _displayState != DetailViewTextDisplay.Plaintext;
            _mainTextBox.IsVisible = _displayState != DetailViewTextDisplay.Markdown;
        }

        /// <summary>
        /// public method to toggle the current state of the display
        /// </summary>
        public void ToggleState()
        {
            DetailViewTextDisplay state = _displayState;
            switch (_displayState)
            {
                case DetailViewTextDisplay.Markdown:
                    state = DetailViewTextDisplay.MarkdownAndPlaintext;
                    break;
                case DetailViewTextDisplay.MarkdownAndPlaintext:
                    state = DetailViewTextDisplay.Plaintext;
                    break;
                case DetailViewTextDisplay.Plaintext:
                    state = DetailViewTextDisplay.Markdown;
                    break;
            }
            SetTextDisplayState(state);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _mainTextboxLayoutManager.SetSize(Width, Height);
            _mainTextboxLayoutManager.ItemWidth = _displayState == DetailViewTextDisplay.MarkdownAndPlaintext ? Width / 2 : Width;
            _mainTextboxLayoutManager.ArrangeItems();
            _overlay.Height = _mainTextBox.Height;
            _overlay.Width = _mainTextBox.Width;
            _overlay.Transform.LocalPosition = _mainTextBox.Transform.LocalPosition;
            _isEditing.Width = _mainTextBox.Width;
            _isEditing.Height = 20;
            _isEditing.Transform.LocalPosition = new Vector2(_mainTextBox.Transform.LocalX, _mainTextBox.Transform.LocalY + _mainTextBox.Height + 10);


            _toggleDisplayButton.Transform.LocalPosition = new Vector2((Width - BorderWidth * 2 - _toggleDisplayButton.Width)/2,_isEditing.Transform.LocalY + 10 + _isEditing.Height);
            base.Update(parentLocalToScreenTransform);
        }

        public void LockChanged(object sender, NetworkUser currentUser)
        {
            if (currentUser != null)
            {
                if (!currentUser.IsLocalUser())
                {
                    _isEditing.IsVisible = true;
                }
            }
            else
            {
                HideOverlay();
                _isEditing.IsVisible = false;
            }
        }
    }
}
