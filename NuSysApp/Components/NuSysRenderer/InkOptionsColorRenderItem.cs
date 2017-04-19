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

namespace NuSysApp
{
    public class InkOptionsColorRenderItem : InteractiveBaseRenderItem
    {
        private Rect _bg;
 
        private CanvasStrokeStyle _strokeStyle = new CanvasStrokeStyle
        {
            TransformBehavior = CanvasStrokeTransformBehavior.Fixed
        };

        public bool IsActive { get; set; }
        public Color Color { get; set; }

        public delegate void TapEventHandler(InkOptionsColorRenderItem sender);
        public event TapEventHandler Tapped;
        public event TapEventHandler DoubleTapped;
        public event TapEventHandler RightTapped;

        public InkOptionsColorRenderItem(Color color, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Color = color;

            var tapRecognizer = new TapGestureRecognizer();
            GestureRecognizers.Add(tapRecognizer);
            tapRecognizer.OnTapped += TapRecognizer_OnTapped;
        }

        private void TapRecognizer_OnTapped(TapGestureRecognizer sender, TapEventArgs args)
        {
            if (args.TapType == TapEventArgs.Tap.SingleTap)
            {
                Tapped?.Invoke(this);
            }
            else if (args.TapType == TapEventArgs.Tap.DoubleTap)
            {
                DoubleTapped?.Invoke(this);
            }
            else if (args.TapType == TapEventArgs.Tap.RightTap)
            {
                RightTapped?.Invoke(this);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override Task Load()
        {
            return base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
            _bg = new Rect(0,0, 40, 25);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || !IsVisible)
                return;

            ds.Transform = Transform.LocalToScreenMatrix;
            ds.FillRectangle(_bg, Color);

            if (IsActive)
                ds.DrawRectangle(_bg, Colors.Black, 2, _strokeStyle);
            else
                ds.DrawRectangle(_bg, Colors.Gray, 1, _strokeStyle);
          

            base.Draw(ds);
        }

        public override Rect GetLocalBounds()
        {
            return _bg;
        }
    }
}
