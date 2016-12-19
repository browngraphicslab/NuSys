using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{

    public class BarChartElement<T> : RectangleUIElement
    {
        public T Item
        {
            set;get;

        }
        public double Value
        { set; get; }

    }
}
