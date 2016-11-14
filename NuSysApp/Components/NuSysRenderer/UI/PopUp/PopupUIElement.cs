using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// Class that pop up elements will extend from.
    /// pop ups are windows that appear over all other ui elements.
    /// usages: things that need user input (setting ACLs), warnings, confirmation boxes
    /// </summary>
    public class PopupUIElement : RectangleUIElement
    {

        /// <summary>
        /// if a popup is dismissable, then it can be clicked out of by clicking anywhere beyond the popup.
        /// non-dismissable popups need some sort of user input in order to be dismissed.
        /// </summary>
        private bool _dismissable;

        public bool Dismissable
        {
            get { return _dismissable; }
        }

        private string _dismissText;
        /// <summary>
        /// if there is a dismiss button, use this to set the text of the button
        /// </summary>
        public string DismissText
        {
            get { return _dismissText; }
            set { _dismissText = value; }
        }

        /// <summary>
        /// dismiss button for popup
        /// </summary>
        private ButtonUIElement _dismissButton;

        private BaseRenderItem _parent;
        /// <summary>
        /// parent element of this popup. 
        /// if there is no parent, then it will show up on top of the whole workspace.
        /// </summary>
        public BaseRenderItem Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// constructor for popup ui element. more information in class header.
        /// initially dismissable is set to true and parent is set to null, this can be changed with the properties Dismissable and Parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public PopupUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _dismissable = true;
            _parent = null;
            _dismissText = "";

            //SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.ScreenPointerPressed +=
            //    CanvasInteractionManager_ClosePopup;
        }

        /// <summary>
        /// this closes the popup if the pointer is not within the popup's bounds, and disposes of the handler for close popup.
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="popup"></param>
        private void CanvasInteractionManager_ClosePopup(CanvasPointer pointer)
        {
            var hittest = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint);
            if (hittest != this && _dismissable)
            {
                DismissPopup();
            }
        }

        /// <summary>
        /// shows popup. usually triggered by some sort of action.
        /// </summary>
        public void ShowPopup()
        {
            this.IsVisible = true;
        }

        /// <summary>
        /// dismisses popup.
        /// for non-dismissable popups, this comes from the dismiss buton.
        /// for dismissable popups, this occurs when the user clicks outside the space.
        /// </summary>
        public void DismissPopup()
        {
            this.IsVisible = false;
            Dispose();
        }

        /// <summary>
        /// if you are setting a popup to have mandatory input, then use this method after you instantiate it.
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <param name="dismissText"></param>
        public void SetNotDismissable(ICanvasResourceCreatorWithDpi resourceCreator, string dismissText = "")
        {
            _dismissable = false;
            MakeMandatoryDismissButton(resourceCreator);
        }

        /// <summary>
        /// makes a button for non-dismissable popups.
        /// </summary>
        private void MakeMandatoryDismissButton(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            if (!_dismissable)
            {
                _dismissButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
                _dismissButton.Width = 100;
                _dismissButton.Height = 30;
                _dismissButton.Background = Constants.color2;
                _dismissButton.BorderWidth = 0;
                _dismissButton.ButtonTextColor = Colors.White;
                _dismissButton.ButtonText = _dismissText;
                _dismissButton.ButtonFontSize = 12;
                _dismissButton.Transform.LocalPosition = new System.Numerics.Vector2(this.Width/2 - _dismissButton.Width/2,
                    this.Height - _dismissButton.Height - 25);
                AddButtonHandlers(_dismissButton);
                this.AddChild(_dismissButton);
            }
        }

        /// <summary>
        /// adds handler to dismiss button
        /// </summary>
        /// <param name="button"></param>
        private void AddButtonHandlers(ButtonUIElement button)
        {
            button.Tapped += DismissButton_OnTapped;
        }

        /// <summary>
        /// handler for the dismiss button that dismisses the button
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void DismissButton_OnTapped(ButtonUIElement item, CanvasPointer pointer)
        {
            DismissPopup();
            Dispose();
        }

        /// <summary>
        /// draws popup. if the popup has no parent, then it will be drawn on top of the freeformviewer for now.
        /// if the popup is not dismissable, then it needs a button in order to dismiss it.
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds) 
        {
            //draw on top of freeform viewer if parent is null
            base.Draw(ds);
        }

        /// <summary>
        /// dispose of handlers
        /// </summary>
        public override void Dispose()
        {
            if (_dismissButton != null)
            {
                _dismissButton.Tapped -= DismissButton_OnTapped;
            }
            SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.ScreenPointerPressed -=
                CanvasInteractionManager_ClosePopup;
            base.Dispose(); 
        }
    }
}
