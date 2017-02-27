using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace NuSysApp
{
    /// <summary>
    /// encapsulates the args passed in a drag event
    /// </summary>
    public class DragEventArgs : GestureEventArgs
    {
        public Vector2 StartPoint { get; private set; }

        public Vector2 EndPoint { get; private set; }

        public DragEventArgs(Vector2 pointerStartPoint)
        {
            StartPoint = pointerStartPoint;
            EndPoint = pointerStartPoint;
            CurrentState = GestureState.Began;
        }

        public Vector2 Translation => EndPoint - StartPoint;

        public void Update(Vector2 pointerCurrentPoint)
        {
            EndPoint = pointerCurrentPoint;
            CurrentState = GestureState.Changed;
        }

        public void Complete(Vector2 pointerCurrentPoint)
        {
            EndPoint = pointerCurrentPoint;
            CurrentState = GestureState.Ended;
        }
    }

    
}
