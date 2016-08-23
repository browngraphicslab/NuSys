using System;
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

        public CanvasAnimatedControl ResourceCreator;
        public bool IsDirty { get; set; } = true;

        public CollectionRenderItem Parent { get; set; }

        public BaseRenderItem(CollectionRenderItem parent, CanvasAnimatedControl resourceCreator)
        {
            Parent = parent;
            ResourceCreator = resourceCreator;
        }

        public virtual async Task Load() {}

        public virtual void Update() {}

        public virtual void Draw(CanvasDrawingSession ds) {}

        public virtual void CreateResources()
        {
            
        }

        public virtual Matrix3x2 GetTransform()
        {
            return Win2dUtil.Invert(C)*S*C*T;
        }

        public virtual void Dispose()
        {
            ResourceCreator = null;
        }

        public virtual bool HitTest(Vector2 point)
        {
            return false;
        }
    }
}
