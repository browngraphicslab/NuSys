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
        
        private CanvasGeometry _triangle;

        private EllipseUIElement _cornerCircle;

        public NodeResizerRenderItem(BaseRenderItem parent, CanvasAnimatedControl resourceCreator, ResizerPosition position) : base(parent, resourceCreator)
        {
            ResizerSize = 30f;
            Position = position;


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

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            _triangle?.Dispose();
            _triangle = null;
            base.Dispose();
        }

        public override async Task Load()
        {
            switch (Position)// Make the polygon correct for whichever corner we're in
            {
                case ResizerPosition.TopLeft:

                    _cornerCircle.Transform.LocalPosition = new Vector2(-_cornerCircle.Width/2, -_cornerCircle.Height / 2);

                    _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                        new Vector2(0, 0),
                        new Vector2(0, ResizerSize),
                        new Vector2(ResizerSize, 0),
                        new Vector2(0, 0)
                    });
                    break;
                case ResizerPosition.TopRight:
                    _cornerCircle.Transform.LocalPosition = new Vector2(ResizerSize - _cornerCircle.Width/2, -_cornerCircle.Height/2);


                    _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                        new Vector2(0, 0),
                        new Vector2(ResizerSize, ResizerSize),
                        new Vector2(ResizerSize, 0),
                        new Vector2(0, 0)
                    });
                    break;
                case ResizerPosition.BottomLeft:
                    _cornerCircle.Transform.LocalPosition = new Vector2(-_cornerCircle.Width/2, ResizerSize - _cornerCircle.Height/2);

                    _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                        new Vector2(0, 0),
                        new Vector2(0, ResizerSize),
                        new Vector2(ResizerSize, ResizerSize),
                        new Vector2(0, 0)
                    });
                    break;
                case ResizerPosition.BottomRight:
                    _cornerCircle.Transform.LocalPosition = new Vector2(ResizerSize - _cornerCircle.Width/2, ResizerSize - _cornerCircle.Height/2);

                    _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                        new Vector2(0, ResizerSize),
                        new Vector2(ResizerSize, ResizerSize),
                        new Vector2(ResizerSize, 0),
                        new Vector2(0, ResizerSize)
                    });
                    break;
                default:
                    break;
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (_triangle != null)
                ds.FillGeometry(_triangle, new Vector2(0, 0), Colors.Transparent);

            ds.Transform = orgTransform;


            base.Draw(ds);
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
