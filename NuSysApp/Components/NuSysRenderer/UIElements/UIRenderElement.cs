using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class UIRenderElement : InteractiveBaseRenderItem
    {

        private Color _background;
        private double _width;
        private double _height;

        /// <summary>
        /// UIRenderElement class that will update layout of children when parent is resized
        /// Classes that should extend: buttons, panels, grids, lists, etc.
        /// 
        /// Also will have basic properties for things like background color, foreground color, stroke, etc. 
        /// alignment properties
        /// 
        /// Can contain other UI elements
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public UIRenderElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {
            
        }

        public Color Background
        {
            get { return _background; }
            set
            {
                _background = value;
            }
        }

        public void UpdateLayout()
        {
            
        }

    }
}
