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

            float rh = (float)NuSysRenderer.Instance.CurrentCollection.ViewModel.Height/(float)NuSysRenderer.Instance.CurrentCollection.ViewModel.Width;
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
            _rect = new Rect(NuSysRenderer.Instance.CurrentCollection.ViewModel.Width - newW, NuSysRenderer.Instance.CurrentCollection.ViewModel.Height - newH, newW, newH);

            using (var dss = _renderTarget.CreateDrawingSession())
            {

                var currentColl = NuSysRenderer.Instance.CurrentCollection;

                dss.Clear(Color.FromArgb(220, 0, 0, 0));
                var nr = NuSysRenderer.Instance;
                var collectionRectOrg = new Rect(NuSysRenderer.Instance.CurrentCollection.ViewModel.X,
                    NuSysRenderer.Instance.CurrentCollection.ViewModel.Y,
                    NuSysRenderer.Instance.CurrentCollection.ViewModel.Width,
                    NuSysRenderer.Instance.CurrentCollection.ViewModel.Height);

               var collectionRectScreen = Win2dUtil.TransformRect(collectionRectOrg, NuSysRenderer.Instance.GetTransformUntil(nr.CurrentCollection));

             //   if (currentColl == NuSysRenderer.Instance.InitialCollection)
             //       collectionRect = Win2dUtil.TransformRect(collectionRect, Win2dUtil.Invert(NuSysRenderer.Instance.GetCollectionTransform(nr.CurrentCollection)));

               var rects = new List<Rect>();
                foreach (var vm in currentColl.ViewModel.Elements)
                {
                    try
                    {
                        rects.Add(new Rect(Math.Max(0, vm.X), Math.Max(vm.Y, 0), Math.Max(0, vm.Width),
                            Math.Max(vm.Height, 0)));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Couldn't get element bounds for minimap.");
                    }
                }
                rects.Add(collectionRectOrg);
                _bb = GetBoundingRect(rects);
                var c = Matrix3x2.CreateTranslation((float)_bb.X, (float)_bb.Y);
                var cp = Win2dUtil.Invert(c);

                var scale = Math.Min(newW / (float)_bb.Width, newH / (float)_bb.Height);
                var s = Matrix3x2.CreateScale((float)scale);

              //  var currentColl = NuSysRenderer.Instance.CurrentCollection;

                /*
                if (NuSysRenderer.Instance.CurrentCollection != NuSysRenderer.Instance.InitialCollection)
                {
                    //var currentColl = NuSysRenderer.Instance.CurrentCollection;
                    var scaleFactor = (float)Math.Min(_bb.Width / (float)currentColl.ViewModel.Width, _bb.Height / (float)currentColl.ViewModel.Height);
                    var s2 = Matrix3x2.CreateScale(scaleFactor);
                    var targetpoint = Vector2.Transform(new Vector2((float)currentColl.ViewModel.X, (float)currentColl.ViewModel.Y), cp * s);

                    dss.Transform = Win2dUtil.Invert(currentColl.Camera.C) * currentColl.Camera.S * currentColl.Camera.C * currentColl.Camera.T *Win2dUtil.Invert(currentColl.C) * currentColl.S * currentColl.C * currentColl.T * cp * s;

                    foreach (var vm in currentColl.ViewModel.Elements.ToArray())
                    {
                        Color color;
                        if (vm.IsSelected)
                            color = Color.FromArgb(150, 0, 102, 255);
                        else
                            color = Color.FromArgb(150, 255, 255, 255);

                        dss.FillRectangle((float)vm.X, (float)vm.Y, (float)vm.Width, (float)vm.Height, color);
                    }

                }
                */

                dss.Transform = cp * s;
                
              // 
                foreach (var vm in currentColl.ViewModel.Elements.ToArray())
                {
                    Color color;
                    if (vm.IsSelected)
                        color = Color.FromArgb(150, 0, 102, 255);
                    else
                        color = Color.FromArgb(150, 255, 255, 255);

                    dss.FillRectangle((float)vm.X, (float)vm.Y, (float)vm.Width, (float)vm.Height, color);
                }

                /*
                var tlp = Vector2.Transform(tl, dss.Transform);
                var trp = Vector2.Transform(tr, dss.Transform);
                dss.Transform = Matrix3x2.Identity;
                var strokeWidth = 3f;
                dss.DrawRectangle(new Rect(tlp.X - strokeWidth, tlp.Y - strokeWidth, Math.Max(0, trp.X - tlp.X + strokeWidth*2),  Math.Max(0, trp.Y - tlp.Y + strokeWidth * 2)), Colors.DarkRed, 3f );
            */
            }

            IsDirty = false;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_renderTarget == null || _collection.Elements.Count == 0)
                return;

            var old = ds.Transform;
            ds.Transform = Matrix3x2.Identity;
            var x = NuSysRenderer.Instance.Size.Width - _rect.Width;
            var y= NuSysRenderer.Instance.Size.Height - _rect.Height;
            ds.DrawImage(_renderTarget, new Rect(x,y,_rect.Width, _rect.Height));
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
