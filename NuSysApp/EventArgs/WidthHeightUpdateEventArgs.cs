using System;

namespace NuSysApp
{
    public class WidthHeightUpdateEventArgs: SuperEventArgs
    {
        

        public WidthHeightUpdateEventArgs(string text, double width, double height):base(text)
        {
            Width = width;
            Height = height;
        }

        public double Width { get; }

        public double Height { get; }
    }
}
