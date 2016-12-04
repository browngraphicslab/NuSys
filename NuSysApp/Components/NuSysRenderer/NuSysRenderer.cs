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
        public NodeMenuButtonRenderItem BtnDelete;
        private bool _isStopped;
        private RenderItemInteractionManager _interactionManager;
        private bool _isInitialized;

        public NuSysRenderer(CanvasAnimatedControl canvas, SessionRootRenderItem root) : base(canvas, root)
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
        }

        public Matrix3x2 GetTransformUntil(BaseRenderItem item)
        {
            if (item.Parent == null)
                return Matrix3x2.Identity;

            return item.Transform.Parent.LocalToScreenMatrix;
        }

        public override BaseRenderItem GetRenderItemAt(Vector2 sp, BaseRenderItem item = null, int maxLevel = int.MaxValue)
        {

            var r = Root.HitTest(sp);
            if (!(r is CollectionRenderItem))
                return r;

            item = item ?? Root;
            var rr = _GetRenderItemAt(item, sp, 0, maxLevel);
            
            return rr;
        }

   
        protected override BaseRenderItem _GetRenderItemAt(BaseRenderItem item, Vector2 screenPoint,  int currentLevel, int maxLevel)
        {
            if (currentLevel < maxLevel)
            {
                var childElements = item.GetChildren();
                childElements.Reverse();
                foreach (var childItem in childElements)
                {
                    var childCollection = childItem as CollectionRenderItem;
                    if (childCollection != null)
                    {

                        if (currentLevel + 1 < maxLevel)
                        {
                            var result = _GetRenderItemAt(childCollection, screenPoint, currentLevel + 1, maxLevel);
                            if (result != item)
                                return result;
                        }
                        else
                        {
                            if (childCollection.HitTest(screenPoint) != null)
                            {
                                return childCollection;
                            }
                        }
                    }
                    if (currentLevel + 1 < maxLevel)
                    {
                        var h = childItem.HitTest(screenPoint);
                        if (h != null)
                        {
                            return h;
                        }
                    }
                    else
                    {
                        var h = childItem.HitTest(screenPoint);
                        if (h != null)
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

        protected override void _GetRenderItemsAt(BaseRenderItem item, Vector2 screenPoint, List<BaseRenderItem> output,  int currentLevel, int maxLevel)
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
                             _GetRenderItemsAt(childCollection, screenPoint,  output, currentLevel + 1, maxLevel);
                        }
                    } else if (childItem.HitTest(screenPoint) != null)
                    {
                        output.Add(childItem);
                    }
                }
            } 
        }

        protected override void CanvasAnimatedControlOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {

            ElementSelectionRect = new ElementSelectionRenderItem(((CollectionRenderItem)(Root.GetChildren()[0])).ViewModel, Root, CanvasAnimatedControl);

            InkOptions = new InkOptionsRenderItem(null, CanvasAnimatedControl);
            InkOptions.IsVisible = false;

            NodeMarkingMenu = new NodeMarkingMenuRenderItem(null, CanvasAnimatedControl);
            BtnDelete = new NodeMenuButtonRenderItem("ms-appx:///Assets/node icons/delete.png", Root, CanvasAnimatedControl);
            BtnDelete.IsVisible = false;
            Root.AddChild(ElementSelectionRect);
            Root.AddChild(NodeMarkingMenu);
            Root.AddChild(InkOptions);
            Root.AddChild(BtnDelete);

            _isInitialized = true;
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
