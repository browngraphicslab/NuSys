﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Input;

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

        public event KeyEventHandler KeyPressed;

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

        public virtual void OnKeyPressed(KeyRoutedEventArgs e)
        {
            KeyPressed?.Invoke(this, e);
        }
    }
}
