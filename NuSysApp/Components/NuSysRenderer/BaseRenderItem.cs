﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class BaseRenderItem : IDisposable, I2dTransformable
    {
        public Matrix3x2 T { get; set; } = Matrix3x2.Identity;
        public Matrix3x2 S { get; set; } = Matrix3x2.Identity;
        public Matrix3x2 C { get; set; } = Matrix3x2.Identity;

        public ICanvasResourceCreatorWithDpi ResourceCreator;
        public bool IsDirty { get; set; } = true;

        public BaseRenderItem Parent { get; set; }

        public List<BaseRenderItem> Children { get; private set; } = new List<BaseRenderItem>();

        public BaseRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
        {
            Parent = parent;
            ResourceCreator = resourceCreator;
        }

        public virtual async Task Load() {

        }

        public virtual void Update()
        {
            foreach (var child in Children)
            {
                child.Update();
            }
        }

        public virtual void Draw(CanvasDrawingSession ds)
        {

            var orgTransform = ds.Transform;

            ds.Transform = GetTransform() * ds.Transform;

            foreach (var child in Children)
            {
                child.Draw(ds);
            }

            ds.Transform = orgTransform;
        }

        public virtual void CreateResources()
        {            
        }

        public virtual Matrix3x2 GetTransform()
        {
            return Win2dUtil.Invert(C)*S*C*T;
        }

        public virtual void Dispose()
        {
            foreach (var child in Children)
            {
                child.Dispose();
            }
            ResourceCreator = null;
            Parent = null;
        }

        public virtual BaseRenderItem HitTest(Vector2 point)
        {
            foreach (var child in Children)
            {
                var hit = child.HitTest(point);
                if (hit != null)
                    return hit;
            }

            return GetMeasure().Contains(new Point(point.X, point.Y)) ? this : null;
        }

        public virtual Rect GetMeasure()
        {
            return new Rect();
        }
    }
}
