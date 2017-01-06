using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public class DetailViewCollectionGridView : RectangleUIElement
    {
        /// <summary>
        /// The controller for the collection associated with this collection grid view
        /// </summary>
        private CollectionLibraryElementController _controller;

        /// <summary>
        /// the collection element models on the collection associated with the controller
        /// </summary>
        private List<ElementModel> _collectionElementModels; 

        /// <summary>
        /// Search box used to filter the elements currently in the grid
        /// </summary>
        private ScrollableTextboxUIElement _gridSearchBox;

        /// <summary>
        /// A dropdown menu offering different sorting options for the grid view
        /// </summary>
        private DropdownUIElement _gridSortDropDown;

        /// <summary>
        /// The handle we use to scroll through the breadcrumb trail
        /// </summary>
        private RectangleUIElement _scrollHandle;

        /// <summary>
        /// The actual scroll bar behind the handle
        /// </summary>
        private RectangleUIElement _scrollBar;

        /// <summary>
        /// Mask rect used to mask CollectionGridViewUIElements in the draw call
        /// </summary>
        private Rect _maskRect;
        

        public enum GridSortOption { Title, Date, Creator}

        private float _scrollBarWidth = 15;

        private bool refreshUI;

        private Vector2 _scrollHandleInitialDragPosition;
        private float _totalGridHeightIfRendered;
        private Rect _cropRect;
        private List<CollectionGridViewUIElement> _visibleCollectionGridElements;

        private List<CollectionGridViewUIElement> _collectionGridElements;

        public DetailViewCollectionGridView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _collectionGridElements = new List<CollectionGridViewUIElement>();

            _visibleCollectionGridElements = new List<CollectionGridViewUIElement>();

            _gridSearchBox = new ScrollableTextboxUIElement(this, resourceCreator, false, false)
            {
                //ColumnFunction = elementModel => elementModel.Title,
                //FilterFunction = s => new List<ElementModel>(_collectionElementModels.Where(em => em.Title.Contains(s))),
            };
            AddChild(_gridSearchBox);

            _gridSortDropDown = new DropdownUIElement(this, resourceCreator)
            {
                Width = 100,
                Height = 30,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                Prompt = "Sort By..."
            };
            AddChild(_gridSortDropDown);

            _gridSortDropDown.AddOptionRange(new List<string>
            {
                string.Empty,
                GridSortOptionToString(GridSortOption.Creator),
                GridSortOptionToString(GridSortOption.Date),
                GridSortOptionToString(GridSortOption.Title)
            });

            _scrollBar = new RectangleUIElement(this, resourceCreator)
            {
                Width = _scrollBarWidth,
                Background = Colors.DimGray
            };
            AddChild(_scrollBar);
            _scrollBar.Transform.LocalPosition = new Vector2(Width- _scrollBar.Width, 0);

            _scrollHandle = new RectangleUIElement(this, resourceCreator)
            {
                Width = _scrollBarWidth,
                Background = Colors.LightGray
            };
            AddChild(_scrollHandle);
            _scrollHandle.Transform.LocalPosition = new Vector2(Width - _scrollBar.Width, 0);


            _scrollBar.Tapped += OnScrollBarTapped;
            _scrollHandle.DragStarted += OnScrollHandleDragStarted;
            _scrollHandle.Dragged += OnScrollHandleDragged;
            _gridSortDropDown.Selected += _gridSortDropDown_Selected;
            Dragged += MainBackgroundDragged;
            DragStarted += MainBackgroundOnDragStarted;
        }

        private void MainBackgroundOnDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _scrollHandleInitialDragPosition = _scrollHandle.Transform.LocalPosition;
        }

        private void MainBackgroundDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var normalizedDiff = pointer.Delta.Y / _totalGridHeightIfRendered;
            var _scrollDiff = -normalizedDiff * Width;
            _scrollHandle.Transform.LocalPosition = _scrollHandleInitialDragPosition + new Vector2(0, _scrollDiff);
            BoundScrollHandle();
            refreshUI = true;
        }

        private void OnScrollHandleDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _scrollHandle.Transform.LocalPosition = _scrollHandleInitialDragPosition + new Vector2(0, pointer.Delta.Y);
            BoundScrollHandle();


            refreshUI = true;
        }

        private void BoundScrollHandle()
        {
            // bound the handle to the bounds
            if (_scrollHandle.Transform.LocalPosition.Y < 0)
            {
                _scrollHandle.Transform.LocalPosition = new Vector2(_scrollHandle.Transform.LocalX, 0);
            }
            else if (_scrollHandle.Transform.LocalPosition.Y + _scrollHandle.Height > Height)
            {
                _scrollHandle.Transform.LocalPosition = new Vector2(_scrollHandle.Transform.LocalX, Height - _scrollHandle.Height);
            }
        }

        private void OnScrollHandleDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _scrollHandleInitialDragPosition = _scrollHandle.Transform.LocalPosition;
        }

        private void OnScrollBarTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPointer = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

            if (currPointer.Y + _scrollHandle.Height < Height)
            {
                _scrollHandle.Transform.LocalPosition = new Vector2(_scrollHandle.Transform.LocalX, currPointer.Y);
            }
            else
            {
                _scrollHandle.Transform.LocalPosition = new Vector2(_scrollHandle.Transform.LocalX, currPointer.Y - _scrollHandle.Height);
            }

            refreshUI = true;
        }

        public override void Dispose()
        {
            _scrollBar.Tapped -= OnScrollBarTapped;
            _scrollHandle.DragStarted -= OnScrollHandleDragStarted;
            _scrollHandle.Dragged -= OnScrollHandleDragged;
            _gridSortDropDown.Selected -= _gridSortDropDown_Selected;
            Dragged -= MainBackgroundDragged;
            DragStarted -= MainBackgroundOnDragStarted;

            base.Dispose();
        }

        private void _gridSortDropDown_Selected(DropdownUIElement sender, string item)
        {
            SortGridBy(StringToGridSortOption(item));
        }

        private void SortGridBy(GridSortOption sortOption)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// helper method to convert a string to a GridSortOption
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static GridSortOption StringToGridSortOption(string str)
        {
            if (str == GridSortOptionToString(GridSortOption.Creator))
            {
                return GridSortOption.Creator;
            }
            if (str == GridSortOptionToString(GridSortOption.Date))
            {
                return GridSortOption.Date;
            }

            // make sure the passed in string is proper
            if (str != GridSortOptionToString(GridSortOption.Title))
            {
                Debug.Fail($"The passed in string {str} is invalid");
            }
            return GridSortOption.Title;

        }

        /// <summary>
        /// helper method to convert a grid sort option to a string
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private static string GridSortOptionToString(GridSortOption option)
        {
            switch (option)
            {
                case GridSortOption.Title:
                    return "Title";
                case GridSortOption.Date:
                    return "Date";
                case GridSortOption.Creator:
                    return "Creator";
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }

        public override async Task Load()
        {
            var request = new GetEntireWorkspaceRequest(_controller.CollectionModel.LibraryElementId, 0);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            if (request.WasSuccessful() == true)
            {
                _collectionElementModels = request.GetReturnedElementModels();
                ComputeScrollHandleSize();
                _scrollHandle.Transform.LocalPosition = new Vector2(Width - _scrollHandle.Width, _scrollHandle.Transform.LocalY);
                refreshUI = true;
            }

            Debug.Assert(_collectionElementModels != null);
            foreach (var elementModel in _collectionElementModels)
            {
                Debug.Assert(elementModel != null);
                var newElement = new CollectionGridViewUIElement(this, Canvas,
                    SessionController.Instance.ContentController.GetLibraryElementController(elementModel.LibraryId));
                Debug.Assert(newElement != null);
                _collectionGridElements.Add(newElement);
                AddChild(newElement);
            }

            base.Load();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
            {
                return;
            }
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

        private bool ExtendsBeyondRight(Vector2 upperLeft, Vector2 lowerRight, Rect cropRect)
        {
            return lowerRight.X > cropRect.Right;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            ComputeScrollHandleSize();
            ReRender();
            base.Update(parentLocalToScreenTransform);
        }

        private void ReRender()
        {
            if (!refreshUI)
            {
                return;
            }

            ComputeCrop();

            // dispose of all the previous bread crumbs
            foreach (var gridElement in _visibleCollectionGridElements)
            {
                gridElement.IsVisible = false;
            }

            Vector2 upperLeft = new Vector2(CollectionGridViewUIElement.DefaultSpacing, 0);
            Vector2 lowerRight = new Vector2(upperLeft.X + CollectionGridViewUIElement.DefaultWidth, upperLeft.Y + CollectionGridViewUIElement.DefaultHeight);
            Vector2 horizontalDiff = new Vector2(CollectionGridViewUIElement.DefaultWidth + CollectionGridViewUIElement.DefaultSpacing, 0);
            Vector2 verticalDiff = new Vector2(0, CollectionGridViewUIElement.DefaultHeight + CollectionGridViewUIElement.DefaultSpacing);
            foreach (var gridElement in _collectionGridElements.ToArray())
            {
                if (ExtendsBeyondRight(upperLeft, lowerRight, _cropRect))
                {
                    upperLeft = new Vector2(CollectionGridViewUIElement.DefaultSpacing, upperLeft.Y + verticalDiff.Y);
                    lowerRight = new Vector2(upperLeft.X + CollectionGridViewUIElement.DefaultWidth, upperLeft.Y + CollectionGridViewUIElement.DefaultHeight);
                }

                if (IsPartiallyContained(upperLeft, lowerRight, _cropRect))
                {
                    gridElement.IsVisible = true;
                    gridElement.Transform.LocalPosition = new Vector2((float)(upperLeft.X - _cropRect.Left), upperLeft.Y);
                    _visibleCollectionGridElements.Add(gridElement);
                    AddElementEvents(gridElement);
                    AddChild(gridElement);
                }

                // shift the positions for the new calculations
                upperLeft += horizontalDiff;
                lowerRight += horizontalDiff;
            }


            refreshUI = false;
        }

        private void AddElementEvents(CollectionGridViewUIElement gridElement)
        {
        }

        private void RemoveElementEvents(CollectionGridViewUIElement gridElement)
        {
        }

        private void ComputeCrop()
        {
            _cropRect = new Rect(0, _totalGridHeightIfRendered * _scrollHandle.Transform.LocalY / Height, Width - _scrollBarWidth, Height);
        }

        private void ComputeScrollHandleSize()
        {
            var numHorizontalElements =
                Math.Floor((Width - CollectionGridViewUIElement.DefaultSpacing - _scrollBarWidth) /
                           (CollectionGridViewUIElement.DefaultWidth + CollectionGridViewUIElement.DefaultSpacing));
            var numVerticalElements = Math.Ceiling(_collectionElementModels.Count/numHorizontalElements);
            // calculate the total height needed to display all the breadcrumbs
            _totalGridHeightIfRendered = (float) numVerticalElements * CollectionGridViewUIElement.DefaultHeight +
                                (_collectionElementModels.Count + 1) * CollectionGridViewUIElement.DefaultSpacing;

            // calculate the ratio of the height needed for the scroll handle
            var ratio = Math.Min(1, Height / _totalGridHeightIfRendered);

            // if all the elements can fit on the grid don't display the scrollbar
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

            // set the new heigt of the _scrollHandle
            _scrollHandle.Height = Height * ratio;

            // update the position of the scroll bar so the crop rect is maintained
            var normalizedOffset = _cropRect.Left / _totalGridHeightIfRendered;
            _scrollHandle.Transform.LocalPosition = new Vector2(_scrollHandle.Transform.LocalX, (float)normalizedOffset * Height);
            BoundScrollHandle();
        }
    }
}
