using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public abstract class LayoutManager : IDisposable
    {
        public float Height { get; set; }
        public float Width { get; set; }

        public virtual void SetSize(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public abstract void ArrangeItems(Vector2 offset);

        public abstract void Dispose();
    }
}
