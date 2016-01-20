using System;

namespace NuSysApp
{
    public class LineSegment
    {
        public Vector2 End;
        public Vector2 Start;

        public LineSegment()
        {
        }

        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public bool Intersects(LineSegment other, out Vector2 intersectionPoint)
        {
            // algorithm based on
            // http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect

            // TODO: remove redudant vector operations for performance optimization 

            intersectionPoint = new Vector2();

            Vector2 p = Start;
            Vector2 q = other.Start;
            Vector2 r = End - Start;
            Vector2 s = other.End - other.Start;

            double t = (q - p).Cross(s/r.Cross(s));
            double u = (q - p).Cross(r/r.Cross(s));

            if (Math.Abs(r.Cross(s)) < 0.001 && Math.Abs((q - p).Cross(r)) < 0.001)
            {
                return false;
            }

            if (Math.Abs(r.Cross(s)) < 0.001 && Math.Abs((q - p).Cross(r)) < 0.001 &&
                !((q - p)*r >= 0 && (q - p)*r <= r*r) &&
                !((p - q)*s >= 0 && (p - q)*s <= s*s)
                )
            {
                return false;
            }

            if (Math.Abs(r.Cross(s)) < 0.001 && Math.Abs((q - p).Cross(r)) > 0.001)
            {
                return false;
            }

            if ((Math.Abs(r.Cross(s)) > 0.001) && t > 0 && t <= 1 && u > 0 && u <= 1)
            {
                intersectionPoint = p + (r*t);
                return true;
            }

            return false;
        }
    }
}