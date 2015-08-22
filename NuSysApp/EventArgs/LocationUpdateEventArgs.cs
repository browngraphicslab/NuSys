using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LocationUpdateEventArgs: EventArgs
    {
        private readonly string _eventInfo;

        public LocationUpdateEventArgs(string text, double x, double y)
        {
            _eventInfo = text;
            X = x;
            Y = y;
        }

        public string GetInfo()
        {
            return _eventInfo;
        }

        public double X { get; }

        public double Y { get; }
    }
}
