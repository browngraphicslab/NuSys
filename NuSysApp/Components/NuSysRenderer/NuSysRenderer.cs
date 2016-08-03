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

namespace NuSysApp
{
    public class NuSysRenderer : I2dTransformable
    {

        private static volatile NuSysRenderer instance;
        private static object syncRoot = new Object();

        private CanvasAnimatedControl _canvas;

        private MinimapRenderItem _minimap;

        public CanvasAnimatedControl Canvas
        {
            get { return _canvas; }
        }

        public Size Size { get; set; }

        private ConcurrentBag<BaseRenderItem> _renderItems0 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems1 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems2 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems3 = new ConcurrentBag<BaseRenderItem>();

        private InkRenderItem _inkRenderItem;
        private ElementSelectionRenderItem _elementSelectionRenderItem;

        public Matrix3x2 T { get; set; } = Matrix3x2.Identity;
        public Matrix3x2 S { get; set; } = Matrix3x2.Identity;
        public Matrix3x2 C { get; set; } = Matrix3x2.Identity;

        private NuSysRenderer()
        {

        }

        private void CanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            //throw new NotImplementedException();
            foreach (var item in _renderItems0)
                item.CreateResources();

            foreach (var item in _renderItems1)
                item.CreateResources();

            foreach (var item in _renderItems2)
                item.CreateResources();

            foreach (var item in _renderItems3)
                item.CreateResources();

        }

        public async Task Init(CanvasAnimatedControl canvas)
        {
            Size = new Size(canvas.Width, canvas.Height);
            _canvas = canvas;
            _canvas.Draw += CanvasOnDraw;
            _canvas.Update += CanvasOnUpdate;
            _canvas.CreateResources += CanvasOnCreateResources;
            _canvas.SizeChanged += CanvasOnSizeChanged;

            var vm = (FreeFormViewerViewModel)_canvas.DataContext;
            vm.Elements.CollectionChanged += ElementsChanged;

            _elementSelectionRenderItem = new ElementSelectionRenderItem(vm, _canvas);
            _renderItems3.Add(_elementSelectionRenderItem);

            _inkRenderItem = new InkRenderItem(canvas);
            _renderItems0.Add(_inkRenderItem);

            _minimap = new MinimapRenderItem(canvas);
            _minimap.Load();
            _renderItems3.Add(_minimap);
        }

        public void Update()
        {
            _minimap.IsDirty = true;
        }

        private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Size = sizeChangedEventArgs.NewSize;
            _minimap.CreateResources();
            _minimap.IsDirty = true;
        }

        public BaseRenderItem GetRenderItemAt(Point sp)
        {
            var os = ScreenPointToObjectPoint(new Vector2((float) sp.X, (float) sp.Y));
            var elems = _renderItems0.Concat(_renderItems1).Concat(_renderItems2);

            foreach (var renderItem in elems)
            {
                if (renderItem.HitTest(os))
                {
                    return renderItem;
                }
            }
            return null;
        }

        public void AddAdornment(InkStroke stroke)
        {
            _renderItems0.Add(new AdornmentRenderItem(stroke, Canvas));
        }

        public void AddStroke(InkStroke stroke)
        {
            _inkRenderItem.AddStroke(stroke);
        }

        public void AddLink(LinkViewModel vm)
        {
            _renderItems1.Add(new LinkRenderItem(vm, Canvas));
        }

        public void AddTrail(PresentationLinkViewModel vm)
        {
            _renderItems1.Add(new TrailRenderItem(vm, Canvas));
        }

        private async void ElementsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (var newItem in args.NewItems)
            {
                BaseRenderItem item;
                var vm = (ElementViewModel) newItem;
                if (vm is TextNodeViewModel)
                {
                    item = new TextElementRenderItem((TextNodeViewModel) vm, Canvas);
                    await item.Load();
                    _renderItems0.Add(item);
                }
                else if (vm is ImageElementViewModel)
                {
                    item = new ImageElementRenderItem((ImageElementViewModel) vm, Canvas);
                    await item.Load();
                    _renderItems1.Add(item);
                }
                else if (vm is PdfNodeViewModel)
                {
                    item = new PdfElementRenderItem((PdfNodeViewModel) vm, Canvas);
                    await item.Load();
                    _renderItems1.Add(item);
                }
                else
                {
                    item = new ElementRenderItem(vm, Canvas);
                    await item.Load();
                    _renderItems2.Add(item);
                }

                _minimap.AddElement(vm);
            }
        }

        public Vector2 ScreenPointToObjectPoint(Vector2 sp)
        {
            var invTransform = Matrix3x2.Identity;
            var cp = Matrix3x2.Identity;
            Matrix3x2.Invert(C, out cp);
            var t = cp*S*C*T;
            Matrix3x2.Invert(t, out invTransform);
            return Vector2.Transform(sp, invTransform);
        }

        public Vector2 ObjectPointToScreenPoint(Vector2 op)
        {
            var invTransform = Matrix3x2.Identity;
            var cp = Matrix3x2.Identity;
            Matrix3x2.Invert(C, out cp);
            var t = cp * S * C * T;
            return Vector2.Transform(op, t);
        }


        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            foreach (var item in _renderItems0)
                item.Update();

            foreach (var item in _renderItems1)
                item.Update();

            foreach (var item in _renderItems2)
                item.Update();

            foreach (var item in _renderItems3)
                item.Update();

        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                ds.Clear(Colors.LightGoldenrodYellow);
                var cp = Matrix3x2.Identity;
                Matrix3x2.Invert(C, out cp);
                ds.Transform = cp*S*C*T;
                foreach (var item in _renderItems0)
                    item.Draw(ds);

                foreach (var item in _renderItems1)
                    item.Draw(ds);

                foreach (var item in _renderItems2)
                    item.Draw(ds);

                foreach (var item in _renderItems3)
                    item.Draw(ds);
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
