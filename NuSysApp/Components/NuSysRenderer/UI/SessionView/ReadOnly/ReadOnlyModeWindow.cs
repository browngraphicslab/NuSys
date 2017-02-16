using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ReadOnlyModeWindow : ResizeableWindowUIElement
    {
        public ReadOnlyModeWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            ShowClosable();

            MinHeight = 300;
            MinWidth = 250;

            KeepAspectRatio = false;
        }

        private void _closeButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            IsVisible = false;
        }
    }
}
