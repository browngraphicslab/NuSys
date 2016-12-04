﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using NusysIntermediate;

namespace NuSysApp
{
    public class BreadCrumbContainer : RectangleUIElement
    {
        /// <summary>
        /// List of bread crumbs which are currently on the trail
        /// </summary>
        private List<BreadCrumb> _breadCrumbData;

        /// <summary>
        /// List of bread crumb ui elements which are actually visible
        /// </summary>
        private List<BreadCrumbUIElement> _visibleBreadCrumbs;

        /// <summary>
        /// The handle we use to scroll through the breadcrumb trail
        /// </summary>
        private RectangleUIElement _scrollHandle;

        /// <summary>
        /// The actual scroll bar behind the handle
        /// </summary>
        private RectangleUIElement _scrollBar;

        /// <summary>
        /// The crop rectangle inside of which are contained all the bread crumbs that we can actually display
        /// </summary>
        private Rect _cropRect;

        /// <summary>
        /// The total width of all the bread crumbs in the path, if we rendered them all
        /// </summary>
        private float _totalPathWidth;

        /// <summary>
        /// true if we have to recompute our crop rect
        /// </summary>
        private bool refreshUI;

        /// <summary>
        /// The initial drag position of the scroll handle
        /// </summary>
        private Vector2 _scrollHandleInitialDragPosition;

        /// <summary>
        /// Mask rect used to mask breadcrumbs in the draw call
        /// </summary>
        private Rect _maskRect;

        public BreadCrumbContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _breadCrumbData = new List<BreadCrumb>();
            _visibleBreadCrumbs = new List<BreadCrumbUIElement>();
            Height = 200;
            Width = 300;
            Background = Colors.DarkGray;

            _maskRect = new Rect(BorderWidth, BorderWidth, Width - 2 * BorderWidth, Height - 2 * BorderWidth);


            _scrollBar = new RectangleUIElement(this, resourceCreator)
            {
                Width = Width,
                Height = 15,
                Background = Colors.DarkGray
            };
            AddChild(_scrollBar);
            _scrollBar.Transform.LocalPosition = new Vector2(0, Height - _scrollBar.Height);

            _scrollHandle = new RectangleUIElement(this, resourceCreator)
            {
                Width = Width,
                Height = 15,
                Background = Colors.LightGray
            };
            AddChild(_scrollHandle);
            _scrollHandle.Transform.LocalPosition = new Vector2(0, Height - _scrollHandle.Height);


            _scrollBar.Tapped += OnScrollBarTapped;
            _scrollHandle.DragStarted += OnScrollHandleDragStarted;
            _scrollHandle.Dragged += OnScrollHandleDragged;
        }

        private void OnScrollHandleDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _scrollHandle.Transform.LocalPosition = _scrollHandleInitialDragPosition + new Vector2(pointer.Delta.X, 0);

            // bound the handle to the bounds
            if (_scrollHandle.Transform.LocalPosition.X < 0)
            {
                _scrollHandle.Transform.LocalPosition = new Vector2(0, _scrollHandle.Transform.LocalY);
            } else if (_scrollHandle.Transform.LocalPosition.X + _scrollHandle.Width > Width)
            {
                _scrollHandle.Transform.LocalPosition = new Vector2(Width - _scrollHandle.Width, _scrollHandle.Transform.LocalY);
            }

