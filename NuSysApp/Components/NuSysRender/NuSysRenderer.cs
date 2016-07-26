using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Numerics;
using Windows.UI;
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

        private HashSet<BaseRenderItem> _renderItems = new HashSet<BaseRenderItem>();
       
        public static Matrix3x2 T = Matrix3x2.Identity;
        public static Matrix3x2 S = Matrix3x2.Identity;

        public NuSysRenderer(CanvasAnimatedControl canvas)
        {
            _canvas = canvas;
            _canvas.Draw += CanvasOnDraw;
            _canvas.Update += CanvasOnUpdate;
            _canvas.CreateResources += CanvasOnCreateResources;
            
            var vm = (FreeFormViewerViewModel)_canvas.DataContext;
            vm.Elements.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        private void CanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            //throw new NotImplementedException();
        }

        public void AddLink(LinkViewModel vm)
        {
            _renderItems.Add(new LinkRenderItem(vm));
        }

        public void AddTrail(PresentationLinkViewModel vm)
        {
            _renderItems.Add(new TrailRenderItem(vm));
        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (var newItem in args.NewItems)
            {
                var vm = (ElementViewModel) newItem;
                var elem = new ElementRenderItem(vm);
                _renderItems.Add(elem);
            }
        }


        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            foreach (var item in _renderItems)
            {
                item.Update();
            }
        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                ds.Clear(Colors.Gold);
                var x = Matrix3x2.CreateTranslation(-50000f, -50000f);
                ds.Transform = T;
                foreach (var item in _renderItems.ToArray())
                {
                    item.Draw(ds);
                }
            }
        }
    }
}
