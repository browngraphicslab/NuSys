using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using System.Numerics;
using Windows.UI;
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NetTopologySuite.Geometries;

namespace NuSysApp
{
    public class AdornmentRenderItem : BaseRenderItem
    {
        private ElementViewModel _vm;
        private InkStroke _stroke;
        private CanvasGeometry _geometry;

        public AdornmentRenderItem(InkStroke stroke, ICanvasResourceCreator resourceCreator):base(resourceCreator)
        {
            _stroke = stroke;
        }

        public void Dispose()
        {
            _vm = null;
            _stroke = null;
            _geometry.Dispose();
            _geometry = null;
        }

        public override void Update()
        {
            if (!IsDirty)
                return;
            var multipoint = new MultiPoint(_stroke.GetInkPoints().Select(p => new NetTopologySuite.Geometries.Point(p.Position.X, p.Position.Y)).ToArray());
            var ch = multipoint.ConvexHull().Coordinates.Select(p => new Vector2((float)p.X, (float)p.Y)).ToArray();
            _geometry = CanvasGeometry.CreatePolygon(ResourceCreator, ch);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_geometry != null)
                ds.FillGeometry(_geometry, Colors.DarkSeaGreen);
        }
    }
}
