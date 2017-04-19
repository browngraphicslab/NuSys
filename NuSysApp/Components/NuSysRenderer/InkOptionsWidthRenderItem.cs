﻿using System;
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
    public class InkOptionsWidthRenderItem : InteractiveBaseRenderItem
    {
        public float Radius;
        private Rect _bg =  new Rect(0,0,40,25);

        private CanvasStrokeStyle _strokeStyle = new CanvasStrokeStyle
        {
            TransformBehavior = CanvasStrokeTransformBehavior.Fixed
        };
        public static Color Color = Colors.Black;
        public bool IsActive { get; set; }


        public delegate void TapEventHandler(InkOptionsWidthRenderItem sender);
        public event TapEventHandler Tapped;
        public event TapEventHandler DoubleTapped;
        public event TapEventHandler RightTapped;

        public InkOptionsWidthRenderItem(float radius, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Radius = radius;

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

        public override Task Load()
        {
            return base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || !IsVisible)
                return;

            ds.Transform = Transform.LocalToScreenMatrix;


            if (IsActive)
                ds.DrawRectangle(_bg, Colors.Black, 2, _strokeStyle);
            else
                ds.DrawRectangle(_bg, Colors.Gray, 1, _strokeStyle);

            ds.FillCircle(new Vector2((float)_bg.Width/2f, (float)_bg.Height / 2f), Radius, Color);

            base.Draw(ds);
        }

        public override Rect GetLocalBounds()
        {
            return _bg;
        }
    }
}
