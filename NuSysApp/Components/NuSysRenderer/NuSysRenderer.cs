using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using NuSysApp.Components.NuSysRenderer;
using Point = Windows.Foundation.Point;

namespace NuSysApp
{
    public class NuSysRenderer
    {

        private static volatile NuSysRenderer instance;
        private static object syncRoot = new Object();

        private CanvasAnimatedControl _canvas;

        private MinimapRenderItem _minimap;
        private SelectMode _selectMode;

        public CanvasAnimatedControl Canvas
        {
            get { return _canvas; }
        }

        public Size Size { get; set; }


        private ElementSelectionRenderItem _elementSelectionRenderItem;

        public CollectionRenderItem ActiveCollection { get; private set; }



        private NuSysRenderer()
        {
        }

        private void CanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            //throw new NotImplementedException();


        }

        public async Task Init(CanvasAnimatedControl canvas)
        {
            Size = new Size(canvas.Width, canvas.Height);
            _canvas = canvas;
            _canvas.Draw += CanvasOnDraw;
            _canvas.Update += CanvasOnUpdate;
            _canvas.CreateResources += CanvasOnCreateResources;
            _canvas.SizeChanged += CanvasOnSizeChanged;


            var vm = (FreeFormViewerViewModel) canvas.DataContext;
            ActiveCollection = new CollectionRenderItem(vm, canvas, true);
            vm.X = 0;
            vm.Y = 0;
            vm.Width = Size.Width;
            vm.Height = Size.Height;
            _elementSelectionRenderItem = new ElementSelectionRenderItem(vm, _canvas);
     
            _minimap = new MinimapRenderItem(vm, canvas);
        }


        private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Size = sizeChangedEventArgs.NewSize;
            _minimap.CreateResources();
            _minimap.IsDirty = true;
        }

        public BaseRenderItem GetRenderItemAt(Point sp)
        {
            
            var os = ActiveCollection.ScreenPointToObjectPoint(new Vector2((float) sp.X, (float) sp.Y));
            var elems = ActiveCollection.GetRenderItems();

            foreach (var renderItem in elems)
            {
                if (renderItem.HitTest(os))
                {
                    return renderItem;
                }
            }
            
            return null;
        }

        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            ActiveCollection.Update();
            _minimap.IsDirty = true;
            _minimap.Update();
            _elementSelectionRenderItem.Update();
        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            using(var ds = args.DrawingSession) {
                ds.Clear(Colors.LightGoldenrodYellow);
                ds.Transform = Matrix3x2.Identity;
                ActiveCollection.Draw(ds);
                ds.Transform = Matrix3x2.Identity;
                _minimap.Draw(ds);
                _elementSelectionRenderItem.Draw(ds);
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
