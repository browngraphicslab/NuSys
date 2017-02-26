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

        public DragEventArgs(Vector2 pointerStartPoint)
        {
            StartPoint = pointerStartPoint;
        }

        public float Translation { get; private set; }

    }
}
