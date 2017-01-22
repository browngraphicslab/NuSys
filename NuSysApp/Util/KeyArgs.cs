using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace NuSysApp
{
    /// <summary>
    /// Args class for when keys are pressed or released
    /// </summary>
    public class KeyArgs
    {
        /// <summary>
        /// The key that was pressed or changed
        /// </summary>
        public VirtualKey Key { get; set; }

        /// <summary>
        /// boolean representing if the key was pressed down or not
        /// </summary>
        public bool Pressed { get; set; }
    }
}