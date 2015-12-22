using System;

namespace NuSysApp
{
    public class WidthHeightUpdateEventArgs : System.EventArgs
    {
     
        public WidthHeightUpdateEventArgs(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double Width { get; }

        public double Height { get; }
    }
}
