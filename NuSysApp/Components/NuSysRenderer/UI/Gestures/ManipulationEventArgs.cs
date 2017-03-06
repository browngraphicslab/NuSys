using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class ManipulationEventArgs : GestureEventArgs
    {
        public float Angle { get; set; }
        public float ScaleDelta { get; set; }
        public Vector2 Focus { get; set; }
    }
}
