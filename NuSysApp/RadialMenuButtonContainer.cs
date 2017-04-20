using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class RadialMenuButtonContainer
    {
        public String BitMapURI;
        public NusysConstants.ElementType Type;

        public RadialMenuButtonContainer(String bitMapURI, NusysConstants.ElementType type)
        {
            BitMapURI = bitMapURI;
            Type = type;
        }
    }
}
