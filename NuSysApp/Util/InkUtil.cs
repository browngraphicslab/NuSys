using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace NuSysApp
{
    public class InkUtil
    {

        public static bool IsPointCloseToStroke(Vector2 p, InkStroke stroke)
        {
            return IsPointCloseToStroke(new Point(p.X, p.Y), stroke);
        }

        public static bool IsPointCloseToStroke(Point p, InkStroke stroke)
        {
            var minDist = double.PositiveInfinity;
            foreach (var inqPoint in stroke.GetInkPoints())
            {
                var dist = Math.Sqrt((p.X - inqPoint.Position.X) * (p.X - inqPoint.Position.X) + (p.Y - inqPoint.Position.Y) * (p.Y - inqPoint.Position.Y));
                if (dist < minDist)
                    minDist = dist;
            }
            return minDist < 100;
        }
    }
}
