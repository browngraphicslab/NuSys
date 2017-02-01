using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DraggableWindowUIElement : WindowUIElement
    {
        /// <summary>
        /// optional close button - call "ShowCloseButton" in order to have it
        /// </summary>
        protected EllipseButtonUIElement _closeButton;

        /// <summary>
        /// Set this to true to support Dragging the DraggableWindowUIElement around the screen using the top bar.
        /// </summary>
        public bool IsDraggable;

        /// <summary>
        /// The initial point of the window when the drag event starts, new positions are calculated as the delta from this point
        /// </summary>
        private Vector2 _initialDragPosition;

        /// <summary>
        /// The buffer we use to stop users from dragging windows off the screen. The top bar is stopped from leaving the screen
        /// at WindowDragBuffer pixels before the edge of the screen on the left, right, and bottom, the top bar can always
        /// reach exactly to the top of the screen
        /// </summary>
        public float WindowDragBuffer;

        /// <summary>
        /// True if the window is snappable false otherwise
        /// </summary>
        public bool IsSnappable;

        /// <summary>
        /// Buffer from the edge of the screen where snapping starts from, be careful not to set this to something
        /// less than WindowDragBuffer
        /// </summary>
        public float Snapbuffer;

        public enum SnapPosition { Left, Top, Right, Bottom, TopLeft, TopRight, BottomLeft, BottomRight}

        /// <summary>
        /// The current snap position, null if the window is not in a position where it will snap
        /// </summary>
        protected SnapPosition? CurrentSnapPosition;

        /// <summary>
        /// The rectangle used to display the snap preview, can be null
        /// </summary>
        protected RectangleUIElement SnapPreviewRect;

        /// <summary>
        /// True if the window is currently snapped to a side
        /// </summary>
        protected bool IsSnapped { get; set; }

        /// <summary>
        /// The size of the window before it was snapped
        /// </summary>
        protected Size PreSnapSize { get; set; }

        public DraggableWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            IsDraggable = UIDefaults.WindowIsDraggable;
            TopBarDragged += OnTopBarDragged;
            TopBarDragStarted += OnTopBarDragStarted;
            TopBarDragCompleted += OnTopBarDragCompleted;
            IsSnappable = UIDefaults.WindowIsSnappable;
            WindowDragBuffer = UIDefaults.WindowDragBuffer;
            Snapbuffer = UIDefaults.SnapBuffer;
        }



        /// <summary>
        /// Called whenever the top bar is dragged and has stopped dragging
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        protected virtual void OnTopBarDragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (CurrentSnapPosition != null && SnapPreviewRect != null)
            {
                SetSnapPreviewDimensions();
                SetSnapPreviewOffset();

                PreSnapSize = new Size(Width, Height);
                Width = SnapPreviewRect.Width;
                Height = SnapPreviewRect.Height;
                Transform.LocalPosition = new Vector2(Transform.LocalX + SnapPreviewRect.Transform.LocalX, Transform.LocalY + SnapPreviewRect.Transform.LocalY);
                IsSnapped = true;
                DestroyAndHideSnapPreview();
            }

        }

        protected virtual void OnTopBarDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _initialDragPosition = Transform.LocalPosition;
            if (IsSnapped)
            {
                // the window is no longer snapped
                IsSnapped = false;

                // get the local point pressed on the top bar
                var localPointOnTopBar = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

                // normalized horizontal pointer offset of the user's clock
                var normalizedHorizontalOffset = localPointOnTopBar.X/Width;

                // reset the size to the presnap size
                Height = (float) PreSnapSize.Height;
                Width = (float) PreSnapSize.Width;

                Transform.LocalPosition = new Vector2(Transform.LocalX + localPointOnTopBar.X - normalizedHorizontalOffset * Width, Transform.LocalY);
                _initialDragPosition = Transform.LocalPosition;
            }
        }

        /// <summary>
        /// Called every time the tap bar dragged event is fired. In other words while the user
        /// is dragging the window
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        protected virtual void OnTopBarDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // shift the window
            ApplyDragDelta(pointer.Delta);
            CheckSnap(pointer);
        }

        /// <summary>
        /// Check to see if the window should snap, and set the _currentSnapPosition, and show the previw accordingly
        /// </summary>
        /// <param name="pointer"></param>
        private void CheckSnap(CanvasPointer pointer)
        {
            // if the window isn't snappable just return
            if (!IsSnappable)
            {
                return;
            }

            // booleans to determine the position we should snap to
            bool snapLeft =false;
            bool snapRight = false;
            bool snapTop = false;
            bool snapBottom = false;

            // set all the booleans based on the current snap position
            if (pointer.CurrentPoint.Y < Snapbuffer)
            {
                snapTop = true;
            }
            if (pointer.CurrentPoint.X < Snapbuffer)
            {
                snapLeft = true;
            }
            if (pointer.CurrentPoint.Y > SessionController.Instance.ScreenHeight - Snapbuffer)
            {
                snapBottom = true;
            }
            if (pointer.CurrentPoint.X > SessionController.Instance.ScreenWidth - Snapbuffer)
            {
                snapRight = true;
            }

            // if none of the snap positions returned true then just set _currentSnapPosition to null and return
            if (!snapLeft && !snapRight && !snapTop && !snapBottom)
            {
                CurrentSnapPosition = null;
                DestroyAndHideSnapPreview();
                return;
            }

            // check the corners for snapping
            if (snapLeft && snapTop)
            {
                CurrentSnapPosition = SnapPosition.TopLeft;
            } else if (snapLeft && snapBottom)
            {
                CurrentSnapPosition = SnapPosition.BottomLeft;
            } else if (snapRight && snapTop)
            {
                CurrentSnapPosition = SnapPosition.TopRight;
            } else if (snapRight && snapBottom)
            {
                CurrentSnapPosition = SnapPosition.BottomRight;
            } else if (snapLeft)
            {
                CurrentSnapPosition = SnapPosition.Left;
            } else if (snapRight)
            {
                CurrentSnapPosition = SnapPosition.Right;
            } else if (snapTop)
            {
                CurrentSnapPosition = SnapPosition.Top;
            } else if (snapBottom)
            {
                CurrentSnapPosition = SnapPosition.Bottom;
            }

            CreateAndShowSnapPreview();
        }

        /// <summary>
        /// display a preview of the snap 
        /// </summary>
        private void CreateAndShowSnapPreview()
        {
            if (SnapPreviewRect == null)
            {
                SnapPreviewRect = new RectangleUIElement(this, ResourceCreator)
                {
                    Background = UIDefaults.SnapPreviewRectColor,
                    IsHitTestVisible = false,
                };
                AddChild(SnapPreviewRect);
            }
        }

        private void DestroyAndHideSnapPreview()
        {
            if (SnapPreviewRect != null)
            {
                RemoveChild(SnapPreviewRect);
                SnapPreviewRect = null;
            }
        }

        /// <summary>
        /// protected method to apply a drag delta to the draggable window.
        /// Should take care of checking if isDraggable.
        /// </summary>
        /// <param name="delta">pointer.Delta</param>
        protected void ApplyDragDelta(Vector2 delta)
        {
            if (IsDraggable)
            {
                Transform.LocalPosition = _initialDragPosition + delta;
            }
        }

        /// <summary>
        /// Bounds the WindowUIElement to the window
        /// </summary>
        private void BoundToWindow()
        {
            // get the upper left corner of the window on the screen
            var upperLeftCornerPositionOnScreen = Vector2.Transform(new Vector2(0, 0), Transform.LocalToScreenMatrix);

            // bound the window so that its top bar is always at the top of the screen
            if (upperLeftCornerPositionOnScreen.Y < 0)
            {
                var vectordiff = Vector2.Transform(new Vector2(0, 0), Transform.ScreenToLocalMatrix).Y;
                Transform.LocalPosition = new Vector2(Transform.LocalX, Transform.LocalY + vectordiff);
            }

            if (upperLeftCornerPositionOnScreen.Y > SessionController.Instance.ScreenHeight - WindowDragBuffer)
            {
                var vectordiff = Vector2.Transform(new Vector2(0, (float) SessionController.Instance.ScreenHeight - WindowDragBuffer), Transform.ScreenToLocalMatrix).Y;
                Transform.LocalPosition = new Vector2(Transform.LocalX, Transform.LocalY + vectordiff);
            }

            // bound the window so that its upper left corner is never beyond the width of the screen
            if (upperLeftCornerPositionOnScreen.X > SessionController.Instance.ScreenWidth - WindowDragBuffer)
            {
                var vectordiff = Vector2.Transform(new Vector2((float) (SessionController.Instance.ScreenWidth - WindowDragBuffer), 0), Transform.ScreenToLocalMatrix).X;
                Transform.LocalPosition = new Vector2(Transform.LocalX + vectordiff, Transform.LocalY);
            }

            // get the upper right corner of the window on the screen
            var upperRightCornerPositionOnScreen = Vector2.Transform(new Vector2(Width, 0), Transform.LocalToScreenMatrix);

            // bound the window so that its upper right corner is never beyond the left side of the screen
            if (upperRightCornerPositionOnScreen.X < WindowDragBuffer)
            {
                var vectordiff = Vector2.Transform(new Vector2(WindowDragBuffer, 0), Transform.ScreenToLocalMatrix).X - Width;
                Transform.LocalPosition = new Vector2(Transform.LocalX + vectordiff, Transform.LocalY);
            }
        }

        /// <summary>
        /// Sets the dimensions and offset of the snap preview window
        /// </summary>
        protected virtual void SetSnapPreviewDimensions()
        {
            if (SnapPreviewRect == null)
            {
                return;
            }

            // set the snap preview rect dimensions
            switch (CurrentSnapPosition)
            {
                case SnapPosition.Left:
                case SnapPosition.Top:
                case SnapPosition.Right:
                case SnapPosition.Bottom:
                case SnapPosition.TopLeft:
                case SnapPosition.TopRight:
                case SnapPosition.BottomLeft:
                case SnapPosition.BottomRight:
                    SnapPreviewRect.Width = Width;
                    SnapPreviewRect.Height = Height;
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Sets the location position of the snap preview window, called after SetSnapPreviewDimensions
        /// </summary>
        protected virtual void SetSnapPreviewOffset()
        {
            if (SnapPreviewRect == null)
            {
                return;
            }

            // set the snap preview rect offset
            switch (CurrentSnapPosition)
            {
                case SnapPosition.Left:
                    SnapPreviewRect.Transform.LocalPosition = Vector2.Transform(new Vector2(0, 0),
                        Transform.ScreenToLocalMatrix);
                    break;
                case SnapPosition.Top:
                    SnapPreviewRect.Transform.LocalPosition = Vector2.Transform(new Vector2(0, 0),
                        Transform.ScreenToLocalMatrix);
                    break;
                case SnapPosition.Right:
                    SnapPreviewRect.Transform.LocalPosition =
                        Vector2.Transform(new Vector2((float) Math.Min(SessionController.Instance.ScreenWidth/2, SessionController.Instance.ScreenWidth - SnapPreviewRect.Width), 0),
                            Transform.ScreenToLocalMatrix);
                    break;
                case SnapPosition.Bottom:
                    SnapPreviewRect.Transform.LocalPosition =
                        Vector2.Transform(new Vector2(0, (float) Math.Min(SessionController.Instance.ScreenHeight/2, SessionController.Instance.ScreenHeight - SnapPreviewRect.Height)),
                            Transform.ScreenToLocalMatrix);
                    break;
                case SnapPosition.TopLeft:
                    SnapPreviewRect.Transform.LocalPosition = Vector2.Transform(new Vector2(0, 0),
                        Transform.ScreenToLocalMatrix);
                    break;
                case SnapPosition.TopRight:
                    SnapPreviewRect.Transform.LocalPosition =
                        Vector2.Transform(new Vector2((float)Math.Min(SessionController.Instance.ScreenWidth / 2, SessionController.Instance.ScreenWidth - SnapPreviewRect.Width), 0),
                            Transform.ScreenToLocalMatrix);
                    break;
                case SnapPosition.BottomLeft:
                    SnapPreviewRect.Transform.LocalPosition =
                        Vector2.Transform(new Vector2(0, (float)Math.Min(SessionController.Instance.ScreenHeight / 2, SessionController.Instance.ScreenHeight - SnapPreviewRect.Height)),
                            Transform.ScreenToLocalMatrix);
                    break;
                case SnapPosition.BottomRight:
                    SnapPreviewRect.Transform.LocalPosition =
                        Vector2.Transform(
                            new Vector2((float)Math.Min(SessionController.Instance.ScreenWidth / 2, SessionController.Instance.ScreenWidth - SnapPreviewRect.Width),
                                (float)Math.Min(SessionController.Instance.ScreenHeight / 2, SessionController.Instance.ScreenHeight - SnapPreviewRect.Height)), Transform.ScreenToLocalMatrix);
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override async Task Load()
        {
            if (_closeButton != null)
            {
                _closeButton.Image = _closeButton.Image ??
                                     await
                                         MediaUtil.LoadCanvasBitmapAsync(Canvas,
                                             new Uri("ms-appx:///Assets/new icons/x white.png"));
            }
            base.Load();
        }

        /// <summary>
        /// shows a close button to the left of the window
        /// </summary>
        public void ShowClosable()
        {
            _closeButton = new EllipseButtonUIElement(this, Canvas, UIDefaults.SecondaryStyle)
            {
                Height = 50,
                Width = 50,
                ImageBounds = new Rect(.25,.25,.5,.5)
            };
            AddChild(_closeButton);
            _closeButton.Transform.LocalPosition = new Vector2(-_closeButton.Width - 10, TopBarHeight + 10);
            _closeButton.Pressed += CloseButtonOnTapped;
            _closeButton.Tapped += CloseButtonOnTapped;
        }

        /// <summary>
        /// closes the window if the close button is visible.
        /// overridable if this needs to have other things in it.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        protected virtual void CloseButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            IsVisible = false;
        }

        /// <summary>
        /// Called when the DraggableWindowUIElement is disposed.
        /// Remove event handlers here
        /// </summary>
        public override void Dispose()
        {
            TopBarDragged -= OnTopBarDragged;
            TopBarDragStarted -= OnTopBarDragStarted;
            TopBarDragCompleted -= OnTopBarDragCompleted;
            if (_closeButton != null)
            {
                _closeButton.Pressed += CloseButtonOnTapped;
                _closeButton.Tapped -= CloseButtonOnTapped;
            }
            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            BoundToWindow();

            SetSnapPreviewDimensions();
            SetSnapPreviewOffset();

            if (_closeButton != null)
            {
                _closeButton.Transform.LocalPosition = new Vector2(_closeButton.Transform.LocalX, _closeButton.Transform.LocalY);
            }

            base.Update(parentLocalToScreenTransform);
        }


    }
}
