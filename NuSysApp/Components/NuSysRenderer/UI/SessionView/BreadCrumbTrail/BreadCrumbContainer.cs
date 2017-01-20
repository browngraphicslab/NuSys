using System;
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
    public class BreadCrumbContainer : MaskedRectangleUIElement
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
        private float initialScrollBarPosition;

        private ScrollBarUIElement _horizontalScrollBar;

        public BreadCrumbContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _breadCrumbData = new List<BreadCrumb>();
            _visibleBreadCrumbs = new List<BreadCrumbUIElement>();
            Height = 200;
            Width = 300;
            Background = Constants.LIGHT_BLUE_TRANSLUCENT;

            Mask = new Rect(BorderWidth, BorderWidth, Width - BorderWidth*2, Height - BorderWidth*2);

            _horizontalScrollBar = new ScrollBarUIElement(this, resourceCreator,
                ScrollBarUIElement.Orientation.Horizontal)
            {
                Height = UIDefaults.ScrollBarWidth,
                Width = Width - 2*BorderWidth
            };

            _horizontalScrollBar.Transform.LocalPosition = new Vector2(0, Height - _horizontalScrollBar.Height);

            AddChild(_horizontalScrollBar);

            _horizontalScrollBar.ScrollBarPositionChanged += _horizontalScrollBar_ScrollBarPositionChanged;

            Dragged += MainBackgroundDragged;
            DragStarted += MainBackgroundOnDragStarted;
        }

        private void _horizontalScrollBar_ScrollBarPositionChanged(object source, float position)
        {
            refreshUI = true;
        }

        private void MainBackgroundOnDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            initialScrollBarPosition = _horizontalScrollBar.Position;
        }

        private void MainBackgroundDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var normalizedDiff = pointer.Delta.X/_totalPathWidth;
            var _scrollDiff = -normalizedDiff;
            _horizontalScrollBar.Position = initialScrollBarPosition + _scrollDiff;
            BoundScrollHandle();
            refreshUI = true;
        }

        private void BoundScrollHandle()
        {
            // bound the handle to the bounds
            if (_horizontalScrollBar.Position < 0)
            {
                _horizontalScrollBar.Position = 0;
            }
            else if (_horizontalScrollBar.Position + _horizontalScrollBar.Range > 1)
            {
                _horizontalScrollBar.Position = 1 - _horizontalScrollBar.Range;
            }
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

            foreach (var crumb in _breadCrumbData.ToArray())
            {
                crumb.Deleted -= OnBreadCrumbDeleted;
            }

            Dragged += MainBackgroundDragged;
            DragStarted += MainBackgroundOnDragStarted;
            base.Dispose();
        }

        public async void AddBreadCrumb(LibraryElementController collectionController, ElementController controller = null)
        {
            var lastCrumb = _breadCrumbData.LastOrDefault();
            var newCrumb = new BreadCrumb(collectionController, Canvas, controller);
            if (lastCrumb == newCrumb)
            {
                newCrumb.Dispose();
                return;
            }
            await newCrumb.Load();
            newCrumb.Deleted += OnBreadCrumbDeleted;

            _breadCrumbData.Add(newCrumb);
            ComputeScrollHandleSize();
            refreshUI = true;
        }

        /// <summary>
        /// Called whenever an element associated with a breadcrumb is deleted
        /// </summary>
        /// <param name="source"></param>
        private void OnBreadCrumbDeleted(BreadCrumb sender)
        {
            _breadCrumbData.Remove(sender);
            ComputeScrollHandleSize();
            refreshUI = true;
        }

        private void ComputeCrop()
        {
            _cropRect = new Rect(_totalPathWidth * _horizontalScrollBar.Position, 0, Width, Height);
        }

        private void ComputeScrollHandleSize()
        {
            // calculate the total width needed to display all the breadcrumbs
            _totalPathWidth = _breadCrumbData.Count*BreadCrumbUIElement.DefaultWidth + 
                                (_breadCrumbData.Count + 1)*BreadCrumbUIElement.DefaultSpacing;

            // calculate the ratio of the width needed for the scrollbar
            var ratio = Math.Min(1, Width/_totalPathWidth);


            // set the new range of the _scrollHandle
            _horizontalScrollBar.Range = ratio;


            // update the position of the scroll bar so the crop rect is maintained
            var normalizedOffset = _cropRect.Left/_totalPathWidth;
            _horizontalScrollBar.Position = (float) normalizedOffset;
            BoundScrollHandle();
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


            var upperLeft = new Point(BreadCrumbUIElement.DefaultSpacing, 0);
            var lowerRight = new Point(upperLeft.X + BreadCrumbUIElement.DefaultWidth, upperLeft.Y + BreadCrumbUIElement.DefaultHeight);
            var diff = new Point(BreadCrumbUIElement.DefaultWidth + BreadCrumbUIElement.DefaultSpacing, 0);
            foreach (var crumb in _breadCrumbData.ToArray())
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
                upperLeft.X += diff.X;
                lowerRight.X += diff.X;
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
            if (crumb.CollectionController.LibraryElementModel == SessionController.Instance.CurrentCollectionLibraryElementModel)
            {
                // and if the crumb is not the current collection
                if (!crumb.IsCollection)
                {
                    // move the camera to the element the crumb represents
                    SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.CenterCameraOnElement(
                        crumb.ElementController.Id);
                }
            }
            else
            {
                // otherwise enter the collection and try to zoom in on the element model that the crumb represents
                SessionController.Instance.EnterCollection(crumb.CollectionController.LibraryElementModel.LibraryElementId, crumb.ElementController?.Id);
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if(IsDisposed)
            {
                return;
            }
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            base.Draw(ds);
            ds.Transform = orgTransform;
        }


        /// <summary>
        /// Return true if the upper left, or lower right points are at least partically contained in the cropRect
        /// </summary>
        /// <param name="upperLeft"></param>
        /// <param name="lowerRight"></param>
        /// <param name="cropRect"></param>
        /// <returns></returns>
        private bool IsPartiallyContained(Point upperLeft, Point lowerRight, Rect cropRect)
        {
            return cropRect.Contains(lowerRight) || cropRect.Contains(upperLeft);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            ReRender();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
