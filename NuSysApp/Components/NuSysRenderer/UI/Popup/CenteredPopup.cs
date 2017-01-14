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
    /// <summary>
    /// a popup that appears centered in the middle of the screen.
    /// for informative purposes: ex. "you cannot put a private element on a public collection"
    /// </summary>
    public class CenteredPopup : PopupUIElement
    {
        /// <summary>
        /// the notification message that the user will be alerted with.
        /// </summary>
        private TextboxUIElement _message;

        public CenteredPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, string message = "") : base(parent, resourceCreator)
        {
            Background = Colors.White;
            BorderWidth = 1;
            BorderColor = Constants.DARK_BLUE;
            Height = 200;
            Width = 300;
            SetNotDismissable(Canvas, "OK");
            MakeMandatoryDismissButton(Canvas);
            _message = new TextboxUIElement(this, Canvas);
            _message.Width = Width - 20;
            _message.Text = message;
            _message.FontFamily = UIDefaults.FontFamily;
            _message.Background = Colors.White;
            _message.TextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _message.Transform.LocalPosition = new Vector2(this.Width/2 - _message.Width/2, 10);
            AddChild(_message);

            Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width / 2 - Width/2, SessionController.Instance.NuSessionView.Height / 2 - Height/2);
        }
    }
}
