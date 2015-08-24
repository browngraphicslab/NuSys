using System;

namespace NuSysApp
{
    public class LocationUpdateEventArgs: SuperEventArgs
    {
        
        public LocationUpdateEventArgs(string text, double x, double y):base(text)
        {
            X = x;
            Y = y;
        }

        public double X { get; }

        public double Y { get; }
    }
}
