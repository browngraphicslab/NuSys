using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface I2dTransformable
    {

        Matrix3x2 T { get; set; }
        Matrix3x2 S { get; set; }
        Matrix3x2 C { get; set; }
    }

}
