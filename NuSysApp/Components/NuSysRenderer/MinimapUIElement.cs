using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using System.Diagnostics;
using Windows.UI;
using Windows.Foundation;

namespace NuSysApp
{
    public class MinimapUIElement : RectangleUIElement
    {
        private Rect _rect;
        private Rect _bb;

        private CollectionRenderItem _collection;
        private CanvasRenderTarget _renderTarget;

        public MinimapUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionRenderItem collection) : base(parent, resourceCreator)
        {
            SwitchCollection(collection);
            Background = Colors.Transparent;
            Width = 300f;
            Height = 170f;
        }

        
        public void SwitchCollection(CollectionRenderItem collection)
        {
            _collection = collection;
            _renderTarget = null;
            //Invalidate();
            IsDirty = true;

        }


        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || !SessionController.Instance.SessionSettings.MinimapVisible)
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

            Debug.Assert(_collection?.ViewModel != null, "this shouldn't be null");
            if (_collection?.ViewModel == null)
            {
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

            try
            {
                var old = ds.Transform;
                ds.Transform = ds.Transform = Transform.LocalToScreenMatrix;
                ds.DrawImage(_renderTarget, new Rect(0,0, _rect.Width, _rect.Height));
                ds.Transform = old;
            }
            catch (Exception e)
            {
                //TODO fix this
            }


            base.Draw(ds);


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
            _renderTarget = new CanvasRenderTarget(ResourceCreator, new Size(newW, newH));
            _rect = new Rect(_collection.ViewModel.Width - newW, _collection.ViewModel.Height - newH, newW, newH);

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
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
        }


    }
}
