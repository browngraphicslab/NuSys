using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp.Components.NuSysRenderer
{
    public class CollectionRenderItem : BaseRenderItem, I2dTransformable
    {
        public Matrix3x2 T { get; set; }
        public Matrix3x2 S { get; set; }
        public Matrix3x2 C { get; set; }

        private ElementCollectionViewModel _vm;

        private ConcurrentBag<BaseRenderItem> _renderItems0 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems1 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems2 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems3 = new ConcurrentBag<BaseRenderItem>();

        private InkRenderItem _inkRenderItem;

        public CollectionRenderItem(ElementCollectionViewModel vm, CanvasAnimatedControl canvas) : base(canvas)
        {
            vm.Elements.CollectionChanged += ElementsChanged;

            _inkRenderItem = new InkRenderItem(canvas);
            _renderItems0.Add(_inkRenderItem);
        }

        public override void Update()
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

        public override void Draw(CanvasDrawingSession ds)
        {
            ds.Clear(Colors.LightGoldenrodYellow);
            var cp = Matrix3x2.Identity;
            Matrix3x2.Invert(C, out cp);
            ds.Transform = cp * S * C * T;
            foreach (var item in _renderItems0)
                item.Draw(ds);

            foreach (var item in _renderItems1)
                item.Draw(ds);

            foreach (var item in _renderItems2)
                item.Draw(ds);

            foreach (var item in _renderItems3)
                item.Draw(ds);
        }

        private async void ElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var newItem in e.NewItems)
            {
                BaseRenderItem item;
                var vm = (ElementViewModel) newItem;
                if (vm is TextNodeViewModel)
                {
                    item = new TextElementRenderItem((TextNodeViewModel) vm, ResourceCreator);
                    await item.Load();
                    _renderItems0.Add(item);
                }
                else if (vm is ImageElementViewModel)
                {
                    item = new ImageElementRenderItem((ImageElementViewModel) vm, ResourceCreator);
                    await item.Load();
                    _renderItems1.Add(item);
                }
                else if (vm is PdfNodeViewModel)
                {
                    item = new PdfElementRenderItem((PdfNodeViewModel) vm, ResourceCreator);
                    await item.Load();
                    _renderItems1.Add(item);
                }
                else
                {
                    item = new ElementRenderItem(vm, ResourceCreator);
                    await item.Load();
                    _renderItems2.Add(item);
                }
            }
        }

        public Vector2 ScreenPointToObjectPoint(Vector2 sp)
        {
            var invTransform = Matrix3x2.Identity;
            var cp = Matrix3x2.Identity;
            Matrix3x2.Invert(C, out cp);
            var t = cp * S * C * T;
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

        public override void CreateResources()
        {
            foreach (var item in _renderItems0)
                item.CreateResources();

            foreach (var item in _renderItems1)
                item.CreateResources();

            foreach (var item in _renderItems2)
                item.CreateResources();

            foreach (var item in _renderItems3)
                item.CreateResources();
        }


        public void AddAdornment(InkStroke stroke)
        {
            _renderItems0.Add(new AdornmentRenderItem(stroke, ResourceCreator));
        }

        public void AddStroke(InkStroke stroke)
        {
            _inkRenderItem.AddStroke(stroke);
        }

        public void AddLink(LinkViewModel vm)
        {
            _renderItems1.Add(new LinkRenderItem(vm, ResourceCreator));
        }

        public void AddTrail(PresentationLinkViewModel vm)
        {
            _renderItems1.Add(new TrailRenderItem(vm, ResourceCreator));
        }
    }
}
