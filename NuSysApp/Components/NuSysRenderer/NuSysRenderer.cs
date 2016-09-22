using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Numerics;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Mathematics;
using SharpDX.Direct2D1;
using Matrix3x2 = System.Numerics.Matrix3x2;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MyToolkit.UI;
using Point = Windows.Foundation.Point;
using Vector2 = System.Numerics.Vector2;
using NusysIntermediate;

namespace NuSysApp
{
    public class NuSysRenderer : CanvasRenderEngine
    { 
        
        private MinimapRenderItem _minimap;
        public ElementSelectionRenderItem ElementSelectionRect;
        public NodeMarkingMenuRenderItem NodeMarkingMenu;
        public InkOptionsRenderItem InkOptions;
        private bool _isStopped;
        private RenderItemInteractionManager _interactionManager;

        public NuSysRenderer(CanvasAnimatedControl canvas, BaseRenderItem root) : base(canvas, root)
        {
            _interactionManager = new RenderItemInteractionManager(this, canvas);
        }

        public override void Start()
        {
            base.Start();

            GameLoopSynchronizationContext.RunOnGameLoopThreadAsync(CanvasAnimatedControl, async () =>
            {
                try
                {
                    await Root.Load();
                }
                catch (Exception e)
                {
                    Debug.Fail("Error while loading collection");
                }
                _isStopped = false;
            });
        }

        public override void Stop()
        {
            if (CanvasAnimatedControl == null)
                return;
            
            CanvasAnimatedControl.RunOnGameLoopThreadAsync(() =>
            {
                _isStopped = true;
            });

            base.Stop();
        }
        

        public Vector2 ScreenPointerToCollectionPoint(Vector2 sp, CollectionRenderItem collection)
        {
            return Vector2.Transform(sp, collection.Camera.ScreenToLocalMatrix);
        }

        public Matrix3x2 GetCollectionTransform(CollectionRenderItem collection)
        {
            return collection.Camera.LocalToScreenMatrix;
            /*
            var transforms = new List<CollectionRenderItem> {collection};
           
            var parent = collection.Parent;
            while (parent != null)
            {
                transforms.Add(parent as CollectionRenderItem);
                parent = parent.Parent;
            }

            transforms.Reverse();
            return transforms.Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.Transform.C)*t.Transform.S *t.Transform.C *t.Transform.T *current);
            */
        }

        public Matrix3x2 GetTransformUntil(BaseRenderItem item)
        {
            if (item.Parent == null)
                return Matrix3x2.Identity;

            return item.Transform.Parent.LocalToScreenMatrix;
            /*
var transforms = new List<I2dTransformable>();

var parent = item.Parent?.Transform;
while (parent != null)
{
    transforms.Add(parent);
    parent = parent.Parent;
}

transforms.Reverse();
*/
            // return transforms.Select(t1 => t1 as RenderItemTransform).Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.Transform.C)*t.Transform.S *t.Transform.C *t.Transform.T *current);
            //return transforms.Select(t1 => t1 as RenderItemTransform).Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.Transform.C)*t.Transform.S *t.Transform.C *t.Transform.T *current);
            //   return transforms.Select(t1 => t1 as RenderItemTransform).Aggregate(Matrix3x2.Identity, (current, t) => t.LocalMatrix * current);
        }

