using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Office.Interop.Word;
using NusysIntermediate;
using Task = System.Threading.Tasks.Task;

//using Wintellect.PowerCollections;

namespace NuSysApp
{
    public class ImageElementRenderItem : ElementRenderItem
    {
        private ImageElementViewModel _vm;
        private ImageDetailRenderItem _image;
        private InkableUIElement _inkable;

        // gridlines, can be null
        private RectangleUIElement _left;
        private RectangleUIElement _right;
        private RectangleUIElement _top;
        private RectangleUIElement _bottom;

        // represent which boundary is snapping to another element
        private enum Location { Left, Right, Top, Bottom, None }
        private Location _horizontalSnapLocation;
        private Location _verticalSnapLocation;

        // rectangle on this element to signal that it is involved in snap alignment, can be null
        private RectangleUIElement _snapSelectionView;

        // a list of ImageElementRenderItem in the same collection used for snap alignment
        private List<ImageElementRenderItem> _siblingImageElementRenderItems;

        // keeps track of the ImageElementRenderItem this item is horizontally snapped to 
        private ImageElementRenderItem _horizontalSnapItem;
        // keeps track of the ImageElementRenderItem this item is vertically snapped to 
        private ImageElementRenderItem _verticalSnapItem;

        /// <summary>
        /// Size of the image (used for snap alignment)
        /// </summary>
        public Size ImageCanvasSize
        {
            get { return _image.CanvasSize; }
        }

        /// <summary>
        /// Top boundary of image
        /// </summary>
        public double ViewModelTop
        {
            get { return _vm.Y; }
            set { _vm.Y = value; }
        }

        /// <summary>
        /// Bottom boundary of image
        /// </summary>
        public double ViewModelBottom
        {
            get { return _vm.Y + _image.CanvasSize.Height; }
            set { _vm.Y = value - _image.CanvasSize.Height; }
        }

        /// <summary>
        /// Left boundary of image
        /// </summary>
        public double ViewModelLeft
        {
            get { return _vm.X; }
            set { _vm.X = value; }
        }

        /// <summary>
        /// Right boundary of image
        /// </summary>
        public double ViewModelRight
        {
            get { return _vm.X + _image.CanvasSize.Width; }
            set { _vm.X = value - _image.CanvasSize.Width; }
        }

        public ImageElementRenderItem(ImageElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _vm.Controller.SizeChanged += ControllerOnSizeChanged;
            var imageController = _vm.Controller.LibraryElementController as ImageLibraryElementController;
            _image = new ImageDetailRenderItem(imageController, new Size(_vm.Width, _vm.Height), this, resourceCreator);
            _image.IsRegionsVisible = true;
            _image.IsRegionsModifiable = false;
            _image.IsHitTestVisible = false;

            AddChild(_image);

            _inkable = new InkableUIElement(imageController, this, resourceCreator);
            _inkable.Background = Colors.Transparent;
            AddChild(_inkable);
            _inkable.Transform.SetParent(_image.Transform);

            _siblingImageElementRenderItems = new List<ImageElementRenderItem>();
            _horizontalSnapLocation = Location.None;
            _verticalSnapLocation = Location.None;

            Pressed += delegate
            {
                foreach (var child in parent.GetChildren())
                {
                    if (child is ImageElementRenderItem && child != this)
                    {
                        var sibling = child as ImageElementRenderItem;
                        _siblingImageElementRenderItems.Add(sibling);
                    }
                }
            };
            Released += OnReleased;
            Dragged += OnDragged;
        }

        private void OnReleased(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // remove selection views from items this item aligned with (can be null)
            _horizontalSnapItem?.RemoveSnapSelectionView();
            _verticalSnapItem?.RemoveSnapSelectionView();
            // deselect this item
            RemoveSnapSelectionView();

            // remove all gridlines on this item
            _horizontalSnapLocation = Location.None;
            _verticalSnapLocation = Location.None;
            ManageGridLines();

            // clear the list of ImageRenderItems
            _siblingImageElementRenderItems.Clear();
        }

