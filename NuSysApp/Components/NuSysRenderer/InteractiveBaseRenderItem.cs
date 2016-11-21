using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;

namespace NuSysApp
{
    public class InteractiveBaseRenderItem : BaseRenderItem
    {
        public delegate void PointerHandler(InteractiveBaseRenderItem item, CanvasPointer pointer);

        public event PointerHandler Pressed;
        public event PointerHandler Released;
        public event PointerHandler DoubleTapped;
        public event PointerHandler Tapped;
        public event PointerHandler Dragged;

        // Delegate for the KeyPressed event
        public delegate void KeyPressedDelegate(Windows.UI.Core.KeyEventArgs args);
        // Event that fires when a key is pressed on this render item
        public event KeyPressedDelegate KeyPressed;

        // Delegate for the KeyReleased event
        public delegate void KeyReleasedDelegate(Windows.UI.Core.KeyEventArgs args);
        // Event that fires when a key is released on this render item
        public event KeyPressedDelegate KeyReleased;

        public InteractiveBaseRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            
        }
        public virtual void OnPressed(CanvasPointer pointer)
        {
            Pressed?.Invoke(this, pointer);
        }

        public virtual void OnReleased(CanvasPointer pointer)
        {
            Released?.Invoke(this, pointer);
        }

        public virtual void OnDoubleTapped(CanvasPointer pointer)
        {
            DoubleTapped?.Invoke(this, pointer);
        }

        public virtual void OnTapped(CanvasPointer pointer)
        {
            Tapped?.Invoke(this, pointer);
        }

        public virtual void OnDragged(CanvasPointer pointer)
        {
            Dragged?.Invoke(this, pointer);
        }

        // Function fired when key is pressed on this render item
        // Invokes the KeyPressed event if possible
        public virtual void OnKeyPressed(KeyEventArgs e)
        {
            KeyPressed?.Invoke(e);
        }

        // Function fired when key is released on this render item
        // Invokes the KeyReleased event if possible
        public virtual void OnKeyReleased(KeyEventArgs e)
        {
            KeyReleased?.Invoke(e);
        }

        // Partial override of BaseRenderItem GotFocus in order to add KeyPressed event
        public override void GotFocus()
        {
            SessionController.Instance.FocusManager.OnKeyPressed += OnKeyPressed;
            SessionController.Instance.FocusManager.OnKeyReleased += OnKeyReleased;
            base.GotFocus();
        }

        // Partial override of BaseRenderItem LostFocus in order to remove KeyPressed event
        public override void LostFocus()
        {
            SessionController.Instance.FocusManager.OnKeyPressed -= OnKeyPressed;
            SessionController.Instance.FocusManager.OnKeyReleased -= OnKeyReleased;
            base.LostFocus();
        }
    }
}
