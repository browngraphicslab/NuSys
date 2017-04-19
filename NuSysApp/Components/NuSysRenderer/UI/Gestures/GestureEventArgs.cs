using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;

namespace NuSysApp
{
    /// <summary>
    /// Super class for gestures
    /// </summary>
    public class GestureEventArgs
    {
        /// <summary>
        /// The different types of states all gestures support
        /// </summary>
        public enum GestureState
        {
            Began,
            Changed,
            Ended
        }

        /// <summary>
        /// The current state of the gesture
        /// </summary>
        public GestureState CurrentState { get; set; }

        public PointerDeviceType DeviceType { get; set; }
    }
}
