﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TapEventArgs : GestureEventArgs
    {
        public Vector2 Position { get; private set; }

        public TapEventArgs(Vector2 pointerPosition)
        {
            Position = pointerPosition;
            CurrentState = GestureState.Ended;
        }
    }
}