            refreshUI = true;
        }

        private void OnScrollHandleDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _scrollHandleInitialDragPosition = _scrollHandle.Transform.LocalPosition;
        }

        public override void Dispose()
        {
            foreach (var child in GetChildren())
            {
                if (child is BreadCrumbUIElement)
                {
                    RemoveCrumbEvents(child as BreadCrumbUIElement);
                }
            }

            _scrollBar.Tapped -= OnScrollBarTapped;
            _scrollHandle.DragStarted -= OnScrollHandleDragStarted;
            _scrollHandle.Dragged -= OnScrollHandleDragged;
            base.Dispose();
        }

        private void OnScrollBarTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPointer = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            
            // try to move the scroll handle so its left side is on the tapped part, if there isn't room put it's right side on the tapped part
            if (currPointer.X + _scrollHandle.Width < Width)
            {
                _scrollHandle.Transform.LocalPosition = new Vector2(currPointer.X, _scrollHandle.Transform.LocalY);
            }
            else
            {
                _scrollHandle.Transform.LocalPosition = new Vector2(currPointer.X - _scrollHandle.Width, _scrollHandle.Transform.LocalY);
            }

            refreshUI = true;
        }

        public void AddBreadCrumb(string collectionId, ElementModel model = null)
        {
            var lastCrumb = _breadCrumbData.LastOrDefault();
            var newCrumb = new BreadCrumb(collectionId, model);
            if (lastCrumb == newCrumb)
            {
                return;
            }

            _breadCrumbData.Add(newCrumb);
            ComputeScrollHandleSize();
            _scrollHandle.Transform.LocalPosition = new Vector2(Width - _scrollHandle.Width, _scrollHandle.Transform.LocalY);
            refreshUI = true;
        }

        private void ComputeCrop()
        {
            _cropRect = new Rect(_totalPathWidth * _scrollHandle.Transform.LocalX / Width, 0, Width, Height);
        }

        private void ComputeScrollHandleSize()
        {
            // calculate the total width needed to display all the breadcrumbs
            _totalPathWidth = _breadCrumbData.Count*BreadCrumbUIElement.DefaultWidth + 
                                (_breadCrumbData.Count + 1)*BreadCrumbUIElement.DefaultSpacing;

            // calculate the ratio of the width needed for the scroll handle
            var ratio = Math.Min(1, Width/_totalPathWidth);
            if (Math.Abs(ratio - 1) < .001)
            {
                _scrollHandle.IsVisible = false;
                _scrollBar.IsVisible = false;
            }
            else
            {
                _scrollHandle.IsVisible = true;
                _scrollBar.IsVisible = true;
            }

            // set the new width of the _scrollHandle
            _scrollHandle.Width = Width * ratio;

            // update the position of the scroll bar so the crop rect is maintained
            var normalizedOffset = _cropRect.Left/_totalPathWidth;
            _scrollHandle.Transform.LocalPosition = new Vector2((float) normalizedOffset * Width, _scrollHandle.Transform.LocalY);
        }

        public void ReRender()
        {
            if (!refreshUI)
            {
                return;
            }

            ComputeCrop();

            // dispose of all the previous bread crumbs
            foreach (var breadCrumb in _visibleBreadCrumbs)
            {
                RemoveCrumbEvents(breadCrumb);
                RemoveChild(breadCrumb); // fires the dispose method automatically
            }


            Vector2 upperLeft = new Vector2(BreadCrumbUIElement.DefaultSpacing, 0);
            Vector2 lowerRight = new Vector2(upperLeft.X + BreadCrumbUIElement.DefaultWidth, upperLeft.Y + BreadCrumbUIElement.DefaultHeight);
            Vector2 diff = new Vector2(BreadCrumbUIElement.DefaultWidth + BreadCrumbUIElement.DefaultSpacing, 0);
            foreach (var crumb in _breadCrumbData)
            {
                if (IsPartiallyContained(upperLeft, lowerRight, _cropRect))
                {
                    var breadCrumb = new BreadCrumbUIElement(this, Canvas, crumb);
                    breadCrumb.Transform.LocalPosition = new Vector2((float) (upperLeft.X - _cropRect.Left), Height/2 - BreadCrumbUIElement.DefaultHeight/2);
                    _visibleBreadCrumbs.Add(breadCrumb);
                    AddCrumbEvents(breadCrumb);
                    AddChild(breadCrumb);
                }

                // shift the positions for the new calculations
                upperLeft += diff;
                lowerRight += diff;
            }


            refreshUI = false;
        }

        /// <summary>
        /// Remove events from the bread crumb ui
        /// </summary>
        /// <param name="breadCrumb"></param>
        private void RemoveCrumbEvents(BreadCrumbUIElement breadCrumb)
        {
            breadCrumb.Tapped -= BreadCrumb_Tapped;
        }

        /// <summary>
        /// Add events to the bread crumb ui
        /// </summary>
        /// <param name="breadCrumb"></param>
        private void AddCrumbEvents(BreadCrumbUIElement breadCrumb)
        {
            breadCrumb.Tapped += BreadCrumb_Tapped;
        }

        private void BreadCrumb_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var crumb = (item as BreadCrumbUIElement)?.Crumb;
            Debug.Assert(crumb != null);

            // if the crumb we clicked on is in the current collection
            if (crumb.CollectionId == SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId)
            {
                // and if the crumb is not the current collection
                if (!crumb.IsCollection)
                {
                    UITask.Run(() =>
                    {
                        // move the camera to the element the crumb represents
                        SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.CenterCameraOnElement(
                            crumb.ElementModel.Id);
                    });

                }
            }
            // otherwise if the crumb is a collection then enter it
            else if (crumb.IsCollection)
            {
                UITask.Run(() =>
                {
                    SessionController.Instance.EnterCollection(crumb.CollectionId);
                });
            }
            else
            {
                UITask.Run(() =>
                {
                    // if the crumb is in another collection and represents an element, enter that collection and go to that element
                    SessionController.Instance.EnterCollection(crumb.CollectionId, crumb.ElementModel.Id);
                });
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            using (ds.CreateLayer(1, _maskRect))
            {
                base.Draw(ds);
            }
            ds.Transform = orgTransform;
        }


        /// <summary>
        /// Return true if the upper left, or lower right points are at least partically contained in the cropRect
        /// </summary>
        /// <param name="upperLeft"></param>
        /// <param name="lowerRight"></param>
        /// <param name="cropRect"></param>
        /// <returns></returns>
        private bool IsPartiallyContained(Vector2 upperLeft, Vector2 lowerRight, Rect cropRect)
        {
            return lowerRight.X > cropRect.Left && lowerRight.X < cropRect.Right ||
                   upperLeft.X < cropRect.Right && upperLeft.X > cropRect.Left;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            ReRender();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