        private void OnDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // select this item
            CreateSnapSelectionView();

            _horizontalSnapItem = null;
            _verticalSnapItem = null;

            foreach (var sibling in _siblingImageElementRenderItems)
            {
                // only consider ImageElementRenderItems within a certain radius of the current location of this ImageElementRenderItem
                if (ViewModelLeft - sibling.ViewModelRight < UIDefaults.SnapRadius &&
                    sibling.ViewModelLeft - ViewModelRight < UIDefaults.SnapRadius &&
                    ViewModelTop - sibling.ViewModelBottom < UIDefaults.SnapRadius &&
                    sibling.ViewModelTop - ViewModelBottom < UIDefaults.SnapRadius)
                {
                    // check horizontal snap to sibling
                    var snappedHorizontally = HorizontalSnapToSibling(sibling);
                    // check vertical snap to sibling
                    var snappedVertically = VerticalSnapToSibling(sibling);

                    if (snappedHorizontally)
                    {
                        // remove selection view from the item this item horizontally snapped to previously
                        _horizontalSnapItem?.RemoveSnapSelectionView();
                        // set current sibling to be the new horizontal snap item
                        _horizontalSnapItem = sibling;
                        _horizontalSnapItem.CreateSnapSelectionView();
                    }
                    if (snappedVertically)
                    {
                        // remove selection view from the item this item vertically snapped to previously
                        _verticalSnapItem?.RemoveSnapSelectionView();
                        // set current sibling to be the new vertical snap item
                        _verticalSnapItem = sibling;
                        _verticalSnapItem.CreateSnapSelectionView();
                    }
                    if (!snappedHorizontally && !snappedVertically)
                    {
                        sibling.RemoveSnapSelectionView();
                    }
                }
                else
                {
                    sibling.RemoveSnapSelectionView();
                }
            }
  
            // create & delete gridlines according to the horizontal and vertical snap location determined above
            ManageGridLines();

            // reset snap locations to none to correctly remove gridlines when necessary
            _horizontalSnapLocation = Location.None;
            _verticalSnapLocation = Location.None;
        }

        /// <summary>
        /// Handle how this ImageElementRenderItem snaps to and align with another ImageElementRenderItem horizontally (left/right alignment)
        /// Return a boolean, which represents whether or not this item has snapped to the sibling passed in
        /// </summary>
        /// <param name="sibling"></param>
        private bool HorizontalSnapToSibling(ImageElementRenderItem sibling)
        {
            // determine the threshold distance for which snap alignment should occur
            var horizontalSnapThreshold = Math.Min(sibling.ImageCanvasSize.Width, ImageCanvasSize.Width) /
                                          UIDefaults.SnapThresholdRatio;

            // true if there doesn't exist an item that this item has previously snapped to horizontally or the previously snapped to item is farther away in the Y direction than the current sibling item in question (fixes the selection view problem and prevents this item from being trapped between two sibling items)
            var shouldCheck = _horizontalSnapItem == null || Math.Abs(ViewModelTop - _horizontalSnapItem.ViewModelTop) >
                               Math.Abs(ViewModelTop - sibling.ViewModelTop);

            bool current = true;

            // when sibling is to the left of this element and the left side of this needs to align with the right side of the sibling
            if (Math.Abs(ViewModelLeft - sibling.ViewModelRight) < horizontalSnapThreshold && shouldCheck)
            {
                ViewModelLeft = sibling.ViewModelRight;
                _horizontalSnapLocation = Location.Left;
            }

            // when sibling is to the right of this element and the right side of this needs to align with the left side of the sibling
            else if (Math.Abs(sibling.ViewModelLeft - ViewModelRight) < horizontalSnapThreshold && shouldCheck)
            {
                ViewModelRight = sibling.ViewModelLeft;
                _horizontalSnapLocation = Location.Right;
            }

            // when the left side of this needs to align with the left side of the sibling
            else if (Math.Abs(ViewModelLeft - sibling.ViewModelLeft) < horizontalSnapThreshold && shouldCheck)
            {
                ViewModelLeft = sibling.ViewModelLeft;
                _horizontalSnapLocation = Location.Left;
            }

            // when the right side of this needs to align with the right side of the sibling
            else if (Math.Abs(sibling.ViewModelRight - ViewModelRight) < horizontalSnapThreshold && shouldCheck)
            {
                ViewModelRight = sibling.ViewModelRight;
                _horizontalSnapLocation = Location.Right;
            }

            else
            {
                current = false;
            }

            // return true if this item has been determined to snap to the sibling passed in
            return current;
        }

