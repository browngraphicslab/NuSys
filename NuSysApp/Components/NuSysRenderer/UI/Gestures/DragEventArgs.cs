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

        public Vector2 CurrentPoint { get; private set; }

        private Vector2 lastPoint;

        public DragEventArgs(Vector2 pointerStartPoint)
        {
            StartPoint = pointerStartPoint;
            CurrentPoint = pointerStartPoint;
            lastPoint = pointerStartPoint;
        }

        public Vector2 Translation => CurrentPoint - lastPoint;
        public Vector2 TotalTranslation => CurrentPoint - StartPoint;

        public void Update(Vector2 pointerCurrentPoint)
        {
            lastPoint = CurrentPoint;
            CurrentPoint = pointerCurrentPoint;
        }

        public void Complete(Vector2 pointerCurrentPoint)
        {
            CurrentPoint = pointerCurrentPoint;
        }
    }

    
}
