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

        public DetailViewTextContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;
            this.Register(false);

            // create the main textbox
            _mainTextBox = new ScrollableTextboxUIElement(this, resourceCreator, true, true)
            {
                BorderWidth = 1,
                Bordercolor = Constants.DARK_BLUE,
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
                VerticalAlignment = VerticalAlignment.Stretch
            };
            _mainTextboxLayoutManager.AddElement(_mainTextBox);

            // event for when the controllers text changes
            _controller.ContentDataController.ContentDataUpdated += LibraryElementControllerOnContentChanged;
            _mainTextBox.TextChanged += _mainTextBox_TextChanged;

            _mainTextBox.OnFocusGained += MainTextBoxOnOnFocusGained;
            _mainTextBox.OnFocusLost += MainTextBoxOnOnFocusLost;
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

        private void _mainTextBox_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            _controller.ContentDataController.ContentDataUpdated -= LibraryElementControllerOnContentChanged;
            _controller.ContentDataController.SetData(text);
            _controller.ContentDataController.ContentDataUpdated += LibraryElementControllerOnContentChanged;

        }

        private void LibraryElementControllerOnContentChanged(object sender, string e)
        {
            _mainTextBox.TextChanged -= _mainTextBox_TextChanged;
            _mainTextBox.Text = e;
            _mainTextBox.TextChanged += _mainTextBox_TextChanged;

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

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _mainTextboxLayoutManager.SetSize(Width, Height);
            _mainTextboxLayoutManager.ArrangeItems();
            _overlay.Height = _mainTextBox.Height;
            _overlay.Width = _mainTextBox.Width;
            _overlay.Transform.LocalPosition = _mainTextBox.Transform.LocalPosition;
            _isEditing.Width = _mainTextBox.Width;
            _isEditing.Height = 20;
            _isEditing.Transform.LocalPosition = new Vector2(_mainTextBox.Transform.LocalX, _mainTextBox.Transform.LocalY + _mainTextBox.Height + 10);
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