        /// <summary>
        /// Handle how this ImageElementRenderItem snaps to and align with another ImageElementRenderItem vertically
        /// Return a boolean, which represents whether or not this item has snapped to the sibling passed in
        /// </summary>
        /// <param name="sibling"></param>
        private bool VerticalSnapToSibling(ImageElementRenderItem sibling)
        {
            // determine the threshold distance for which snap alignment should occur
            var verticalSnapThreshold = Math.Min(sibling.ImageCanvasSize.Height, ImageCanvasSize.Height) /
                                        UIDefaults.SnapThresholdRatio;

            // true if there doesn't exist an item that this item has previously snapped to or the previously snapped to vertically item is farther away in the X direction than the current sibling item in question (fixes the selection view problem and prevents this item from being trapped between two sibling items)
            var shouldCheck = _verticalSnapItem == null || Math.Abs(ViewModelLeft - _verticalSnapItem.ViewModelLeft) >
                               Math.Abs(ViewModelLeft - sibling.ViewModelLeft);

            bool current = true;

            // when the sibling is above this element and the top of this element needs to align with the bottom of the sibling
            if (Math.Abs(ViewModelTop - sibling.ViewModelBottom) < verticalSnapThreshold && shouldCheck)
            {
                ViewModelTop = sibling.ViewModelBottom;
                _verticalSnapLocation = Location.Top;
            }

            // when the sibling is below this element and the bottom of this element needs to align with the top of the sibling
            else if (Math.Abs(sibling.ViewModelTop - ViewModelBottom) < verticalSnapThreshold && shouldCheck)
            {
                ViewModelBottom = sibling.ViewModelTop;
                _verticalSnapLocation = Location.Bottom;
            }

            // when the top of this element needs to align with the top of the sibling
            else if (Math.Abs(ViewModelTop - sibling.ViewModelTop) < verticalSnapThreshold && shouldCheck)
            {
                ViewModelTop = sibling.ViewModelTop;
                _verticalSnapLocation = Location.Top;
            }

            // when the bottom of this element needs to align with the bottom of the sibling
            else if (Math.Abs(sibling.ViewModelBottom - ViewModelBottom) < verticalSnapThreshold && shouldCheck)
            {
                ViewModelBottom = sibling.ViewModelBottom;
                _verticalSnapLocation = Location.Bottom;
            }

            else
            {
                current = false;
            }

            // return true if this item has been determined to snap to the sibling passed in
            return current;
        }

        /// <summary>
        /// Create rectangle on this ImageElementRenderItem to signal that it is being aligned with
        /// </summary>
        public void CreateSnapSelectionView()
        {
            if (_snapSelectionView == null)
            {
                _snapSelectionView = new RectangleUIElement(this, ResourceCreator)
                {
                    Width = (float) ImageCanvasSize.Width,
                    Height = (float) ImageCanvasSize.Height,
                    Background = UIDefaults.SnapPreviewRectColor,
                    IsHitTestVisible = false,
                };
                AddChild(_snapSelectionView);
            }
        }

        /// <summary>
        /// Remove the snap selection view rectangle
        /// </summary>
        public void RemoveSnapSelectionView()
        {
            if (_snapSelectionView == null) return;
            RemoveChild(_snapSelectionView);
            _snapSelectionView = null;
        }

