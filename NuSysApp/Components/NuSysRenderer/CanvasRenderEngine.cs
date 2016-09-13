using System;
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
        private MinimapRenderItem _minimap;
        public ElementSelectionRenderItem ElementSelectionRenderItem;
        public NodeMarkingMenuRenderItem NodeMarkingMenu;

        public BaseRenderItem Root { get; set; }

        public GameLoopSynchronizationContext GameLoopSynchronizationContext { get; private set; }


        private void CanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
        }

        public virtual void Stop()
        {
            if (_canvasAnimatedControl != null)
            {
                _canvasAnimatedControl.Draw -= CanvasOnDraw;
                _canvasAnimatedControl.Update -= CanvasOnUpdate;
                _canvasAnimatedControl.CreateResources -= CanvasOnCreateResources;
            }

            if (_canvasControl != null)
            {
                _canvasControl.Draw -= CanvasControlOnDraw;
                _canvasControl.CreateResources -= CanvasControlOnCreateResources;
            }


            Root.Dispose();
        }

        private void CanvasControlOnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
        }

        private void CanvasControlOnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                ds.Clear(Colors.Transparent);
                Root.Draw(ds);
            }
        }


        public virtual void Init(CanvasAnimatedControl canvas, BaseRenderItem root)
        {
            Root = root;
            _canvasAnimatedControl = canvas;
            _canvasAnimatedControl.Draw += CanvasOnDraw;
            _canvasAnimatedControl.Update += CanvasOnUpdate;
            _canvasAnimatedControl.CreateResources += CanvasOnCreateResources;
            GameLoopSynchronizationContext = new GameLoopSynchronizationContext(canvas);
        }

        public virtual void Init(CanvasControl canvas, BaseRenderItem root)
        {
            Root = root;
            _canvasControl = canvas;
            _canvasControl.Draw += CanvasControlOnDraw;
            _canvasControl.CreateResources += CanvasControlOnCreateResources;
        }


        public Matrix3x2 GetTransformUntilOf(BaseRenderItem item)
        {
            var transforms = new List<I2dTransformable>();

            var parent = item.Parent;
            while (parent != null)
            {
                transforms.Add(parent);
                parent = parent.Parent;
            }

            transforms.Reverse();

            return transforms.Aggregate(Matrix3x2.Identity, (current, t) =>  Win2dUtil.Invert(t.C) * t.S * t.C * t.T * current);
        }

        public BaseRenderItem GetRenderItemAt(Vector2 sp, BaseRenderItem startItem, int maxLevel = int.MaxValue)
        {
            var mat = GetTransformUntilOf(startItem);
            return _GetRenderItemAt(startItem, sp, mat, 0, maxLevel);
        }

        public List<BaseRenderItem> GetRenderItemsAt(Vector2 sp, BaseRenderItem startItem = null, int maxLevel = int.MaxValue)
        {
            var output = new List<BaseRenderItem>();
            startItem = startItem ?? Root;
            var mat = GetTransformUntilOf(startItem);
           // _GetRenderItemsAt(startItem, sp, mat, output, 0, maxLevel);
            return null;
        }

        public BaseRenderItem GetRenderItemAt(Point sp, BaseRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            var result = GetRenderItemAt(new Vector2((float)sp.X, (float)sp.Y), collection, maxLevel);
            return result;
        }

        private BaseRenderItem _GetRenderItemAt(BaseRenderItem startItem, Vector2 sp, Matrix3x2 transform, int currentLevel, int maxLevel)
        {
            var t = startItem.GetTransform() * transform;

            if (currentLevel < maxLevel)
            {                

                foreach (var child in startItem.Children)
                {
                    if (currentLevel + 1 < maxLevel)
                    {
                        var result = _GetRenderItemAt(child, sp, t, currentLevel + 1, maxLevel);
                        if (result != null)
                            return result;
                    }
                }
            }

            if (startItem.HitTest(Vector2.Transform(sp, Win2dUtil.Invert(t))) != null)
                return startItem;

            return null;
        }
        /*
        private void _GetRenderItemsAt(CollectionRenderItem collection, Vector2 sp, Matrix3x2 transform, List<BaseRenderItem> output, int currentLevel, int maxLevel)
        {
            if (currentLevel < maxLevel)
            {


                if (collection.HitTest(Vector2.Transform(sp, Win2dUtil.Invert(transform))) != null)
                    output.Add(collection);

                var poo = Win2dUtil.Invert(collection.Camera.C) * collection.Camera.S * collection.Camera.C * collection.Camera.T *
Win2dUtil.Invert(collection.C) * collection.S * collection.C * collection.T * transform;
                var childTransform = Win2dUtil.Invert(poo);

                foreach (var renderItem in collection.GetRenderItems())
                {
                    var coll = renderItem as CollectionRenderItem;
                    if (coll != null)
                    {

                        if (currentLevel + 1 < maxLevel)
                        {
                            _GetRenderItemsAt(coll, sp, poo, output, currentLevel + 1, maxLevel);
                        }
                    }
                    else if (renderItem.HitTest(Vector2.Transform(sp, childTransform)) != null)
                    {
                        output.Add(renderItem);
                    }
                }
            }
        }*/

        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            Root.Update();
        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
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
