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

        private CanvasGeometry _triangle;
        public NodeResizerRenderItem(BaseRenderItem parent, CanvasAnimatedControl resourceCreator, ResizerPosition position) : base(parent, resourceCreator)
        {
            Position = position;
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
                    _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                        new Vector2(0, 0),
                        new Vector2(0, 30),
                        new Vector2(30, 0),
                        new Vector2(0, 0)
                    });
                    break;
                case ResizerPosition.TopRight:
                    _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                        new Vector2(0, 0),
                        new Vector2(30, 30),
                        new Vector2(30, 0),
                        new Vector2(0, 0)
                    });
                    break;
                case ResizerPosition.BottomLeft:
                    _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                        new Vector2(0, 0),
                        new Vector2(0, 30),
                        new Vector2(30, 30),
                        new Vector2(0, 0)
                    });
                    break;
                case ResizerPosition.BottomRight:
                    _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                        new Vector2(0, 30),
                        new Vector2(30, 30),
                        new Vector2(30, 0),
                        new Vector2(0, 30)
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
                ds.FillGeometry(_triangle, new Vector2(0, 0), Colors.Black);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        public override Rect GetLocalBounds()
        {
            return new Rect(0, 0, 30, 30);
        }
    }


}
