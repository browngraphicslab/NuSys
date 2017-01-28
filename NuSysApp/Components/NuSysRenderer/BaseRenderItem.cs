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
        /// <summary>
        /// Delegate and events to manage the focus of this element
        /// </summary>
        /// <param name="item"></param>
        public delegate void BaseRenderItemFocusEvent(BaseRenderItem item);
        /// <summary>
        /// Event fired when this render item gains focus
        /// </summary>
        public event BaseRenderItemFocusEvent OnFocusGained;
        /// <summary>
        /// Event fired when this render item loses focus
        /// </summary>
        public event BaseRenderItemFocusEvent OnFocusLost;
        /// <summary>
        /// Event fired when the child's focus is gained, the base render item is NOT THE (father) CHILD
        /// </summary>
        public event BaseRenderItemFocusEvent OnChildFocusGained;
        /// <summary>
        /// Event fired when the child's focus is lost, the base render item is NOT THE (father) CHILD
        /// </summary>
        public event BaseRenderItemFocusEvent OnChildFocusLost;


        private bool _isHitTestVisible = true;
        public RenderItemTransform Transform { get; private set; } = new RenderItemTransform();

        public ICanvasResourceCreatorWithDpi ResourceCreator;
        public virtual bool IsDirty { get; set; } = true;

        // Boolean which determines whether this render item can gain focus
        public bool IsFocusable { get; set; } = true;

        public BaseRenderItem Parent { get; set; }

        protected List<BaseRenderItem> _children = new List<BaseRenderItem>();

        public bool IsDisposed { get; protected set; }

        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// True if one of the children has focus
        /// </summary>
        public bool ChildHasFocus { get; set; }


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

        /// <summary>
        /// True if the object currently has focus set by the focus manager
        /// try to avoid setting this yourself
        /// </summary>
        public bool HasFocus { get; set; }

        public BaseRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
        {
            Parent = parent;
            ResourceCreator = resourceCreator;
        }

        public virtual async Task Load() {

            foreach (var child in _children.ToArray())
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
            if (child != null)
            {
                child?.Dispose();
                _children.Remove(child);
            }
        }

        public virtual List<BaseRenderItem> GetChildren()
        {
            return _children.ToList();
        }

        public virtual void ClearChildren()
        {
            var children = GetChildren();
            foreach (var child in children)
            {
                child.Dispose();
            }
            _children.Clear();
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
                child?.Update(Transform.LocalToScreenMatrix);
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

            foreach (var child in _children.ToArray())
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

            var a = GetScreenBounds();

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

        /// <summary>
        /// Called when this item gains focus
        /// </summary>
        public virtual void GotFocus()
        {
            HasFocus = true;
            OnFocusGained?.Invoke(this);
        }

        /// <summary>
        /// Called when this item loses focus
        /// </summary>
        public virtual void LostFocus()
        {
            HasFocus = false;
            OnFocusLost?.Invoke(this);
        }

        /// <summary>
        /// Called whenever a child of this element gains focus
        /// </summary>
        public virtual void ChildGotFocus()
        {
            ChildHasFocus = true;
            OnChildFocusGained?.Invoke(this);
        }

        /// <summary>
        /// Called whenever a child of this element loses focus
        /// </summary>
        public virtual void ChildLostFocus()
        {
            ChildHasFocus = false;
            OnChildFocusLost?.Invoke(this);
        }
    }
}
