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
    /// <summary>
    /// Stores button information
    /// </summary>
    public class RadialMenuButtonContainer
    {
        public String BitMapURI; //The URI of the button's image
        public NusysConstants.ElementType Type; //The type of element associated with the button
        public Action<Vector2> Action; //The action to be called when the button is triggered

        public RadialMenuButtonContainer(String bitMapURI, NusysConstants.ElementType type, Action<Vector2> action)
        {
            BitMapURI = bitMapURI;
            Type = type;
            Action = action;
        }
    }
}
