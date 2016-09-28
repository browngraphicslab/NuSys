using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using NuSysApp;

namespace NuSysAppp
{
    public class CreateNewBaseWindowRenderItemArgs
    {
        public CreateNewBaseWindowRenderItemArgs()
        {
            
        }

#region behavior booleans
        public bool IsSnappable;

        public List<WindowBaseRenderItem.SnapPosition> SnappablePositions;

        public bool IsResizable;

        public List<WindowResizerBorderRenderItem.ResizerBorderPosition> ResizableDirections;

        public bool IsDraggable;
#endregion behavior booleans

#region appearance values
        public Color Foreground;

        public Color Background;

        public Color ResizerColor;

        public Color TopBarColor;

        public float ResizerSize;

        public float TopBarSize;

        public float MinHeight;

        public float MaxHeight;

        public float MinWidth;

        public float MaxWidth;
#endregion appearance values

    }
}
