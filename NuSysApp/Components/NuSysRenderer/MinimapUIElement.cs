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

        private RectangleUIElement _zoomRectangle;

        public MinimapUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionRenderItem collection) : base(parent, resourceCreator)
        {
            SwitchCollection(collection);
            Background = Colors.Transparent;
            Width = 300f;
            Height = 170f;

            DoubleTapped += MinimapUIElement_DoubleTapped;
            Released += MinimapUIElement_Released;
            Dragged += MinimapUIElement_Dragged;

            _zoomRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                BorderType = BorderType.Outside,
                BorderWidth = 2f,
                BorderColor = Colors.Red,
                Background = Colors.Transparent,
                Width = 4f,
                Height = 4f,
                IsVisible = false

            };
            AddChild(_zoomRectangle);

        }

        private void MinimapUIElement_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var point = GetCollectionPointFromCanvasPointer(pointer);
            _collection.CenterCameraOnPoint(point, 800f, 800f);
        }

        private void MinimapUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var localPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            var ratioHeight = (float)_collection.ViewModel.Height / (float)_collection.ViewModel.Width;

            if (!_zoomRectangle.IsVisible)
            {
                _zoomRectangle.IsVisible = true;
                _zoomRectangle.Transform.LocalPosition = localPoint;
                _zoomRectangle.Width = 4f;
                _zoomRectangle.Height = 4f* ratioHeight ;

            }


            var width = Math.Max(0, localPoint.X - _zoomRectangle.Transform.LocalX);
            var height = width*ratioHeight;

            _zoomRectangle.Width = width;
            _zoomRectangle.Height = height;


            
        }


        private void MinimapUIElement_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_zoomRectangle.IsVisible)
            {
                var topLeftPoint = GetCollectionPointFromLocalPoint(GetTrueLocalPoint(_zoomRectangle.Transform.LocalPosition));
                var bottomRightPoint = GetCollectionPointFromLocalPoint(GetTrueLocalPoint(_zoomRectangle.Transform.LocalPosition + new Vector2(_zoomRectangle.Width, _zoomRectangle.Height)));
                _collection.CenterCameraOnRectangle(topLeftPoint, bottomRightPoint);
                _zoomRectangle.IsVisible = false;
            }

        }




        private Vector2 GetCollectionPointFromLocalPoint(Vector2 localPoint)
        {
            var collectionRectOrg = new Rect(_collection.ViewModel.X,
_collection.ViewModel.Y,
_collection.ViewModel.Width,
_collection.ViewModel.Height);

            var collectionRectScreen = Win2dUtil.TransformRect(collectionRectOrg, SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetTransformUntil(_collection));

            //if (currentColl == SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.InitialCollection)
            var collectionRect = Win2dUtil.TransformRect(collectionRectScreen, Win2dUtil.Invert(SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetCollectionTransform(_collection)));

            var c = Matrix3x2.CreateTranslation((float)_bb.X, (float)_bb.Y);
            var cp = Win2dUtil.Invert(c);

            var scale = Math.Min(_rect.Width / (float)_bb.Width, _rect.Height / (float)_bb.Height);
            var s = Matrix3x2.CreateScale((float)scale);

            var matrix = cp * s;

            var point = Vector2.Transform(localPoint, Win2dUtil.Invert(matrix));
            return point;
        }
        /// <summary>
        /// input: the local point
        /// output: the true local point (relative to the _rect)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Vector2 GetTrueLocalPoint(Vector2 point)
        {
            return point + new Vector2(300f - (float)_rect.Width, 170f - (float)_rect.Height);
        }

        private Vector2 GetCollectionPointFromCanvasPointer(CanvasPointer pointer)
        {
            var localPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            var trueLocalPoint = GetTrueLocalPoint(localPoint);
            return GetCollectionPointFromLocalPoint(trueLocalPoint);

        }


        public void SwitchCollection(CollectionRenderItem collection)
        {
            _collection = collection;
            _renderTarget = null;
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
                var xOffset =  300f - _rect.Width;
                var yOffset = 170f - _rect.Height;

                ds.DrawImage(_renderTarget, new Rect(xOffset, yOffset, _rect.Width, _rect.Height));
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
