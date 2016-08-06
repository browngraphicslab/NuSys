using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{

    public class ElementRenderItem : BaseRenderItem, I2dTransformable
    {
        private ElementViewModel _vm;
        private CanvasTextLayout _textLayout;

        public ElementViewModel ViewModel => _vm;

        public ElementRenderItem(ElementViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) :base(parent, resourceCreator)
        {
            _vm = vm;
            T = Matrix3x2.CreateTranslation((float)_vm.X, (float)_vm.Y);
            _vm.Controller.PositionChanged += ControllerOnPositionChanged;
        }

        private void ControllerOnPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            T = Matrix3x2.CreateTranslation((float) d, (float) d1);
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
        }

        public override void Update()
        {
            if (!IsDirty)
                return;
            var format = new CanvasTextFormat { FontSize = 12f, WordWrapping = CanvasWordWrapping.NoWrap };
            _textLayout = new CanvasTextLayout(ResourceCreator, _vm.Title, format, 0.0f, 0.0f);

            IsDirty = false;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_textLayout == null)
                return;

            var to = Matrix3x2.CreateTranslation(new Vector2((float)_vm.X, (float)(_vm.Y - 30)));
            var top = Matrix3x2.Identity;
            Matrix3x2.Invert(to, out top);

            var oldTransform = ds.Transform;
            var sp = Matrix3x2.Identity;
            Matrix3x2.Invert(NuSysRenderer.Instance.InitialCollection.S, out sp);
            var tt = Matrix3x2.CreateTranslation(0, -30);
            var newTransform = tt * top * sp * to * ds.Transform;

            ds.Transform = newTransform;
          //  ds.DrawTextLayout(_textLayout, new Vector2((float)_vm.X, (float)(_vm.Y - 20)), Colors.Black);
            ds.Transform = oldTransform;
        }

        public override bool HitTest(Vector2 point)
        {
            var rect = new Rect
            {
                X = _vm.X,
                Y = _vm.Y - 20 - _textLayout.DrawBounds.Height,
                Width = _vm.Width,
                Height = _vm.Height+20 + _textLayout.DrawBounds.Height
            };

            return rect.Contains(new Point(point.X, point.Y));
        }

        public virtual bool HitTestTitle(Vector2 point)
        {
            var rect = new Rect
            {
                X = _vm.X,
                Y = _vm.Y - 20 - _textLayout.DrawBounds.Height,
                Width = _textLayout.DrawBounds.Width + 20,
                Height = _textLayout.DrawBounds.Height + 20
            };

            return rect.Contains(new Point(point.X, point.Y));
        }
    }
}
