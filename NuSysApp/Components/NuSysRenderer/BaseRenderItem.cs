using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class BaseRenderItem : IDisposable
    {
        protected ICanvasResourceCreator ResourceCreator;
        public bool IsDirty { get; set; } = true;

        public BaseRenderItem(ICanvasResourceCreator resourceCreator)
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
