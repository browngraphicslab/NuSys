using System.Diagnostics;
using System.Numerics;

namespace NuSysApp2
{
    public class LineEquation
    {
        public double A = 0;
        public double B = 0;

        public LineEquation(double a, double b)
        {
            A = a;
            B = b;
        }

        public double GetY(double x)
        {
            return A + B*x;
        }

        public Vector2d GetDirection()
        {
            int x0 = 0;
            double y0 = GetY(x0);
            int x1 = 10;
            double y1 = GetY(x1);
            return (new Vector2d(x1, y1) - new Vector2d(x0, y0)).GetNormalized();
        }
    }
}