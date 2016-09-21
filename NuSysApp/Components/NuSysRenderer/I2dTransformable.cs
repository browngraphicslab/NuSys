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

        Matrix3x2 T { get; }
        Matrix3x2 S { get; }
        Matrix3x2 C { get; }

        void Update(Matrix3x2 parentTransform);
        Vector2 LocalPosition { get; set; }
        Vector2 LocalScaleCenter { get; set; }
        Vector2 LocalScale { get; set; }
    }

}