        /// <summary>
        /// Create, position, and destroy horizontal and vertical gridlines on this element when necessary for snap alignment
        /// </summary>
        private void ManageGridLines()
        {
            switch (_horizontalSnapLocation)
            {
                case Location.Left:
                    _left = CreateVerticalLine(_left);
                    _left.Transform.LocalPosition = new Vector2(0, (float)(-_left.Height / 2 + _vm.Height / 2));
                    RemoveLine(_right);
                    _right = null;
                    break;
                case Location.Right:
                    _right = CreateVerticalLine(_right);
                    _right.Transform.LocalPosition = new Vector2((float)_vm.Width, (float)(-_right.Height / 2 + _vm.Height / 2));
                    RemoveLine(_left);
                    _left = null;
                    break;
                case Location.Top:
                    break;
                case Location.Bottom:
                    break;
                case Location.None:
                    RemoveLine(_right);
                    _right = null;
                    RemoveLine(_left);
                    _left = null;
                    break;
            }

            switch (_verticalSnapLocation)
            {
                case Location.Left:
                    break;
                case Location.Right:
                    break;
                case Location.Top:
                    _top = CreateHorizontalLine(_top);
                    _top.Transform.LocalPosition = new Vector2((float)(-_top.Width / 2 + _vm.Width / 2), 0);
                    RemoveLine(_bottom);
                    _bottom = null;
                    break;
                case Location.Bottom:
                    _bottom = CreateHorizontalLine(_bottom);
                    _bottom.Transform.LocalPosition = new Vector2((float)(-_bottom.Width / 2 + _vm.Width / 2), (float)_vm.Height);
                    RemoveLine(_top);
                    _top = null;
                    break;
                case Location.None:
                    RemoveLine(_bottom);
                    _bottom = null;
                    RemoveLine(_top);
                    _top = null;
                    break;
            }
        }

        /// <summary>
        /// If not null, remove from this element the rectangle passed in
        /// </summary>
        /// <param name="line"></param>
        private void RemoveLine(RectangleUIElement line)
        {
            if (line != null)
            {
                RemoveChild(line);
            }
        }

        /// <summary>
        /// Create a vertical rectangle to represent a gridline for horizontal snap alignment when the rectangle passed in is null
        /// </summary>
        /// <returns></returns>
        private RectangleUIElement CreateVerticalLine(RectangleUIElement line)
        {
            if (line != null) return line;
            var rect = new RectangleUIElement(this, ResourceCreator)
            {
                Width = UIDefaults.GridLineWidth,
                Height = UIDefaults.GridLineLength,
                IsHitTestVisible = false,
                Background = Constants.COLOR_BLUE
            };
            AddChild(rect);
            return rect;
        }

        /// <summary>
        /// Create a horizontal rectangle to represent a gridline for vertical snap alignment when the rectangle passed in is null
        /// </summary>
        /// <returns></returns>
        private RectangleUIElement CreateHorizontalLine(RectangleUIElement line)
        {
            if (line != null) return line;
            var rect = new RectangleUIElement(this, ResourceCreator)
            {
                Width = UIDefaults.GridLineLength,
                Height = UIDefaults.GridLineWidth,
                IsHitTestVisible = false,
                Background = Constants.COLOR_BLUE,

            };
            AddChild(rect);
            return rect;
        }

        public async override Task Load()
        {
            await _image.Load();
            _vm.Controller.SetSize(_vm.Controller.Model.Width, _vm.Controller.Model.Height, false);
            _image.CanvasSize = new Size(_vm.Controller.Model.Width, _vm.Controller.Model.Height);
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
           _image.CanvasSize = new Size(width,height);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _image.Dispose();
            _image = null;
            _vm.Controller.SizeChanged -= ControllerOnSizeChanged;
            _vm = null;
            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _inkable.Width = (float) _image.CroppedImageTarget.Width;
            _inkable.Height = (float)_image.CroppedImageTarget.Height;
            _inkable.Transform.LocalPosition = _image.Transform.LocalPosition;
            base.Update(parentLocalToScreenTransform);

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;

            if (_vm == null )
                return;

           

            ds.Transform = Transform.LocalToScreenMatrix;
            base.Draw(ds);
            ds.Transform = orgTransform;

        }
    }
}
