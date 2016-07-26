using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Numerics;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Mathematics;
using SharpDX.Direct2D1;
using Matrix3x2 = System.Numerics.Matrix3x2;

namespace NuSysApp
{
    public class NuSysRenderer
    {
        private CanvasAnimatedControl _canvas;

        private ConcurrentBag<BaseRenderItem> _renderItems0 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems1 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems2 = new ConcurrentBag<BaseRenderItem>();

        private InkRenderItem _inkRenderItem = new InkRenderItem();
       
        public static Matrix3x2 T = Matrix3x2.Identity;
        public static Matrix3x2 S = Matrix3x2.Identity;

        public NuSysRenderer(CanvasAnimatedControl canvas)
        {
            _canvas = canvas;
            _canvas.Draw += CanvasOnDraw;
            _canvas.Update += CanvasOnUpdate;
            _canvas.CreateResources += CanvasOnCreateResources;
            
            var vm = (FreeFormViewerViewModel)_canvas.DataContext;
            vm.Elements.CollectionChanged += ElementsChanged;

            _renderItems0.Add(_inkRenderItem);
        }

        private void CanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            //throw new NotImplementedException();
        }

        public void AddAdornment(InkStroke stroke)
        {
            _renderItems0.Add(new AdornmentRenderItem(stroke));
        }

        public void AddStroke(InkStroke stroke)
        {
            _inkRenderItem.AddStroke(stroke);
        }

        public void AddLink(LinkViewModel vm)
        {
            _renderItems1.Add(new LinkRenderItem(vm));
        }

        public void AddTrail(PresentationLinkViewModel vm)
        {
            _renderItems1.Add(new TrailRenderItem(vm));
        }

        private async void ElementsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (var newItem in args.NewItems)
            {
                var vm = (ElementViewModel) newItem;
                var elem = new ElementRenderItem(vm);
                _renderItems2.Add(elem);
            }
        }


        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            foreach (var item in _renderItems0)
                item.Update();

            foreach (var item in _renderItems1)
                item.Update();

            foreach (var item in _renderItems2)
                item.Update();

        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                ds.Clear(Colors.Gold);
                ds.Transform = T;
                foreach (var item in _renderItems0)
                    item.Draw(ds);

                foreach (var item in _renderItems1)
                    item.Draw(ds);

                foreach (var item in _renderItems2)
                    item.Draw(ds);
            }
        }
    }
}
