using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

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
        /// Event called whenever the popup is dismissed.  
        /// Can be used to remove the UI element from the its parent. 
        /// </summary>
        public event EventHandler<PopupUIElement> Dismissed; 

        /// <summary>
        /// if a popup is dismissable, then it can be clicked out of by clicking anywhere beyond the popup.
        /// non-dismissable popups need some sort of user input in order to be dismissed.
        /// </summary>
        private bool _dismissable;

        /// <summary>
        /// the text set for a dismiss button, if there is one
        /// </summary>
        private string _dismissText;
        /// <summary>
        /// if there is a dismiss button, use this to set the text of the button
        /// </summary>
        public string DismissText
        {
            get { return _dismissText; }
            set { _dismissText = value; }
        }

        public bool Dismissable
        {
            get { return _dismissable;}
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
        /// Its important that the parent you pass in is actually the element you call 'AddChild' on.  
        /// Otherwise There may be a memory leak with this popup if not used correctly.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public PopupUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _dismissable = true;
            _parent = parent;
            _dismissText = "";

            //SessionController.Instance.SessionView.FreeFormViewer.FocusManager.ChangeFocus(this);
            
            OnChildFocusLost += PopupUIElement_OnFocusLost;
            OnFocusLost += PopupUIElement_OnFocusLost;

            //SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.PointerPressed +=
            //    CanvasInteractionManager_ClosePopup;
        }

        public virtual void PopupUIElement_OnFocusLost(BaseRenderItem item)
        {
            if (_dismissable && !ChildHasFocus)
            {
                DismissPopup();
            }
        }



        /// <summary>
        /// this closes the popup if the pointer is not within the popup's bounds, and disposes of the handler for close popup.
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="popup"></param>
        private void CanvasInteractionManager_ClosePopup(CanvasPointer pointer)
        {
            var hittest = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint);
            if (hittest.Parent != this && _dismissable)
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
            //TODO add 'dismissable' logic here so its encapsulated in one place
            this.IsVisible = false;
            Dismissed?.Invoke(this, this);
            Parent?.RemoveChild(this);
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
            DismissText = dismissText;
            MakeMandatoryDismissButton(resourceCreator);
        }

        /// <summary>
        /// makes a button for non-dismissable popups.
        /// </summary>
        protected void MakeMandatoryDismissButton(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            if (!_dismissable)
            {
                _dismissButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.PrimaryStyle, DismissText);
                _dismissButton.Transform.LocalPosition = new System.Numerics.Vector2(this.Width / 2 - _dismissButton.Width / 2,
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
            button.Tapped += DismissButton_Tapped;
        }

        private void DismissButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            DismissPopup();
            Dispose();
        }

        /// <summary>
        /// dispose of handlers
        /// </summary>
        public override void Dispose()
        {
            if (_dismissButton != null)
            {
                _dismissButton.Tapped -= DismissButton_Tapped;
            }
            OnFocusLost -= PopupUIElement_OnFocusLost;
            OnChildFocusLost -= PopupUIElement_OnFocusLost;

            //SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.PointerPressed -=
            //    CanvasInteractionManager_ClosePopup;
            base.Dispose();
        }
    }
}
