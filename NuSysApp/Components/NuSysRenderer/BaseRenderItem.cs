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
    public class BaseRenderItem : IDisposable
    {
        protected CanvasAnimatedControl ResourceCreator;
        public bool IsDirty { get; set; } = true;

        public BaseRenderItem(CanvasAnimatedControl resourceCreator)
        {
            ResourceCreator = resourceCreator;
        }

        public virtual async Task Load() {}

        public virtual void Update() {}

        public virtual void Draw(CanvasDrawingSession ds) {}


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
