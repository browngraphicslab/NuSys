using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SharpDX.Direct2D1;
using WinRTXamlToolkit.IO.Serialization;

namespace NuSysApp
{
    public class MinimapRenderItem : BaseRenderItem
    {
        private Rect _rect;
        private Rect _bb;
        private CanvasRenderTarget _renderTarget;
        private ElementCollectionViewModel _collection;

        public MinimapRenderItem(ElementCollectionViewModel collection, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) : base(parent, resourceCreator)
        {
            _collection = collection;
            collection.Elements.CollectionChanged += ElementsOnCollectionChanged;
        }

        public override void Dispose()
        {
            _collection.Elements.CollectionChanged -= ElementsOnCollectionChanged;
            _collection = null;
            base.Dispose();
        }

        private void ElementsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null) { 
                foreach (var newItem in args.NewItems)
                {
                    AddElement((ElementViewModel)newItem);
                }
            }

            if (args.OldItems == null)
                return;

            foreach (var newItem in args.OldItems)
            {
                RemoveElement((ElementViewModel)newItem);
            }
        }

        public void AddElement(ElementViewModel element)
        {
            element.Controller.SizeChanged += ControllerOnSizeChanged;
            element.Controller.PositionChanged += ControllerOnPositionChanged;
        }
        
        public void RemoveElement(ElementViewModel element)
        {
            element.Controller.SizeChanged -= ControllerOnSizeChanged;
            element.Controller.PositionChanged -= ControllerOnPositionChanged;
        }

        public override void CreateResources()
        {

            float rh = (float)NuSysRenderer.Instance.Size.Height / (float)NuSysRenderer.Instance.Size.Width;
            float newW;
            float newH;
            if (rh < 1)
            {
                newH = rh * 300f;
                newW = 300;
            }
            else
            {
                newW = 1 / rh * 170;
                newH = 170;
            }
            _renderTarget = new CanvasRenderTarget(ResourceCreator, new Size(newW, newH));
            _rect = new Rect(NuSysRenderer.Instance.Size.Width - newW, NuSysRenderer.Instance.Size.Height - newH, newW, newH);
        }

        public override void Update()
        {
            if (!IsDirty)
                return;

            if (_collection.Elements.Count == 0)
                return;

            if (_renderTarget == null)
                CreateResources();

            float rh = (float)NuSysRenderer.Instance.Size.Height/(float)NuSysRenderer.Instance.Size.Width;
            float newW;
            float newH;
            if (rh < 1)
            {
                newH = rh*300f;
                newW = 300;
            }
            else
            {
                newW = 1/rh*170;
                newH = 170;
            }
            _rect = new Rect(NuSysRenderer.Instance.Size.Width - newW, NuSysRenderer.Instance.Size.Height - newH, newW, newH);

            using (var dss = _renderTarget.CreateDrawingSession())
            {
                var nr = NuSysRenderer.Instance;
                var screenRect = new Rect(0, 0, nr.Size.Width, nr.Size.Height);
                var tl = nr.InitialCollection.ScreenPointToObjectPoint(new Vector2((float)screenRect.X, (float)screenRect.Y));
                var tr = nr.InitialCollection.ScreenPointToObjectPoint(new Vector2((float)screenRect.X + (float)screenRect.Width, (float)screenRect.Y + (float)screenRect.Height));

                var rects = _collection.Elements.Select(vm => new Rect(vm.X, vm.Y, vm.Width, vm.Height)).ToList();
                rects.Add(new Rect(tl.X, tl.Y, tr.X - tl.X, tr.Y - tl.Y));
                _bb = GetBoundingRect(rects);
                var c = Matrix3x2.CreateTranslation((float)_bb.X, (float)_bb.Y);
                var cp = Matrix3x2.Identity;
                Matrix3x2.Invert(c, out cp);

                var scale = Math.Min(newW / (float)_bb.Width, newH / (float)_bb.Height);
                var s = Matrix3x2.CreateScale((float)scale);
                dss.Transform = cp * s;
                dss.Clear(Color.FromArgb(220,0,0,0));
                foreach (var vm in _collection.Elements.ToArray())
                {
                    dss.FillRectangle((float)vm.X, (float)vm.Y, (float)vm.Width, (float)vm.Height, Color.FromArgb(150,255,255,255));
                }

                var tlp = Vector2.Transform(tl, dss.Transform);
                var trp = Vector2.Transform(tr, dss.Transform);
                dss.Transform = Matrix3x2.Identity;
                var strokeWidth = 3f;
                dss.DrawRectangle(new Rect(tlp.X + strokeWidth, tlp.Y + strokeWidth, Math.Max(0, trp.X - tlp.X - strokeWidth*2),  Math.Max(0, trp.Y - tlp.Y - strokeWidth * 2)), Colors.DarkRed, 3f );
            }

            IsDirty = false;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_renderTarget == null || _collection.Elements.Count == 0)
                return;

            var old = ds.Transform;
            ds.Transform = Matrix3x2.Identity;
            ds.DrawImage(_renderTarget, _rect);
            ds.Transform = old;
        }


        private Rect GetBoundingRect(List<Rect> rects)
        {
            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var maxW = double.NegativeInfinity;
            var maxH = double.NegativeInfinity;
            foreach (var rect in rects)
            {
                minX = rect.X < minX ? rect.X : minX;
                minY = rect.Y < minY ? rect.Y : minY;
                maxW = rect.X + rect.Width > maxW ? rect.X + rect.Width : maxW;
                maxH = rect.Y + rect.Height > maxH ? rect.Y + rect.Height : maxH;
            }
            return new Rect(minX, minY, maxW - minX, maxH - minY);

        }

        private void ControllerOnPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            IsDirty = true;
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            IsDirty = true;
        }
    }
}
