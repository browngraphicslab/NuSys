using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// Textbox UI element class that will automatically register itself with the server and start locking.
    /// This requires an ID to lock to.  
    /// The lock will work such that no two people can have the lock for the GUID at the same time.
    /// Good examples for ID's are LibraryElementID's, ElementModel ID's, and ContentDataModelID's
    /// </summary>
    public class LockableTextBoxUIElement : ScrollableTextboxUIElement, ILockable
    {
        /// <summary>
        /// the rectangle that will overlay the text box when it is locked
        /// </summary>
        private RectangleUIElement _overlay;

        /// <summary>
        /// private version of the UserColorHighlighting bool;
        /// </summary>
        private bool _userColorHighlighting = true;

        /// <summary>
        /// bool representing whether this text box will have its inner border change colors with the current user.
        /// If thiis is set to true, it will take full controll of this UIElement's BorderWidth and BorderColor properties
        /// </summary>
        public bool UserColorHighlighting {
            get { return _userColorHighlighting;}
            set
            {
                _userColorHighlighting = value;
                UpdateBorder();
            }
        }

        /// <summary>
        /// Getter and setter for the interaction-blocking  overlay color
        /// </summary>
        public Color OverlayColor { get; set; } = UIDefaults.ScrollableTextboxOverlayColor;
        

        /// <summary>
        /// Constructor is identical to the Scrollable Text Box's constructor with the addition of the import ID property.
        /// IMPORTANT: IF THE LOCK ID IS INCORRECT IT WON'T REGISTER WITH THE SERVER
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="scrollVert"></param>
        /// <param name="showScrollBar"></param>
        public LockableTextBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            string lockId, bool scrollVert, bool showScrollBar)
            : base(parent, resourceCreator, scrollVert, showScrollBar)
        {
            LockId = lockId;
            _overlay = new RectangleUIElement(this, resourceCreator)
            {
                Background = UIDefaults.ScrollableTextboxOverlayColor,
            };
            _overlay.Transform.LocalPosition = new Vector2(0, 0);
            AddChild(_overlay);

            var overlayTapRecognizer = new TapGestureRecognizer();
            overlayTapRecognizer.OnTapped += OverlayOnTapped;
            _overlay.GestureRecognizers.Add(overlayTapRecognizer);
            OnFocusGained += OnOnFocusGained;
            OnFocusLost += OnOnFocusLost;
            if (!string.IsNullOrEmpty(lockId))
            {
                this.Register(false);
            }
        }

        /// <summary>
        /// event handler for when this text box gains focus
        /// </summary>
        /// <param name="item"></param>
        private void OnOnFocusLost(BaseRenderItem item)
        {
            this.ReturnLock();
        }

        /// <summary>
        /// event handler for when this text box loses focus
        /// </summary>
        /// <param name="item"></param>
        private void OnOnFocusGained(BaseRenderItem item)
        {
            if (!this.HasLock())
            {
                this.GetLock();
            }
        }

        /// <summary>
        /// async task to unregister and then re-register with a new ID.
        /// </summary>
        /// <param name="newLockId"></param>
        /// <returns></returns>
        public void SetNewId(string newLockId)
        {
            Debug.Assert(!string.IsNullOrEmpty(newLockId));
            if (this.IsRegistered())
            {
                this.UnRegister();
            }
            if (!string.IsNullOrEmpty(newLockId))
            {
                LockId = newLockId;
                this.Register(false);
            }
            LockChanged(this, null);
        }

        /// <summary>
        /// this update just keeps the overlay updated as well
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _overlay.Width = Width;
            _overlay.Height = Height;
            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// dispose override removes handlers and unregisters this lockable
        /// </summary>
        public override void Dispose()
        {
            this.UnRegister();
            OnFocusGained -= OnOnFocusGained;
            OnFocusLost -= OnOnFocusLost;
            base.Dispose();
        }

        /// <summary>
        /// event handler fired whenever the overlay is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OverlayOnTapped(TapGestureRecognizer sender, TapEventArgs args)
        {
            if (_overlay.IsVisible)
            {
                _overlay.Background = OverlayColor;
            }
        }

        /// <summary>
        /// The lock id required by the Lockable interface
        /// </summary>
        public string LockId { get; private set; }

        /// <summary>
        /// method called whenever the lock holder for this class's lock id changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="currentUser"></param>
        public void LockChanged(object sender, NetworkUser currentUser)
        {
            if (currentUser == null)
            {
                IsFocusable = true;
                IsHitTestVisible = true;
                _overlay.IsVisible = false;
            }
            else if (currentUser.IsLocalUser())
            {
                IsFocusable = true;
                IsHitTestVisible = true;
                _overlay.IsVisible = false;
            }
            else
            {
                IsFocusable = false;
                IsHitTestVisible = false;
                _overlay.IsVisible = true;
                _overlay.Background = Colors.Transparent;
                SessionController.Instance.FocusManager.ClearFocus();
            }
            
            UpdateBorder();
        }

        /// <summary>
        /// private method to correctly set the border width and color if this text box has the user color highlighting property
        /// </summary>
        private void UpdateBorder()
        {
            if (_userColorHighlighting)
            {
                BorderWidth = UIDefaults.ScrollableTextboxBorderWidth;
                var user = this.GetLockOwner();
                BorderColor = user == null ? Colors.Transparent : user.Color;
            }
        }
    }
}
