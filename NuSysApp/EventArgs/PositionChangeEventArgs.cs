using System;

namespace NuSysApp
{
    public class PositionChangeEventArgs: System.EventArgs
    {
        
        public PositionChangeEventArgs(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }

        public double Y { get; }
    }
}
