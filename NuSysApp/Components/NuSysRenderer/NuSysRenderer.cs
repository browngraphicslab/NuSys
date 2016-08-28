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
    public class NuSysRenderer
    { 
        private static volatile NuSysRenderer instance;
        private static object syncRoot = new Object();

        private CanvasAnimatedControl _canvas;
        private MinimapRenderItem _minimap;
        public ElementSelectionRenderItem ElementSelectionRenderItem;

        public CollectionRenderItem Root { get; set; }

        public CanvasAnimatedControl Canvas
        {
            get { return _canvas; }
        }

        public Size Size { get; set; }

        private NuSysRenderer()
        {
        }

        private void CanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            ElementSelectionRenderItem = new ElementSelectionRenderItem(Root.ViewModel, null, _canvas);
            ElementSelectionRenderItem.Load();
        }

        public async Task Init(CanvasAnimatedControl canvas, CollectionRenderItem topCollection)
        {
            Size = new Size(canvas.Width, canvas.Height);
            Root = topCollection;
            _canvas = canvas;
            _canvas.Draw += CanvasOnDraw;
            _canvas.Update += CanvasOnUpdate;
            _canvas.CreateResources += CanvasOnCreateResources;
            _canvas.SizeChanged += CanvasOnSizeChanged;


        }

        private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Size = sizeChangedEventArgs.NewSize;
        }

        public Vector2 ScreenPointerToCollectionPoint(Vector2 sp, CollectionRenderItem collection)
        {
            var t = Win2dUtil.Invert(GetCollectionTransform(collection));
            return Vector2.Transform(sp, t);
        }

        public Matrix3x2 GetCollectionTransform(CollectionRenderItem collection)
        {
            var transforms = new List<CollectionRenderItem> {collection};
           
            var parent = collection.Parent;
            while (parent != null)
            {
                transforms.Add(parent);
                parent = parent.Parent;
            }

            transforms.Reverse();
            return transforms.Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.C)*t.S*t.C*t.T*current);
        }

        public Matrix3x2 GetTransformUntil(BaseRenderItem item)
        {
            var transforms = new List<I2dTransformable>();
        
            var parent = item.Parent;
            while (parent != null)
            {
                transforms.Add(parent);
                parent = parent.Parent;
            }

            transforms.Reverse();

            return transforms.Select(t1 => t1 as CollectionRenderItem).Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.C)*t.S*t.C*t.T*current);
        }

        public BaseRenderItem GetRenderItemAt(Vector2 sp, CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            if (ElementSelectionRenderItem.Resizer.HitTest(sp))
                return ElementSelectionRenderItem.Resizer;

            foreach (var btn in ElementSelectionRenderItem.Buttons)
            {
                if (btn.HitTest(sp))
                {
                    return btn;
                }
            }

            collection = collection ?? Root;
            var mat = GetTransformUntil(collection);
            return _GetRenderItemAt(collection, sp, mat, 0, maxLevel);
        }

        public List<BaseRenderItem> GetRenderItemsAt(Vector2 sp,  CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            var output = new List<BaseRenderItem>();
            collection = collection ?? Root;
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
          Win2dUtil.Invert(collection.C) * collection.S * collection.C * collection.T * transform;
                var childTransform = Win2dUtil.Invert(poo);

                foreach (var renderItem in collection.GetRenderItems())
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
                            if (innerCollection.HitTest(Vector2.Transform(sp, childTransform)))
                            {
                                return innerCollection;
                            }
                        }
                    }


                    if (renderItem.HitTest(Vector2.Transform(sp, childTransform)))
                    {
                        return renderItem;
                    }
                }
            }

            if (collection.HitTest(Vector2.Transform(sp, Win2dUtil.Invert(transform))))
                return collection;

            return null;
        }

        private void _GetRenderItemsAt(CollectionRenderItem collection, Vector2 sp, Matrix3x2 transform, List<BaseRenderItem> output,  int currentLevel, int maxLevel)
        {
            if (currentLevel < maxLevel)
            {


                if (collection.HitTest(Vector2.Transform(sp, Win2dUtil.Invert(transform))))
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
                    } else if (renderItem.HitTest(Vector2.Transform(sp, childTransform)))
                    {
                        output.Add(renderItem);
                    }
                }
            } 
        }

        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            Root.Update();
         //   _minimap.IsDirty = true;
         //   _minimap.Update();
            ElementSelectionRenderItem.Update();
        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            using(var ds = args.DrawingSession) {
                ds.Clear(Colors.Transparent);
                ds.Transform = Matrix3x2.Identity;
                Root.Draw(ds);
                ds.Transform = Matrix3x2.Identity;
            //    _minimap.Draw(ds);
                ElementSelectionRenderItem.Draw(ds);
            }
        }

        public static NuSysRenderer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new NuSysRenderer();
                    }
                }

                return instance;
            }
        }
    }
}
