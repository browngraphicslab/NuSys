using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class StackPanelUIElement : RectangleUIElement
    {
        private StackLayoutManager _layoutManager;

        public StackLayoutManager LayoutManager
        {
            get
            {
                return _layoutManager;
            }
        }
        public StackPanelUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _layoutManager = new StackLayoutManager();
        }

        public override void AddChild(BaseRenderItem child)
        {
            var interactiveBaseRenderItem = child as BaseInteractiveUIElement;
            Debug.Assert(interactiveBaseRenderItem != null);
            _layoutManager.AddElement(interactiveBaseRenderItem);
            base.AddChild(child);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _layoutManager.Width = Width;
            _layoutManager.Height = Height;
            _layoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }

        private void UpdateTransforms()
        {
            var horizontalOffset = 0f;
            foreach (var baseRenderItem in _children)
            {
                var child = baseRenderItem as BaseInteractiveUIElement;
                Debug.Assert(child != null);
                child.Transform.LocalPosition = new Vector2(horizontalOffset, 0);
                horizontalOffset += child.Width;
            }

        }

        private void UpdateWidths()
        {
            var itemToWidthDict = new Dictionary<BaseRenderItem, float>();
            float oldTotalWidth = 0;
            foreach (var baseRenderItem in _children)
            {
                var child = baseRenderItem as BaseInteractiveUIElement;
                Debug.Assert(child != null);
                itemToWidthDict[child] = child.Width;
                oldTotalWidth += child.Width;
            }
            foreach (var baseRenderItem in _children)
            {
                var child = baseRenderItem as BaseInteractiveUIElement;
                Debug.Assert(child != null);
                child.Width = itemToWidthDict[child] / oldTotalWidth * Width;
            }
        }
    }
}