        public BaseRenderItem GetRenderItemAt(Vector2 sp, CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {

            var r = Root.HitTest(sp);
            if (!(r is CollectionRenderItem))
                return r;

            collection = (CollectionRenderItem)(collection ?? Root.GetChildren()[0]);
            var mat = GetTransformUntil(collection);
            var rr = _GetRenderItemAt(collection, sp, mat, 0, maxLevel);
            
            return rr;
        }

        public List<BaseRenderItem> GetRenderItemsAt(Vector2 sp,  CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            var output = new List<BaseRenderItem>();
            collection = (CollectionRenderItem)(collection ?? Root.GetChildren()[0]);
            var mat = GetTransformUntil(collection);
            _GetRenderItemsAt(collection, sp, mat, output, 0, maxLevel);
            return output;
        }

        public BaseRenderItem GetRenderItemAt(Point sp, CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            var result = GetRenderItemAt(new Vector2((float)sp.X, (float)sp.Y), collection, maxLevel);
            return result;
        }

        private BaseRenderItem _GetRenderItemAt(CollectionRenderItem collection, Vector2 sp, Matrix3x2 transform, int currentLevel, int maxLevel)
        {
            if (currentLevel < maxLevel)
            {
                var poo = Win2dUtil.Invert(collection.Camera.C) * collection.Camera.S * collection.Camera.C * collection.Camera.T *
          Win2dUtil.Invert(collection.Transform.C) * collection.Transform.S * collection.Transform.C * collection.Transform.T * transform;
                var childTransform = Win2dUtil.Invert(poo);

                var childElements = collection.GetRenderItems();
                childElements.Reverse();
                foreach (var renderItem in childElements)
                {
                    var innerCollection = renderItem as CollectionRenderItem;
                    if (innerCollection != null)
                    {

                        if (currentLevel + 1 < maxLevel)
                        {
                            var result = _GetRenderItemAt(innerCollection, sp, poo, currentLevel + 1, maxLevel);
                            if (result != collection)
                                return result;
                        }
                        else
                        {
                            if (innerCollection.HitTest(Vector2.Transform(sp, childTransform)) != null)
                            {
                                return innerCollection;
                            }
                        }
                    }


                    if (renderItem.HitTest(Vector2.Transform(sp, childTransform)) != null)
                    {
                        return renderItem;
                    }
                }
            }

            if (collection.HitTest(Vector2.Transform(sp, Win2dUtil.Invert(transform))) != null)
                return collection;

            return null;
        }

        private void _GetRenderItemsAt(CollectionRenderItem collection, Vector2 sp, Matrix3x2 transform, List<BaseRenderItem> output,  int currentLevel, int maxLevel)
        {
            if (currentLevel < maxLevel)
            {


                if (collection.HitTest(Vector2.Transform(sp, Win2dUtil.Invert(transform))) != null)
                    output.Add(collection);

                var poo = Win2dUtil.Invert(collection.Camera.C) * collection.Camera.S * collection.Camera.C * collection.Camera.T * Win2dUtil.Invert(collection.Transform.C) * collection.Transform.S * collection.Transform.C * collection.Transform.T * transform;
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
                    } else if (renderItem.HitTest(Vector2.Transform(sp, childTransform)) != null)
                    {
                        output.Add(renderItem);
                    }
                }
            } 
        }

        protected override void CanvasAnimatedControlOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            ElementSelectionRect = new ElementSelectionRenderItem(((CollectionRenderItem)(Root.GetChildren()[0])).ViewModel, null, CanvasAnimatedControl);
            ElementSelectionRect.Load();

            InkOptions = new InkOptionsRenderItem(null, CanvasAnimatedControl);
            InkOptions.IsVisible = false;
            InkOptions.Load();

            NodeMarkingMenu = new NodeMarkingMenuRenderItem(null, CanvasAnimatedControl);
            Root.AddChild(ElementSelectionRect);
            Root.AddChild(NodeMarkingMenu);
            Root.AddChild(InkOptions);
        }

        
        protected override void CanvasAnimatedControlOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            if (_isStopped)
                return;

            Root.Update(Matrix3x2.Identity);
        }

        protected override void CanvasAnimatedControlOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            if (_isStopped)
                return;

            using(var ds = args.DrawingSession) {
                ds.Clear(Colors.Transparent);
                ds.Transform = Matrix3x2.Identity;
                Root.Draw(ds);
                ds.Transform = Matrix3x2.Identity;
            }
        }
    }
}
