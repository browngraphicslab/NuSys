using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class UIElementRenderItem : InteractiveBaseRenderItem
    {
        public enum Alignment
        {
            Right, Left, Center
        }

        private Color _background;
        private double _width;
        private double _height;
        private Alignment _alignment;
        private int _padding;
        private int _margin;

        /// <summary>
        /// UIRenderElement class that will update layout of children when parent is resized
        /// Classes that should extend: buttons, panels, grids, lists, etc.
        /// 
        /// Also will have basic properties for things like background color, foreground color, stroke, etc. 
        /// alignment properties
        /// 
        /// Can contain other UI elements
        /// 
        /// NOTE: UIElementRenderItem is an abstract class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public UIElementRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, double width, double height)
            : base(parent, resourceCreator)
        {
            _background = Colors.LightSlateGray;
            _width = width;
            _height = height;
            _alignment = Alignment.Left;
            _padding = 0;
            _margin = 0;
        }

        public Color Background
        {
            get { return _background; }
            set { _background = value; }
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public Alignment AlignContent
        {
            set { _alignment = value; }
        }

        public int Padding
        {
            set { _padding = value; }
        }

        public int Margin
        {
            set { _margin = value; }
        }

        /// <summary>
        /// draw the uielement - override this in the children classes
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            UpdateLayout();
            //draw children with call to base
            base.Draw(ds);
        }

        /// <summary>
        /// update layout of children elements - call this within draw?
        /// </summary>
        public void UpdateLayout()
        {

        }
    }
}
