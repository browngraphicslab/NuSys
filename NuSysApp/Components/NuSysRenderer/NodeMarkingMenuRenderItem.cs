using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
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
    public class NodeMarkingMenuRenderItem : BaseRenderItem
    {

        private CanvasTextLayout _txtLink;
        private CanvasTextLayout _txtTrail;
        private Rect _bounds = new Rect(0,0,170,160);
        private CanvasGeometry _triangle;
        private Vector2 _pointerPosition;
        public int CurrentIndex { private set; get; } = 0;
        private bool _isActive;
        private Vector2 _entryPoint;

        public bool IsVisible { get; set; } = false;

        public NodeMarkingMenuRenderItem( CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) :base(parent, resourceCreator)
        {
            _txtLink = new CanvasTextLayout(resourceCreator, "Link",
                new CanvasTextFormat()
                {
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                VerticalAlignment = CanvasVerticalAlignment.Center,
                FontSize = 22,
                FontWeight = FontWeights.Bold
                }, 150, 80);

            _txtTrail = new CanvasTextLayout(resourceCreator, "Trail",
                new CanvasTextFormat()
                {
                    HorizontalAlignment = CanvasHorizontalAlignment.Center,
                    VerticalAlignment = CanvasVerticalAlignment.Center,
                    FontSize = 22,
                    FontWeight = FontWeights.Bold
                }, 150, 80);

            _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                new Vector2(0, 0),
                new Vector2(15, 10),
                new Vector2(0, 20),
                new Vector2(0, 0)});
        }

        public override void Dispose()
        {
            _txtLink.Dispose();
            _txtLink = null;
            _txtTrail.Dispose();
            _txtTrail = null;
            _triangle.Dispose();
            _triangle = null;

            base.Dispose();
        }

        public void UpdatePointerLocation(Vector2 pointerPosition)
        {
            _pointerPosition = pointerPosition;
        }

        public void Show(float x, float y)
        {
            int index = CurrentIndex < 0 ? 0 : CurrentIndex;
            _entryPoint = new Vector2(x,y);
            _isActive = true;
            _bounds.X = x - _bounds.Width/5*4;
            _bounds.Y = y - _bounds.Height/2 * index - _bounds.Height / 4;
            T = Matrix3x2.CreateTranslation((float)_bounds.X, (float)_bounds.Y);
        }


        public override void Update()
        {
            if (IsDisposed)
                return;

            base.Update();

            if (!IsVisible)
                return;
            
            if (_bounds.Contains(new Point(_pointerPosition.X, _pointerPosition.Y)))
            {
                if (!_isActive)
                {
                    Show((float) _entryPoint.X, (float) _entryPoint.Y);
                    _isActive = true;
                }
                else
                {
                    CurrentIndex = _pointerPosition.Y < (_bounds.Y + _bounds.Height / 2) ? 0 : 1;
                }
            }
            else if (_isActive)
            {
                _isActive = false;
                CurrentIndex = - 1;
            }
            
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            base.Draw(ds);

            if (!IsVisible || !_isActive)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = GetTransform() * ds.Transform;

            ds.FillRoundedRectangle(new Rect{X= 0, Y=0, Width = _bounds.Width, Height = _bounds.Height},5,5, Color.FromArgb(0x99, 0x6b,0x93,0x97));
           // ds.FillRectangle(new Rect { X = 0, Y = 200, Width = 150, Height = 200 }, Color.FromArgb(0x55, 0x55, 0x55, 0x55));


            ds.DrawTextLayout(_txtLink, 0, 0, Colors.White);
            ds.DrawTextLayout(_txtTrail, 0, 75, Colors.White);
            if (CurrentIndex == 0)
                ds.FillGeometry(_triangle, 20, 30, Colors.White);
            if (CurrentIndex == 1)
                ds.FillGeometry(_triangle, 20, 104, Colors.White);
            ds.Transform = orgTransform;
        }
    }
}
