using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

    }
}
