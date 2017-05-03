using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Foundation;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas.Brushes;

namespace NuSysApp
{
    public class NodeResizerRenderItem : BaseRenderItem
    {

        /// <summary>
        /// Enum representing which corner this resizer is in
        /// </summary>
        public enum ResizerPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public ResizerPosition Position { get; }

        public float ResizerSize { set; get; }
        
        //Circle represents the resizer
        private EllipseUIElement _cornerCircle;

        public NodeResizerRenderItem(BaseRenderItem parent, CanvasAnimatedControl resourceCreator, ResizerPosition position) : base(parent, resourceCreator)
        {
            ResizerSize = 30f;
            Position = position;

            //Set up the circle handle
            _cornerCircle = new EllipseUIElement(this, ResourceCreator)
            {
                Background = Colors.White,
                BorderWidth = 3f,
                BorderColor = Colors.SlateGray,
                Width = 15f,
                Height = 15f
            };
            AddChild(_cornerCircle);
        }



        public override async Task Load()
        {
            switch (Position)// Make the polygon correct for whichever corner we're in
            {
                case ResizerPosition.TopLeft:

                    _cornerCircle.Transform.LocalPosition = new Vector2(-_cornerCircle.Width/2, -_cornerCircle.Height / 2);
                    break;
                case ResizerPosition.TopRight:
                    _cornerCircle.Transform.LocalPosition = new Vector2(ResizerSize - _cornerCircle.Width/2, -_cornerCircle.Height/2);
                    break;
                case ResizerPosition.BottomLeft:
                    _cornerCircle.Transform.LocalPosition = new Vector2(-_cornerCircle.Width/2, ResizerSize - _cornerCircle.Height/2);
                    break;
                case ResizerPosition.BottomRight:
                    _cornerCircle.Transform.LocalPosition = new Vector2(ResizerSize - _cornerCircle.Width/2, ResizerSize - _cornerCircle.Height/2);
                    break;
                default:
                    break;
            }
        }


        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            var cornerHT = _cornerCircle.HitTest(screenPoint);

            if (cornerHT != null)
            {
                return this;
            }
            return base.HitTest(screenPoint);
        }

        public override Rect GetLocalBounds()
        {
            return new Rect(0, 0, 30, 30);
        }
    }


}
