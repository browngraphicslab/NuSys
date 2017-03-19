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
using Windows.UI.Xaml.Media;

namespace NuSysApp
{

    /// <summary>
    /// MinimapUIElement replaces MinimapRenderItem, which was once on its own CanvasControl.
    /// 
    /// This is now a RectangleUIElement that draws the minimap on the corner of the screen.
    /// </summary>
    public class MinimapUIElement : RectangleUIElement
    {
        private Rect _rect;
        private Rect _bb;

        private CollectionRenderItem _collection;
        private CanvasRenderTarget _renderTarget;

        private RectangleUIElement _zoomRectangle;

        public MinimapUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            //SwitchCollection(collection);
            Background = Colors.Transparent;
            Width = UIDefaults.MaxMinimapWidth;
            Height = UIDefaults.MaxMinimapHeight;
            AddHandlers();
            SetUpZoomRectangle();
        }

        #region misc
        /// <summary>
        /// Sets up the rectangle that represents where we'll be zooming when released
        /// </summary>
        private void SetUpZoomRectangle()
        {
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
        /// <summary>
        /// Adds interaction handlers
        /// </summary>
        private void AddHandlers()
        {
            DoubleTapped += MinimapUIElement_DoubleTapped;
            Released += MinimapUIElement_Released;
            Dragged += MinimapUIElement_Dragged;
        }
        /// <summary>
        /// Removes interaction handlers
        /// </summary>
        private void RemoveHandlers()
        {
            DoubleTapped -= MinimapUIElement_DoubleTapped;
            Released -= MinimapUIElement_Released;
            Dragged -= MinimapUIElement_Dragged;
        }

        #endregion misc

        #region events

        /// <summary>
        /// On double tap, center camera at that point
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void MinimapUIElement_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //Goes from CanvasPointer, whose CurrentPoint is the local representation of the point, to the representation of the point relative to the minimap _rect, then to the representation of the point on the collection
            var point = GetCollectionPointFromCanvasPointer(pointer);
            //Finally we center the camera on that point, only if the current collection is the top collection
            if (_collection == SessionController.Instance.SessionView.FreeFormViewer.InitialCollection)
            {
                _collection.CenterCameraOnPoint(point);
            }
        }

        /// <summary>
        /// On drag, if called when the zoom rectangle is invisible, make it visible and of 0 size. Then increase the width and
        /// height based on the position of the pointer
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void MinimapUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //Don't do anything if this is a nested collection
            if (_collection != SessionController.Instance.SessionView.FreeFormViewer.InitialCollection)
            {
                return;
            }
            var localPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

            // get ratio of height to width of collection so that the corresponding Zoom rectangle is of the same dimension
            var ratioHeight = (float)_collection.ViewModel.Height / (float)_collection.ViewModel.Width;


            if (!_zoomRectangle.IsVisible)
            {
                _zoomRectangle.IsVisible = true;
                _zoomRectangle.Transform.LocalPosition = localPoint;
                _zoomRectangle.Width = 0f;
                _zoomRectangle.Height = 0f;

            }
            // Width of zoom rectangle
            var width = Math.Max(0, localPoint.X - _zoomRectangle.Transform.LocalX);
            //Height of zoom rectangle
            var height = width * ratioHeight;

