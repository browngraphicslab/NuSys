﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class CanvasRenderEngine
    {
        private CanvasControl _canvasControl;
        private CanvasAnimatedControl _canvasAnimatedControl;

        public CanvasControl CanvasControl => _canvasControl;
        public CanvasAnimatedControl CanvasAnimatedControl => _canvasAnimatedControl;

        public BaseRenderItem Root { get; set; }

        public GameLoopSynchronizationContext GameLoopSynchronizationContext { get; private set; }

        public CanvasRenderEngine(CanvasAnimatedControl canvas, BaseRenderItem root)
        {
            Root = root;
            _canvasAnimatedControl = canvas;
        }

        public CanvasRenderEngine(CanvasControl canvas, BaseRenderItem root)
        {
            Root = root;
            _canvasControl = canvas;
        }

        public virtual void Start()
        {
            if (_canvasAnimatedControl != null)
            {
                _canvasAnimatedControl.Draw += CanvasAnimatedControlOnDraw;
                _canvasAnimatedControl.Update += CanvasAnimatedControlOnUpdate;
                _canvasAnimatedControl.CreateResources += CanvasAnimatedControlOnCreateResources;
                GameLoopSynchronizationContext = new GameLoopSynchronizationContext(_canvasAnimatedControl);
            }

            if (_canvasControl != null)
            {
                _canvasControl.Draw += CanvasControlOnDraw;
                _canvasControl.CreateResources += CanvasControlOnCreateResources;
            }
        }

        public virtual BaseRenderItem GetRenderItemAt(Vector2 sp, BaseRenderItem item = null, int maxLevel = int.MaxValue)
        {
            item = item ?? Root;
            var hit = _GetRenderItemAt(item, sp, 0, maxLevel);
            return hit;
        }

        public virtual List<BaseRenderItem> GetRenderItemsAt(Vector2 sp, BaseRenderItem item = null, int maxLevel = int.MaxValue)
        {
            var output = new List<BaseRenderItem>();
            item = item ?? Root;
            _GetRenderItemsAt(item, sp, output, 0, maxLevel);
            return output;
        }

        protected virtual BaseRenderItem _GetRenderItemAt(BaseRenderItem item, Vector2 screenPoint, int currentLevel, int maxLevel)
        {
            if (currentLevel < maxLevel)
            {
                var childElements = item.GetChildren();
                childElements.Reverse();
                foreach (var childItem in childElements)
                {
                    if (currentLevel + 1 < maxLevel)
                    {
                        var result = _GetRenderItemAt(childItem, screenPoint, currentLevel + 1, maxLevel);
                        if (result != null && result != item)
                            return result;
                    }
                    else
                    {
                        if (childItem.HitTest(screenPoint) != null)
                        {
                            return childItem;
                        }
                    }
                   
                }
            }

            if (item.HitTest(screenPoint) != null)
                return item;

            return null;
        }

        protected virtual void _GetRenderItemsAt(BaseRenderItem item, Vector2 screenPoint, List<BaseRenderItem> output, int currentLevel, int maxLevel)
        {
            if (currentLevel < maxLevel)
            {
                if (item.HitTest(screenPoint) != null)
                    output.Add(item);

                foreach (var childItem in item.GetChildren())
                {
                    var childCollection = childItem as CollectionRenderItem;
                    if (childCollection != null)
                    {
                        if (currentLevel + 1 < maxLevel)
                        {
                            _GetRenderItemsAt(childCollection, screenPoint, output, currentLevel + 1, maxLevel);
                        }
                    }
                    else if (childItem.HitTest(screenPoint) != null)
                    {
                        output.Add(childItem);
                    }
                }
            }
        }

        public virtual void Stop()
        {
            if (_canvasAnimatedControl != null)
            {
                _canvasAnimatedControl.Draw -= CanvasAnimatedControlOnDraw;
                _canvasAnimatedControl.Update -= CanvasAnimatedControlOnUpdate;
                _canvasAnimatedControl.CreateResources -= CanvasAnimatedControlOnCreateResources;
                _canvasAnimatedControl.RunOnGameLoopThreadAsync(() =>
                {
                    Root.ClearChildren();
                    CanvasAnimatedControl.Invalidate();
                });
            }

            if (_canvasControl != null)
            {
                _canvasControl.Draw -= CanvasControlOnDraw;
                _canvasControl.CreateResources -= CanvasControlOnCreateResources;
            }
        }

        protected virtual void CanvasControlOnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
        }

        protected virtual void CanvasAnimatedControlOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
        }


        private void CanvasControlOnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                ds.Clear(Colors.Transparent);
                Root.Update(Matrix3x2.Identity);
                Root.Draw(ds);
            }
        }

        protected virtual void CanvasAnimatedControlOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            Root.Update(Matrix3x2.Identity);
        }

        protected virtual void CanvasAnimatedControlOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                ds.Clear(Colors.Transparent);
                ds.Transform = Matrix3x2.Identity;
                Root.Draw(ds);
                ds.Transform = Matrix3x2.Identity;
            }
        }
    }
}
