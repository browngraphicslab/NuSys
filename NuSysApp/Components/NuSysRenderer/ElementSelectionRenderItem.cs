using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private Rect _screenRect;
        private FreeFormViewerViewModel _vm;
        private bool _isVisible;
        private List<ElementRenderItem> _selectedItems = new List<ElementRenderItem>();
        private Matrix3x2 _transform;
        public NodeMenuButtonRenderItem BtnDelete;
        public NodeMenuButtonRenderItem BtnPresent;


        public ElementSelectionRenderItem(FreeFormViewerViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) : base(parent, resourceCreator)
        {
            BtnDelete = new NodeMenuButtonRenderItem(parent, resourceCreator);
            BtnPresent = new NodeMenuButtonRenderItem(parent, resourceCreator);


            NuSysRenderer.Instance.Selections.CollectionChanged += SelectionsOnCollectionChanged;

        }

        private void SelectionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
                _selectedItems.Clear();

            if (args.OldItems != null)
            {

                foreach (var newItem in args.OldItems)
                {
                    var item = (ElementRenderItem)newItem;
                    _selectedItems.Remove(item);
                    item.ViewModel.Controller.PositionChanged -= OnSelectedItemPositionChanged;
                    item.ViewModel.Controller.SizeChanged -= OnSelectedItemSizeChanged;
                }
            }

            if (args.NewItems != null) { 
                foreach (var newItem in args.NewItems)
                {
                    var item = (ElementRenderItem) newItem;
                    _selectedItems.Add(item);
                    item.ViewModel.Controller.PositionChanged += OnSelectedItemPositionChanged;
                    item.ViewModel.Controller.SizeChanged += OnSelectedItemSizeChanged;
                }
            }

            if (_selectedItems.Count > 0)
                _transform = NuSysRenderer.Instance.GetTransformUntil(_selectedItems.First());

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

            BtnDelete.Update();
            BtnPresent.Update();

            if (_selectedItems.Count == 0)
            {
                _isVisible = false;
                return;
            }

            var bbs = _selectedItems.Select(elem => new Rect(elem.ViewModel.X, elem.ViewModel.Y, elem.ViewModel.Width, elem.ViewModel.Height)).ToList();
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

            if (_selectedItems.Count > 0)
                _transform = NuSysRenderer.Instance.GetTransformUntil(_selectedItems.First());

            var tl = Vector2.Transform(new Vector2((float)_rect.X, (float)_rect.Y), _transform);
            var tr = Vector2.Transform(new Vector2((float)(_rect.X+_rect.Width), (float)(_rect.Y + _rect.Height)), _transform);
           
            _screenRect = new Rect(tl.X, tl.Y, tr.X - tl.X, tr.Y - tl.Y);
            ds.Transform = Matrix3x2.Identity;

            var margin = 15 * ResourceCreator.DpiScale;
            _screenRect.X -= margin;
            _screenRect.Y -= margin;
            _screenRect.Width += margin * 2;
            _screenRect.Height += margin * 2;

            ds.DrawRectangle(_screenRect, Colors.SlateGray, 3f, new CanvasStrokeStyle { DashCap = CanvasCapStyle.Flat, DashStyle = CanvasDashStyle.Dash, DashOffset = 10f });

            BtnDelete.Postion = new Vector2((float)_screenRect.X - 40, (float)_screenRect.Y + 15);
            BtnPresent.Postion = new Vector2((float)_screenRect.X - 40, (float)_screenRect.Y + 15 + 40);
            BtnDelete.Draw(ds);
            BtnPresent.Draw(ds);

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