            _zoomRectangle.Width = width;
            _zoomRectangle.Height = height;
        }

        /// <summary>
        /// On released, we want to zoom to the rectangle represented by the zoomrectangle
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void MinimapUIElement_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (!_zoomRectangle.IsVisible)
            {
                return;
            }
            _zoomRectangle.IsVisible = false;

            var ratioHeight = (float)_collection.ViewModel.Height / (float)_collection.ViewModel.Width;

            if (_zoomRectangle.Width < 15 || _zoomRectangle.Height < 15 * ratioHeight)
            {
                return;
            }

            //collection points of the rectangle
            var topLeftPoint = GetCollectionPointFromLocalPoint(GetTrueLocalPoint(_zoomRectangle.Transform.LocalPosition));
            var bottomRightPoint = GetCollectionPointFromLocalPoint(GetTrueLocalPoint(_zoomRectangle.Transform.LocalPosition + new Vector2(_zoomRectangle.Width, _zoomRectangle.Height)));

            //Centers camera on that rectangle. Only call this when current collection is the top collection
            if (_collection == SessionController.Instance.SessionView.FreeFormViewer.InitialCollection)
            {
                _collection.CenterCameraOnRectangle(topLeftPoint, bottomRightPoint);

            }

        }
        #endregion events

        /// <summary>
        /// Input: local point relative to the _rect.
        /// Output: point on collection the input represents
        /// </summary>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        private Vector2 GetCollectionPointFromLocalPoint(Vector2 localPoint)
        {


            //Creates translation based on the start of the bounding rectangle of all the nodes in the collection
            var translationMatrix = Matrix3x2.CreateTranslation((float)_bb.X, (float)_bb.Y);
            //Inverts this matrix
            var translationMatrixInverse = Win2dUtil.Invert(translationMatrix);
            //Gets scale based on the rectangle width and the bounding rectangle of all the nodes in the collection
            var scale = Math.Min(_rect.Width / (float)_bb.Width, _rect.Height / (float)_bb.Height);
            //Creates scale based on this double
            var scaleInverse = Matrix3x2.CreateScale((float)scale);


            //var pt = Vector2.Transform(localPoint, Win2dUtil.Invert(SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetCollectionTransform(_collection)));
            var matrix =  translationMatrixInverse * scaleInverse;

            //TODO: Someone smarter than me pls fix this
            if (_collection != SessionController.Instance.SessionView.FreeFormViewer.InitialCollection)
            {
                //Currently, this method doesn't give the correct collection point for nested collections. I figure it probably has to do with having to use the GetTransformUntil method on the collection to take into account the parent's transform.
            }
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
            return point - new Vector2(UIDefaults.MaxMinimapWidth - (float)_rect.Width, UIDefaults.MaxMinimapHeight - (float)_rect.Height);
        }
        /// <summary>
        /// input: local point
        /// output: whether the point is on the rect
        /// </summary>
        /// <returns></returns>
        private bool HitsRect(Vector2 point)
        {
            var maxX = UIDefaults.MaxMinimapWidth;
            var minX = UIDefaults.MaxMinimapWidth - _rect.Width;
            var maxY = UIDefaults.MaxMinimapHeight;
            var minY = UIDefaults.MaxMinimapHeight - _rect.Height;
            
            if(point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Overrides hittest so that only the rect can be hit
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            var localPoint = Vector2.Transform(screenPoint, Transform.ScreenToLocalMatrix);

            if (HitsRect(localPoint))
            {
                return base.HitTest(screenPoint);
            }
            return null;
        }
        
        /// <summary>
        /// Goes from CanvasPointer's currentpoint to the local point relative to the MinimapUIElement to the point relative to the rect.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        private Vector2 GetCollectionPointFromCanvasPointer(CanvasPointer pointer)
        {
            var localPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            var trueLocalPoint = GetTrueLocalPoint(localPoint);
            return GetCollectionPointFromLocalPoint(trueLocalPoint);
        }

        /// <summary>
        /// IMPORTANT: Call this when we switch collections
        /// </summary>
        /// <param name="collection"></param>
        public void SwitchCollection(CollectionRenderItem collection)
        {
            _collection = collection;
            _renderTarget = null;
            IsDirty = true;
        }

        /// <summary>
        /// Most of this is written by Phil
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || !SessionController.Instance.SessionSettings.MinimapVisible || _collection == null)
            {
                return;
            }

            if (_collection?.ViewModel?.Elements?.Count != null && _collection?.ViewModel?.Elements?.Count == 0)
            {
                return;
            }

            Debug.Assert(_collection?.ViewModel != null, "this shouldn't be null");
            if (_collection?.ViewModel == null)
            {
                return;
            }


            if (_renderTarget == null)
            {
                CreateResources();
            }



            float rh = (float)_collection.ViewModel.Height / (float)_collection.ViewModel.Width;
            float newW;
            float newH;

            
            if (rh < UIDefaults.MaxMinimapHeight/UIDefaults.MaxMinimapWidth)
            {
                newH = rh * UIDefaults.MaxMinimapWidth;
                newW = UIDefaults.MaxMinimapWidth;
            }
            else
            {
                newW = 1 / rh * UIDefaults.MaxMinimapHeight;
                newH = UIDefaults.MaxMinimapHeight;
            }

            //_rect = new Rect(_collection.ViewModel.Width - newW, _collection.ViewModel.Height - newH, newW, newH);
            _rect = new Rect(0,0, newW, newH); //I don't know the difference between this line and the line above. Both behave the same.

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

                if (!_zoomRectangle.IsVisible)
                {
                    dss.DrawRectangle(viewport, Colors.DarkRed, 3f);

                }

            }

            if (_renderTarget == null || _collection.ViewModel.Elements.Count == 0)
                return;

            try
            {
                var old = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;
                var xOffset =  UIDefaults.MaxMinimapWidth - _rect.Width;
                var yOffset = UIDefaults.MaxMinimapHeight - _rect.Height;

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
                newH = rh * UIDefaults.MaxMinimapWidth;
                newW = UIDefaults.MaxMinimapWidth;
            }
            else
            {
                newW = 1 / rh * UIDefaults.MaxMinimapHeight;
                newH = UIDefaults.MaxMinimapHeight;
            }
            _renderTarget = new CanvasRenderTarget(ResourceCreator, new Size(newW, newH));
            _rect = new Rect(_collection.ViewModel.Width - newW, _collection.ViewModel.Height - newH, newW, newH);

        }

        /// <summary>
        /// Gets bounding rect from a list of rects
        /// </summary>
        /// <param name="rects"></param>
        /// <returns></returns>
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

        public override void Dispose()
        {
            RemoveHandlers();
            base.Dispose();
        }
    }
}
