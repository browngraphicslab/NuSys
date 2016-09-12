using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
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
        private WrapRenderItem _tagRenderItem;

        public ElementViewModel ViewModel => _vm;
        private bool _needsTitleUpdate = true;
        private CanvasTextFormat _format;

        public ElementRenderItem(ElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(parent, resourceCreator)
        {
            _vm = vm;
            if (_vm != null) { 
                T = Matrix3x2.CreateTranslation((float)_vm.X, (float)_vm.Y);
                _vm.Controller.PositionChanged += ControllerOnPositionChanged;
                _vm.Controller.SizeChanged += ControllerOnSizeChanged;
                _vm.Controller.LibraryElementController.TitleChanged += LibraryElementControllerOnTitleChanged;
                _tagRenderItem = new WrapRenderItem(_vm.Width, parent, resourceCreator);
                _vm.Tags.CollectionChanged += TagsOnCollectionChanged;

                _format = new CanvasTextFormat
                {
                    FontSize = 17f,
                    WordWrapping = CanvasWordWrapping.Wrap,
                    HorizontalAlignment = CanvasHorizontalAlignment.Center,
                    FontFamily = "/Assets/fonts/freightsans.ttf#FreightSans BookSC"
                };

                foreach (var tag in _vm.Tags)
                {
                    _tagRenderItem.AddItem(new TagRenderItem(tag, Parent, ResourceCreator));
                }
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _tagRenderItem.Dispose();
            _tagRenderItem = null;
            _vm.Controller.PositionChanged -= ControllerOnPositionChanged;
            _vm.Controller.SizeChanged -= ControllerOnSizeChanged;
            if (_vm.Controller?.LibraryElementController != null)
            {
                _vm.Controller.LibraryElementController.TitleChanged -= LibraryElementControllerOnTitleChanged;
            }
            _vm = null;
            _textLayout.Dispose();
            _textLayout = null;
            base.Dispose();
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            _tagRenderItem.MaxWidth = width;
        }

        private void TagsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _tagRenderItem.Items.Clear();
                return;
            }

            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    _tagRenderItem.RemoveItem(_tagRenderItem.Items.Where( i => ((TagRenderItem)i).Text == oldItem ).First());
                }
            }

            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    _tagRenderItem.AddItem(new TagRenderItem(newItem.ToString(), Parent, ResourceCreator));
                }
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



        public override void Update()
        {
            if (IsDisposed)
                return;

            base.Update();
            if (_tagRenderItem != null)
                _tagRenderItem.Update();


            if (!_needsTitleUpdate && _vm != null)
                return;

            if (_vm != null)
                _textLayout = new CanvasTextLayout(ResourceCreator, _vm.Title, _format, 200, 0.0f);
            _needsTitleUpdate = false;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            if (_textLayout == null)
            {
                IsDirty = true;
                return;
            }
            var oldTransform = ds.Transform;

            ds.Transform = Matrix3x2.CreateTranslation(0, (float)_vm.Height + 10f) * GetTransform()*oldTransform;
            _tagRenderItem.Draw(ds);

            _transform = oldTransform;
            
            var sp = Vector2.Transform(new Vector2((float) _vm.X, (float) (_vm.Y)), _transform);
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height)), _transform);
            
            ds.Transform = Matrix3x2.Identity;
            ds.DrawTextLayout(_textLayout, new Vector2(sp.X + (spr.X-sp.X - 200f)/2f, sp.Y - (float)_textLayout.DrawBounds.Height-18), Colors.Black);

            ds.Transform = oldTransform;
            base.Draw(ds);
            ds.Transform = oldTransform;

        }

        public Rect GetScreenBoundingRect()
        {
            if (_textLayout == null)
                return new Rect();

            _transform = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetTransformUntil(this);
            var sp = Vector2.Transform(new Vector2((float)_vm.X, (float)(_vm.Y)), _transform);
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height)), _transform);
            var titlePos = new Vector2(sp.X + (spr.X - sp.X - (float)_textLayout.DrawBounds.Width) /2f, sp.Y - (float) _textLayout.DrawBounds.Height - 10);
            var tagsMeasurement = _tagRenderItem.GetMeasure();
            var rect = new Rect
            {
                X = Math.Min(sp.X, titlePos.X),
                Y = Math.Min(sp.Y, titlePos.Y),
                Width = Math.Max(_textLayout.DrawBounds.Width, spr.X - sp.X),
                Height = spr.Y- titlePos.Y + tagsMeasurement.Height + 10
            };
            return rect;
        }

        public Vector2 GetCenterOnScreen()
        {
            var bb = GetScreenBoundingRect();
            return new Vector2((float)(bb.X + bb.Width/2), (float)(bb.Y + bb.Height/2));
        }

        public override BaseRenderItem HitTest(Vector2 point)
        {
            var rect = new Rect
            {
                X = _vm.X,
                Y = _vm.Y,
                Width = _vm.Width,
                Height = _vm.Height
            };

            return rect.Contains(new Point(point.X, point.Y)) ? this : null;
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
