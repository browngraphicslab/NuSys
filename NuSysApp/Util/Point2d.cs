using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Foundation;

namespace NuSysApp
{
    public class Point2d
    {
        
        public static implicit operator Point(Point2d cpoint)
        {
            return new Point(cpoint.X, cpoint.Y);
        }

        public Point2d(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
