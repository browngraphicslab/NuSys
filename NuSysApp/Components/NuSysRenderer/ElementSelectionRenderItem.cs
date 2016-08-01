using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;


namespace NuSysApp
{
    public class ElementSelectionRenderItem : BaseRenderItem
    {
        private Rect _rect;
        private FreeFormViewerViewModel _vm;
        private bool _isVisible;
        private List<ISelectable> _selectedItems = new List<ISelectable>(); 

        public ElementSelectionRenderItem(FreeFormViewerViewModel vm, CanvasAnimatedControl resourceCreator) : base(resourceCreator)
        {
            _vm = vm;
            _vm.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object source)
        {
            foreach (var selectedItem in _selectedItems)
            {
                var elem = (ElementViewModel)selectedItem;
                elem.Controller.PositionChanged -= OnSelectedItemPositionChanged;
                elem.Controller.SizeChanged -= OnSelectedItemSizeChanged;
            }

            _selectedItems = _vm.Selections.ToList();
            foreach (var selectedItem in _selectedItems)
            {
                var elem = (ElementViewModel) selectedItem;
                elem.Controller.PositionChanged += OnSelectedItemPositionChanged;
                elem.Controller.SizeChanged += OnSelectedItemSizeChanged;
            }

            IsDirty = true;
        }

        private void OnSelectedItemSizeChanged(object source, double width, double height)
        {
            IsDirty = true;
        }

        private void OnSelectedItemPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            IsDirty = true;
        }

        public override void Update()
        {
            if (!IsDirty)
                return;

            base.Update();

            if (_vm.Selections.Count == 0)
            {
                _isVisible = false;
                return;
            }

            var bbs = _vm.Selections.OfType<ElementViewModel>().Select(elem => new Rect(elem.X, elem.Y, elem.Width, elem.Height)).ToList();
            _rect = GetBoundingRect(bbs);

            IsDirty = false;
            _isVisible = true;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (!_isVisible)
                return;

            base.Draw(ds);
            
 
            var old = ds.Transform;

            var tl = Vector2.Transform(new Vector2((float)_rect.X, (float)_rect.Y), old);
            var tr = Vector2.Transform(new Vector2((float)(_rect.X+_rect.Width), (float)(_rect.Y + _rect.Height)), old);
           
            var rect = new Rect(tl.X, tl.Y, tr.X - tl.X, tr.Y - tl.Y);
            ds.Transform = Matrix3x2.Identity;

            var margin = 15 * ResourceCreator.DpiScale;
            rect.X -= margin;
            rect.Y -= margin;
            rect.Width += margin * 2;
            rect.Height += margin * 2;

            ds.DrawRectangle(rect, Colors.SlateGray, 3f, new CanvasStrokeStyle { DashCap = CanvasCapStyle.Flat, DashStyle = CanvasDashStyle.Dash, DashOffset = 10f });
            ds.Transform = old;
        }

        private Rect GetBoundingRect(List<Rect> rects )
        {
            var minX = double.PositiveInfinity;
            var  minY = double.PositiveInfinity;
            var maxW = double.NegativeInfinity;
            var maxH = double.NegativeInfinity;
            foreach (var rect in rects)
            {
                minX = rect.X < minX ? rect.X : minX;
                minY = rect.Y < minY ? rect.Y : minY;
                maxW = rect.X + rect.Width > maxW ? rect.X + rect.Width : maxW;
                maxH = rect.Y + rect.Height > maxH ? rect.Y + rect.Height : maxH;
            }
            return new Rect(minX, minY, maxW-minX, maxH-minY);

        }
    }
}
