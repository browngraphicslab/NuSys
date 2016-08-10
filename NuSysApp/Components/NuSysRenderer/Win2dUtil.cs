using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class Win2dUtil
    {

        public static Matrix3x2 Invert(Matrix3x2 m)
        {
            var inv = Matrix3x2.Identity;
            Matrix3x2.Invert(m, out inv);
            return inv;
        }

        public static Rect TransformRect(Rect rect, Matrix3x2 transform)
        {
            var tl = Vector2.Transform(new Vector2((float)rect.X, (float)rect.Y), transform);
            var tr = Vector2.Transform(new Vector2((float)rect.X + (float)rect.Width, (float)rect.Y + (float)rect.Height), transform);
            return new Rect(tl.X, tl.Y, tr.X-tl.X,tr.Y-tl.Y);
        }

    }
}
