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
    public class MinimapRenderItem
    {
        private Rect _rect;
        private Rect _bb;
        private CanvasRenderTarget _renderTarget;
        private CollectionRenderItem _collection;
        private CanvasControl _canvasControl;
        public Size RenderTargetSize => _renderTarget.Size;

        private bool _isDisposed;

        public MinimapRenderItem(CollectionRenderItem collection, CollectionRenderItem parent, CanvasControl resourceCreator)
        {
            _collection = collection;
            _canvasControl = resourceCreator;
            _canvasControl.Draw += CanvasControlOnDraw;
        }

        private void CanvasControlOnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                Draw(ds);
            }
        }

        public void SwitchCollection(CollectionRenderItem collection)
        {
            _collection = collection;
            _renderTarget = null;
            Invalidate();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _collection = null;
            if (_canvasControl != null)
            {
                _canvasControl.Draw -= CanvasControlOnDraw;
            }
            _isDisposed = true;
        }
        
        public void CreateResources()
        {
            if (_renderTarget != null)
            {
                _renderTarget.Dispose();
            }

            float rh = (float)_collection.ViewModel.Height / (float)_collection.ViewModel.Width;
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
            _renderTarget = new CanvasRenderTarget(_canvasControl, new Size(newW, newH));
            _rect = new Rect(_collection.ViewModel.Width - newW, _collection.ViewModel.Height - newH, newW, newH);

        }

        public void Invalidate()
        {
            _canvasControl.Invalidate();
        }

        public void Draw(CanvasDrawingSession ds)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_collection?.ViewModel?.Elements?.Count != null && _collection?.ViewModel?.Elements?.Count == 0)
            {
                return;
            }

            if (_renderTarget == null)
            {
                CreateResources();
            }

            if(_collection?.ViewModel == null)
            {
                Debug.Fail("this shouldn't be null");
                return;
            }

            float rh = (float)_collection.ViewModel.Height / (float)_collection.ViewModel.Width;
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
            _rect = new Rect(_collection.ViewModel.Width - newW, _collection.ViewModel.Height - newH, newW, newH);

            using (var dss = _renderTarget.CreateDrawingSession())
            {

                dss.Clear(Color.FromArgb(220, 0, 0, 0));
                var collectionRectOrg = new Rect(_collection.ViewModel.X,
                    _collection.ViewModel.Y,
                    _collection.ViewModel.Width,
                    _collection.ViewModel.Height);

                var collectionRectScreen = Win2dUtil.TransformRect(collectionRectOrg, SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetTransformUntil(_collection));

                //if (currentColl == SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.InitialCollection)
                var collectionRect = Win2dUtil.TransformRect(collectionRectScreen, Win2dUtil.Invert(SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetCollectionTransform(_collection)));

                var rects = new List<Rect>();
                foreach (var vm in _collection.ViewModel.Elements.ToArray())
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
                rects.Add(collectionRect);
                _bb = GetBoundingRect(rects);
                var c = Matrix3x2.CreateTranslation((float)_bb.X, (float)_bb.Y);
                var cp = Win2dUtil.Invert(c);

                var scale = Math.Min(newW / (float)_bb.Width, newH / (float)_bb.Height);
                var s = Matrix3x2.CreateScale((float)scale);

                

                dss.Transform = cp * s;

                // 
                foreach (var vm in _collection.ViewModel.Elements.ToArray())
                {
                    Color color;
                    if (vm.IsSelected)
                        color = Color.FromArgb(150, 0xf0, 0xdb, 0x71);
                    else
                        color = Color.FromArgb(150, 255, 255, 255);

                    dss.FillRectangle((float)vm.X, (float)vm.Y, (float)vm.Width, (float)vm.Height, color);
                }


                //  var tlp = Vector2.Transform(tl, dss.Transform);
                //  var trp = Vector2.Transform(tr, dss.Transform);
                var viewport = Win2dUtil.TransformRect(collectionRect, dss.Transform, 3f);
                dss.Transform = Matrix3x2.Identity;
                var strokeWidth = 3f;
                dss.DrawRectangle(viewport, Colors.DarkRed, 3f);

            }

            if (_renderTarget == null || _collection.ViewModel.Elements.Count == 0)
                return;

            var old = ds.Transform;
            ds.Transform = Matrix3x2.Identity;
            var x = _canvasControl.Width - _rect.Width;
            var y= _canvasControl.Height - _rect.Height;
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
    }
}
