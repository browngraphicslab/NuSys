using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    class WindowTopBarRenderItem : InteractiveBaseRenderItem
    {

        /// <summary>
        ///  The canvas the WindowTopBarRenderItem is drawn on
        /// </summary>
        private CanvasAnimatedControl _canvas;

        public WindowTopBarRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the canvas equal to the passed in resourceCreator
            _canvas = resourceCreator as CanvasAnimatedControl;
            Debug.Assert(_canvas != null, "The passed in canvas should be an CanvasAnimatedControl if not add support for other types here");
        }
    }
}
