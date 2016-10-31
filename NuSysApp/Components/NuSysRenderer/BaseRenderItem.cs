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
        // Delegate and events to manage the focus of this element
        public delegate void BaseRenderItemFocusEvent(BaseRenderItem item);
        // Event fired when this render item gains focus
        public event BaseRenderItemFocusEvent OnFocusGained;
        // Event fired when this render item loses focus
        public event BaseRenderItemFocusEvent OnFocusLost;

        private bool _isHitTestVisible = true;
        public RenderItemTransform Transform { get; private set; } = new RenderItemTransform();

        public ICanvasResourceCreatorWithDpi ResourceCreator;
        public bool IsDirty { get; set; } = true;

        // Boolean which determines whether this render item can gain focus
        public bool IsFocusable { get; set; } = true;

        public BaseRenderItem Parent { get; set; }

        protected List<BaseRenderItem> _children = new List<BaseRenderItem>();

        public bool IsDisposed { get; protected set; }

        public bool IsVisible { get; set; } = true;

        public bool IsHitTestVisible
        {
            get
            {
                return _isHitTestVisible;
            }
            set
            {
                _isHitTestVisible = value;
                IsChildrenHitTestVisible = value;
            }
        }

        public bool IsChildrenHitTestVisible { get; set; } = true;

        public BaseRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
        {
            Parent = parent;
            ResourceCreator = resourceCreator;
        }

        public virtual async Task Load() {

            foreach (var child in _children)
            {
                await child.Load();
            }
        }

        public virtual void AddChild(BaseRenderItem child)
        {
            child.Transform.SetParent(Transform);
            _children.Add(child);
        }

        public virtual void RemoveChild(BaseRenderItem child)
        {
            _children.Remove(child);
        }

        public virtual List<BaseRenderItem> GetChildren()
        {
            return _children.ToList();
        }

        public virtual void ClearChildren()
        {
            var children = GetChildren();
            _children.Clear();
            foreach (var child in children)
            {
                child.Dispose();
            }
        }

        public virtual void SortChildren(Comparison<BaseRenderItem> comparison)
        {
            _children.Sort(comparison);
        }

        public virtual void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;

            Transform.Update(parentLocalToScreenTransform);

            foreach (var child in _children.ToArray())
            {
                child.Update(Transform.LocalToScreenMatrix);
            }
        }

        public virtual void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || !IsVisible)
                return;

            var orgTransform = ds.Transform;

            ds.Transform = Transform.LocalToScreenMatrix;

            foreach (var child in _children.ToArray())
            {
                child?.Draw(ds);
            }

            ds.Transform = orgTransform;
        }

        public virtual void CreateResources()
        {            
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            foreach (var child in _children)
            {
                child?.Dispose();
            }

            Parent = null;
            IsDisposed = true;
        }

        public virtual BaseRenderItem HitTest(Vector2 screenPoint)
        {
            if (!IsVisible)
                return null;

            if (IsChildrenHitTestVisible)
            {
                var children = _children.ToList();
                children.Reverse();

                foreach (var child in children)
                {
                    var hit = child.HitTest(screenPoint);
                    if (hit != null)
                        return hit;
                }
            }

            if (!IsHitTestVisible)
            {
                return null;
            }

            return GetScreenBounds().Contains(screenPoint.ToPoint()) ? this : null;
        }

        public virtual Rect GetLocalBounds()
        {
            return new Rect();
        }

        public virtual Rect GetScreenBounds()
        {
            return Win2dUtil.TransformRect(GetLocalBounds(), Transform.LocalToScreenMatrix);
        }

        // Called when this item gains focus - fires event
        public virtual void GotFocus()
        {
            OnFocusGained?.Invoke(this);
        }

        // Called when this item loses focus - fires event
        public virtual void LostFocus()
        {
            OnFocusLost?.Invoke(this);
        }
    }
}
