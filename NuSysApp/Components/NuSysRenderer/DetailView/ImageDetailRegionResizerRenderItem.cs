﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Foundation;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Windows.UI;
using Windows.UI.Input;

namespace NuSysApp
{
    public class ImageDetailRegionResizerRenderItem : InteractiveBaseRenderItem
    {
        public delegate void ResizerDraggedHandler(Vector2 delta);
        public delegate void ResizerHandler();

        public event ResizerDraggedHandler ResizerDragged;
        public event ResizerHandler ResizerDragStarted;
        public event ResizerHandler ResizerDragEnded;
        private CanvasGeometry _triangle;
        public ImageDetailRegionResizerRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _triangle = CanvasGeometry.CreatePolygon(ResourceCreator, new System.Numerics.Vector2[4]{
                new Vector2(0, 0),
                new Vector2(0, -30),
                new Vector2(-30, 0),
                new Vector2(0, 0)
            });
        }

        public override async Task Load()
        {
 
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (_triangle != null)
                ds.FillGeometry(_triangle, new Vector2(0,0), Colors.Black);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        public override void OnPressed(CanvasPointer pointer)
        {
            ResizerDragStarted?.Invoke();
        }

        public override void OnReleased(CanvasPointer pointer)
        {
            ResizerDragEnded?.Invoke();
        }

        public override Rect GetLocalBounds()
        {
            return new Rect(-30, -30, 30, 30);
        }
    }
}
