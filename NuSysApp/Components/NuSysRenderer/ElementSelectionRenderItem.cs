using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;


namespace NuSysApp
{
    public class ElementSelectionRenderItem : BaseRenderItem
    {
        public Rect _selectionBoundingRect;
        public Rect _screenRect;
        private FreeFormViewerViewModel _vm;
        private List<ElementRenderItem> _selectedItems = new List<ElementRenderItem>();

        public NodeMenuButtonRenderItem BtnDelete;
        public NodeMenuButtonRenderItem BtnPresent;
        public NodeMenuButtonRenderItem BtnGroup;
        public NodeMenuButtonRenderItem BtnEnterCollection;
        public NodeMenuButtonRenderItem BtnLayoutTool;
        public PdfPageButtonRenderItem BtnPdfLeft;
        public PdfPageButtonRenderItem BtnPdfRight;
        public NodeResizerRenderItem Resizer;
        public List<BaseRenderItem> Buttons = new List<BaseRenderItem>();
        private List<BaseRenderItem> _menuButtons = new List<BaseRenderItem>();
        private bool _isSinglePdfSelected;
        private bool _isSingleCollectionSelected;

        public ElementSelectionRenderItem(ElementCollectionViewModel vm, BaseRenderItem parent, CanvasAnimatedControl resourceCreator) : base(parent, resourceCreator)
        {
            BtnDelete = new NodeMenuButtonRenderItem("ms-appx:///Assets/node icons/delete.png", parent, resourceCreator);
            BtnPresent = new NodeMenuButtonRenderItem("ms-appx:///Assets/node icons/presentation-mode-dark.png", parent, resourceCreator);
            BtnGroup = new NodeMenuButtonRenderItem("ms-appx:///Assets/node icons/collection icon bluegreen.png", parent, resourceCreator);
            BtnEnterCollection = new NodeMenuButtonRenderItem("ms-appx:///Assets/node icons/icon_enter.png", parent, resourceCreator);
            BtnLayoutTool = new NodeMenuButtonRenderItem("ms-appx:///Assets/layout_icons/layout_icon.png", parent, resourceCreator);

            BtnPdfLeft = new PdfPageButtonRenderItem(-1,parent, resourceCreator);
            BtnPdfRight = new PdfPageButtonRenderItem(1,parent, resourceCreator);
            Resizer = new NodeResizerRenderItem(parent, resourceCreator);


            Buttons = new List<BaseRenderItem>
            {
                BtnDelete,
                BtnGroup,
                BtnPresent,
                BtnLayoutTool,
                BtnPdfLeft,
                BtnPdfRight,
                BtnEnterCollection,
                Resizer
            };
            _menuButtons = new List<BaseRenderItem> {BtnDelete, BtnGroup, BtnPresent, BtnLayoutTool, BtnEnterCollection };

            IsHitTestVisible = false;
            IsChildrenHitTestVisible = true;

            foreach (var btn in Buttons)
            {
                AddChild(btn);
            }

            AddChild(Resizer);

            SessionController.Instance.SessionView.FreeFormViewer.Selections.CollectionChanged += SelectionsOnCollectionChanged;
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

            _isSinglePdfSelected = _selectedItems.Count == 1 && _selectedItems[0] is PdfElementRenderItem;
            _isSingleCollectionSelected = _selectedItems.Count == 1 && _selectedItems[0] is CollectionRenderItem;

            BtnEnterCollection.IsVisible = _isSingleCollectionSelected;


            BtnDelete.IsVisible = !SessionController.Instance.SessionView.IsReadonly;
            BtnGroup.IsVisible = !SessionController.Instance.SessionView.IsReadonly;
            // Layout tool only available when editing more than one node
            BtnLayoutTool.IsVisible = !SessionController.Instance.SessionView.IsReadonly && _selectedItems.Count > 1;
            Resizer.IsVisible = !SessionController.Instance.SessionView.IsReadonly;

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

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;

            if (!IsDirty && !IsVisible)
                return;

            if (_selectedItems.Count == 0)
            {
                IsVisible = false;
                return;
            }

            var bbs = _selectedItems.ToArray().Select(elem => elem.GetSelectionBoundingRect()).ToList();

            _selectionBoundingRect = GetBoundingRect(bbs);


            var tl = new Vector2((float)_selectionBoundingRect.X, (float)_selectionBoundingRect.Y);
            var tr = new Vector2((float)(_selectionBoundingRect.X + _selectionBoundingRect.Width), (float)(_selectionBoundingRect.Y + _selectionBoundingRect.Height));

            _screenRect = new Rect(tl.X, tl.Y, tr.X - tl.X, tr.Y - tl.Y);
            var margin = 15 * SessionController.Instance.SessionView.FreeFormViewer.RenderCanvas.DpiScale;
            _screenRect.X -= margin;
            _screenRect.Y -= margin;
            _screenRect.Width += margin * 2;
            _screenRect.Height += margin * 2;

            Transform.LocalPosition = new Vector2((float)_screenRect.X, (float)_screenRect.Y);
            _screenRect.X = 0;
            _screenRect.Y = 0;


            Resizer.Transform.LocalPosition = new Vector2((float)(_screenRect.X + _screenRect.Width - 30 + 1.5f), (float)(_screenRect.Y + _screenRect.Height - 30 + 1.5f));

        
            float leftOffset = -40;
            if (_isSinglePdfSelected)
            {
                var menuEnd = _screenRect.Y + 20 + (_menuButtons.Count - 1) * 35 + 30;
                var rectCenterY = (_screenRect.Y + _screenRect.Height / 2);
                var delta = Math.Max(0, menuEnd - rectCenterY);
                leftOffset = (float)Math.Max(-80, Math.Min(-40 - delta, -40));
            }

            var count = 0;
            foreach (var btn in _menuButtons)
            {
                if (!btn.IsVisible)
                    continue;
                btn.Transform.LocalPosition = new Vector2((float)_screenRect.X + leftOffset, (float)_screenRect.Y + 20 + count * 35);
                count++;
            }
            BtnPdfLeft.IsVisible = _isSinglePdfSelected;
            BtnPdfRight.IsVisible = _isSinglePdfSelected;

            if (_isSinglePdfSelected)
            {
                BtnPdfLeft.Transform.LocalPosition = new Vector2((float)_screenRect.X, (float)(_screenRect.Y + _screenRect.Height / 2));
                BtnPdfRight.Transform.LocalPosition = new Vector2((float)(_screenRect.X + _screenRect.Width), (float)(_screenRect.Y + _screenRect.Height / 2));
            }

            base.Update(parentLocalToScreenTransform);

            IsDirty = false;
            IsVisible = true;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            if (!IsVisible)
                return;


            var tl = new Vector2((float)_selectionBoundingRect.X, (float)_selectionBoundingRect.Y);
            var tr = new Vector2((float)(_selectionBoundingRect.X + _selectionBoundingRect.Width), (float)(_selectionBoundingRect.Y + _selectionBoundingRect.Height));

            _screenRect = new Rect(tl.X, tl.Y, tr.X - tl.X, tr.Y - tl.Y);
            var margin = 15 * SessionController.Instance.SessionView.FreeFormViewer.RenderCanvas.DpiScale;
            _screenRect.X -= margin;
            _screenRect.Y -= margin;
            _screenRect.Width += margin * 2;
            _screenRect.Height += margin * 2;

            Transform.LocalPosition = new Vector2((float)_screenRect.X, (float)_screenRect.Y);
            _screenRect.X = 0;
            _screenRect.Y = 0;

            
            Resizer.Transform.LocalPosition = new Vector2((float)(_screenRect.X + _screenRect.Width - 30 + 1.5f), (float)(_screenRect.Y + _screenRect.Height - 30 + 1.5f));

            var old = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            ds.DrawRectangle(_screenRect, Colors.SlateGray, 3f, new CanvasStrokeStyle { DashCap = CanvasCapStyle.Flat, DashStyle = CanvasDashStyle.Dash, DashOffset = 10f });

          

            base.Draw(ds);

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
                if (double.IsNaN(rect.X) || double.IsNaN(rect.Y) || double.IsNaN(rect.Width) || double.IsNaN(rect.Height) )
                    return new Rect(0,0,0,0);
                minX = rect.X < minX ? rect.X : minX;
                minY = rect.Y < minY ? rect.Y : minY;
                maxW = rect.X + rect.Width > maxW ? rect.X + rect.Width : maxW;
                maxH = rect.Y + rect.Height > maxH ? rect.Y + rect.Height : maxH;
            }
            return new Rect(minX, minY, maxW-minX, maxH-minY);
        }

        public override Rect GetLocalBounds()
        {
            return new Rect(0, 0, _screenRect.Width, _screenRect.Height);
        }

    }
}
