using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    /// <summary>
    /// Manages keyboard input events in order to declare and control which BaseRenderItem has focus
    /// </summary>
    public class FocusManager : IDisposable
    {
        // BaseRenderItem that is currently in focus
        public BaseRenderItem ActiveFocusElement { get; set; }

        // Used to set the pointerpressed event of the canvas to fire the
        // pointer pressed function
        private CanvasInteractionManager _canvasInteractionManager;

        // Used to get the element that the pointer press point lands on
        private CanvasRenderEngine _canvasRenderEngine;

        // Delegate to handle KeyPressed events
        public delegate void KeyPressedDelegate(Windows.UI.Core.KeyEventArgs args);
        
        // Fired when a key is pressed on the application anywhere
        public event KeyPressedDelegate OnKeyPressed; 

        public FocusManager(CanvasInteractionManager cim, CanvasRenderEngine cre)
        {
            Debug.Assert(cim != null);
            Debug.Assert(cre != null);

            _canvasInteractionManager = cim;
            _canvasRenderEngine = cre;

            _canvasInteractionManager.PointerPressed += _canvasInteractionManager_PointerPressed;
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.KeyDown += FireKeyPressed;
        }

        // Fired whenever a key is pressed on the application - Invokes OnKeyPressed
        private void FireKeyPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            OnKeyPressed?.Invoke(args);    
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

        // Changes the focus from the currently focused item to the one passed in. Sets this as the ActiveFocusElement
        public void ChangeFocus(BaseRenderItem newBaseRenderItem)
        {
            if (newBaseRenderItem.IsFocusable)
            {
                ActiveFocusElement?.LostFocus();
                newBaseRenderItem.GotFocus();
                ActiveFocusElement = newBaseRenderItem;
            }     
        }

        // Prevent memory leaks by disposing of resources
        public void Dispose()
        {
            _canvasInteractionManager.PointerPressed -= _canvasInteractionManager_PointerPressed;
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.KeyDown -= FireKeyPressed;

        }
    }
}
