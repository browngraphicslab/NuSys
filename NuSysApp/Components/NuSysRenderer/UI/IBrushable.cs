using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
    interface IBrushable
    {
        /// <summary>
        /// Converts a brush id to a brush color
        /// </summary>
        Dictionary<string, Color> BrushDictionary { get; set; }

        void DrawBrush();

    }
}
