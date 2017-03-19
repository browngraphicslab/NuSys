using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ManipulationEventArgs : GestureEventArgs
    {

        public Vector2 CurrentFocus { get; set; }

        public Vector2 PreviousFocus { get; set; }


        public Vector2 Translation => CurrentFocus - PreviousFocus;

        public float Angle { get; set; }
        public float ScaleDelta { get; set; }
        public Vector2 Focus { get; set; }

    }
}
