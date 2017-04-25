using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class RadialMenuButtonContainer
    {
        public String BitMapURI;
        public NusysConstants.ElementType Type;
        public Action<Vector2> Action;

        public RadialMenuButtonContainer(String bitMapURI, NusysConstants.ElementType type, Action<Vector2> action)
        {
            BitMapURI = bitMapURI;
            Type = type;
            Action = action;
        }
    }
}
