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
    public class ConfirmationPopupUIElement : PopupUIElement
    {

        /// <summary>
        /// The message the popup should display to the user
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The text on the confirmation button
        /// </summary>
        public string ConfirmButtonText { get; set; } = "Ok";

        /// <summary>
        ///  The text on the cancel button
        /// </summary>
        public string CancelButtonText { get; set; } = "Cancel";

        /// <summary>
        /// Pointer handler fired when the confirm button is tapped
        /// </summary>
        public PointerHandler OnConfirmTapped { get; }

        /// <summary>
        /// Pointer handler fired when the cancle button is tapped
        /// </summary>
        public PointerHandler OnCancelTapped { get; }

        /// <summary>
        /// Textbox used to display a message to the user
        /// </summary>
        private TextboxUIElement _messageBox;

        /// <summary>
        /// the confirmation button
        /// </summary>
        private ButtonUIElement _confirmButton;

        /// <summary>
        /// The cancel button
        /// </summary>
        private ButtonUIElement _cancelButton;

        /// <summary>
        /// the height of the entire popup
        /// </summary>
        private float popupHeight = 200;

        /// <summary>
        /// the height of the buttons
        /// </summary>
        private float buttonHeight = 30;

        public ConfirmationPopupUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, PointerHandler onConfirmTapped, PointerHandler onCancelTapped) : base(parent, resourceCreator)
        {
            // set ui defaults
            Background = Colors.White;
            BorderWidth = 1;
            BorderColor = Constants.DARK_BLUE;
            Height = 200;
            Width = 300;

            // store passed in parameters
            OnConfirmTapped = onConfirmTapped;
            OnCancelTapped = onCancelTapped;
            Dismissable = false;

            // add two button's and the message box
            _confirmButton = new ButtonUIElement(this, resourceCreator);
            AddChild(_confirmButton);
            _cancelButton = new ButtonUIElement(this, resourceCreator);
            AddChild(_cancelButton);
            _messageBox = new TextboxUIElement(this, ResourceCreator);
            AddChild(_messageBox);

            // add events for the buttons
            _confirmButton.Tapped += _confirmButton_Tapped;
            _cancelButton.Tapped += _cancelButton_Tapped;
        }

        /// <summary>
        /// called when the cancel button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _cancelButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnCancelTapped?.Invoke(this, pointer);
            DismissPopup();
        }

        /// <summary>
        /// Called when the confrim button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _confirmButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnConfirmTapped?.Invoke(this, pointer);
            DismissPopup();
        }

        public override void Dispose()
        {
            _confirmButton.Tapped -= _confirmButton_Tapped;
            _cancelButton.Tapped -= _cancelButton_Tapped;
            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // arrange the ui
            _messageBox.Text = Message;
            _messageBox.Width = 290;
            _messageBox.Height = Height - 15 - buttonHeight;
            _messageBox.TextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _messageBox.TextVerticalAlignment = CanvasVerticalAlignment.Center;
            _messageBox.Transform.LocalPosition = new Vector2(5);

            _confirmButton.Width = (Width - 15) / 2;
            _confirmButton.Height = buttonHeight;
            _confirmButton.Transform.LocalPosition = new Vector2(5, Height - 5  - buttonHeight);
            _confirmButton.ButtonText = ConfirmButtonText;
            _confirmButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _confirmButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;


            _cancelButton.Width = (Width - 15) / 2;
            _cancelButton.Height = buttonHeight;
            _cancelButton.Transform.LocalPosition = new Vector2(10 + (Width - 15) / 2, Height - 5 - buttonHeight);
            _cancelButton.ButtonText = CancelButtonText;
            _cancelButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _cancelButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;


            var vectordiff = Vector2.Transform(new Vector2((float)(SessionController.Instance.ScreenWidth/2 - Width/2), (float)SessionController.Instance.ScreenHeight/2 - Height/2), Transform.ScreenToLocalMatrix);
            Transform.LocalPosition = new Vector2(Transform.LocalX + vectordiff.X, Transform.LocalY + vectordiff.Y);
            base.Update(parentLocalToScreenTransform);
        }
    }
}
