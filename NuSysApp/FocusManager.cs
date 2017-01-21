using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    /// <summary>
    /// Manages keyboard input events in order to declare and control which BaseRenderItem has focus
    /// </summary>
    public class FocusManager : IDisposable
    {
        public Boolean InReadOnly { get; set; }

        // BaseRenderItem that is currently in focus
        public BaseRenderItem ActiveFocusElement { get; set; }

        private IEnumerable<BaseRenderItem> _parentsWithFocus;

        // Used to set the pointerpressed event of the canvas to fire the
        // pointer pressed function
        private CanvasInteractionManager _canvasInteractionManager;

        // Used to get the element that the pointer press point lands on
        private CanvasRenderEngine _canvasRenderEngine;

        // Delegate to handle KeyPressed events
        public delegate void KeyPressedDelegate(KeyArgs args);

        // Fired when a key is pressed on the application anywhere
        public event KeyPressedDelegate OnKeyPressed;

        // Delegate to handle KeyPressed events
        public delegate void KeyReleasedDelegate(KeyArgs args);

        // Fired when a key is pressed on the application anywhere
        public event KeyReleasedDelegate OnKeyReleased;



        public FocusManager(CanvasInteractionManager cim, CanvasRenderEngine cre)
        {
            Debug.Assert(cim != null);
            Debug.Assert(cre != null);

            InReadOnly = false;

            _canvasInteractionManager = cim;
            _canvasRenderEngine = cre;

            _parentsWithFocus = new List<BaseRenderItem>();

            _canvasInteractionManager.PointerPressed += _canvasInteractionManager_PointerPressed;
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.KeyDown += FireKeyPressed;
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.KeyUp += FireKeyReleased;
        }

        // Fired whenever a key is pressed on the application - Invokes OnKeyPressed
        private void FireKeyPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (SessionController.Instance.SessionView.FreeFormViewer.Keyboard.Visibility == Visibility.Visible)
            {
                SessionController.Instance.SessionView.FreeFormViewer.Keyboard.LosePseudoFocus();
            }
            OnKeyPressed?.Invoke(new KeyArgs() {Pressed = true, Key = args.VirtualKey});
            if (args.VirtualKey == VirtualKey.Shift)
            {
                SessionController.Instance.ShiftHeld = true;
            }
            if (args.VirtualKey == VirtualKey.CapitalLock)
            {
                SessionController.Instance.CapitalLock = !SessionController.Instance.CapitalLock;
            }


    }

        // Fired whenever a key is released on the application - Invokes OnKeyReleased
        private void FireKeyReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            OnKeyReleased?.Invoke(new KeyArgs() { Pressed = false, Key = args.VirtualKey });
            if (args.VirtualKey == VirtualKey.Shift)
            {
                SessionController.Instance.ShiftHeld = false;
            }
        }

        /// <summary>
        /// Called when a pointer is pressed. Uses CanvasRenderEngine to determine which BaseRenderItem was
        /// clicked and changes the focus to this item.
        /// </summary>
        /// <param name="pointer">Where the pointer was pressed</param>
        private void _canvasInteractionManager_PointerPressed(CanvasPointer pointer)
        {

                BaseRenderItem curr = _canvasRenderEngine.GetRenderItemAt(pointer.CurrentPoint);
                ChangeFocus(curr);
            
        }

        // Clears the focus of the current BaseRenderItem in focus
        public void ClearFocus()
        {
            ActiveFocusElement?.LostFocus();
            ActiveFocusElement = null;
        }

        /// <summary>
        /// Changes the focus to the passed in item. The passed in itme becomes the ActiveFocusElement.
        /// If the passed in item is the ActiveFocusElement, or is not focusable, then nothing happens.
        /// Otherwise the child focus and got focus events are fired correctly
        /// </summary>
        /// <param name="newBaseRenderItem"></param>
        public void ChangeFocus(BaseRenderItem newBaseRenderItem)
        {
            // if we are not in read only, and the newBaseRenderItem is focusable, and the new base render
            // item is not currently focused
            if (newBaseRenderItem.IsFocusable && newBaseRenderItem != ActiveFocusElement)
            {
                var newParentsWithFocus = GetParents(newBaseRenderItem);
                var parentsWhoLostFocus = _parentsWithFocus.Except(newParentsWithFocus);

                foreach (var parent in parentsWhoLostFocus)
                {
                    parent.ChildLostFocus();
                }

                var parentsWhoGainedFocus = newParentsWithFocus.Except(_parentsWithFocus);
                foreach (var parent in parentsWhoGainedFocus)
                {
                    parent.ChildGotFocus();
                }
                _parentsWithFocus = newParentsWithFocus;

                ActiveFocusElement?.LostFocus();
                newBaseRenderItem.GotFocus();
                ActiveFocusElement = newBaseRenderItem;
            }     
        }

        public void ManualFireKeyPressed(KeyArgs args)
        {
            OnKeyPressed?.Invoke(args);

        }

        /// <summary>
        /// Gets all the parents of a base render item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private List<BaseRenderItem> GetParents(BaseRenderItem item)
        {
            List<BaseRenderItem> parents = new List<BaseRenderItem>();
            while (item.Parent != null)
            {
                item = item.Parent;
                parents.Add(item);
            }

            return parents;
        }

        // Prevent memory leaks by disposing of resources
        public void Dispose()
        {
            _canvasInteractionManager.PointerPressed -= _canvasInteractionManager_PointerPressed;
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.KeyDown -= FireKeyPressed;
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.KeyUp -= FireKeyReleased;
        }
    }
}
