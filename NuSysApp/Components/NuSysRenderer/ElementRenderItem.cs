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
        private Matrix3x2 _transform;

        public ElementViewModel ViewModel => _vm;
        private bool _needsTitleUpdate = true;

        public ElementRenderItem(ElementViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) :base(parent, resourceCreator)
        {
            _vm = vm;
            if (_vm != null) { 
                T = Matrix3x2.CreateTranslation((float)_vm.X, (float)_vm.Y);
                _vm.Controller.PositionChanged += ControllerOnPositionChanged;
                _vm.Controller.LibraryElementController.TitleChanged += LibraryElementControllerOnTitleChanged;
            }
        }

        private void LibraryElementControllerOnTitleChanged(object sender, string s)
        {
            _needsTitleUpdate = true;
        }

        private void ControllerOnPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            T = Matrix3x2.CreateTranslation((float) d, (float) d1);
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm.Controller.LibraryElementController.TitleChanged -= LibraryElementControllerOnTitleChanged;
            _vm = null;
        }

        public override void Update()
        {
            base.Update();
            
            if (!_needsTitleUpdate)
                return;
            var format = new CanvasTextFormat { FontSize = 12f, WordWrapping = CanvasWordWrapping.Wrap, HorizontalAlignment = CanvasHorizontalAlignment.Center};
            _textLayout = new CanvasTextLayout(ResourceCreator, _vm.Title, format, 200, 0.0f);
            _needsTitleUpdate = false;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_textLayout == null)
            {
                IsDirty = true;
                return;
            }

            _transform = ds.Transform;
            var oldTransform = ds.Transform;
            var sp = Vector2.Transform(new Vector2((float) _vm.X, (float) (_vm.Y)), _transform);
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height)), _transform);
            
            ds.Transform = Matrix3x2.Identity;
            ds.DrawTextLayout(_textLayout, new Vector2(sp.X + (spr.X-sp.X - 200f)/2f, sp.Y - (float)_textLayout.DrawBounds.Height-10), Colors.Black);
            ds.Transform = oldTransform;
        }

        public Rect GetScreenBoundingRect()
        {
            if (_textLayout == null)
                return new Rect();

            _transform = NuSysRenderer.Instance.GetTransformUntil(this);
            var sp = Vector2.Transform(new Vector2((float)_vm.X, (float)(_vm.Y)), _transform);
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height)), _transform);
            var titlePos = new Vector2(sp.X + (spr.X - sp.X - (float)_textLayout.DrawBounds.Width) /2f, sp.Y - (float) _textLayout.DrawBounds.Height - 10);
            var rect = new Rect
            {
                X = Math.Min(sp.X, titlePos.X),
                Y = Math.Min(sp.Y, titlePos.Y),
                Width = Math.Max(_textLayout.DrawBounds.Width, spr.X - sp.X),
                Height = spr.Y- titlePos.Y
            };
            return rect;
        }

        public override bool HitTest(Vector2 point)
        {
            var rect = new Rect
            {
                X = _vm.X,
                Y = _vm.Y,
                Width = _vm.Width,
                Height = _vm.Height
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
