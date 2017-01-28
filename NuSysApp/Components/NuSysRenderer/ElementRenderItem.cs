using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using WinRTXamlToolkit.IO.Extensions;

namespace NuSysApp
{

    public class ElementRenderItem : RectangleUIElement

    {
        private ElementViewModel _vm;
        private CanvasTextLayout _textLayout;
        private Matrix3x2 _transform;
        private WrapRenderItem _tagRenderItem;

        public ElementViewModel ViewModel => _vm;
        private bool _needsTitleUpdate = true;
        private CanvasTextFormat _format;
        // Controls the bubbles showing what users are currently editing this node
        private UserBubbles _userBubbles;

        /// <summary>
        ///  true if the element is currently highlighted
        /// </summary>
        private bool _isHighlighted;

        /// <summary>
        /// the highlight background color
        /// </summary>
        private Color _highlightBackground = UIDefaults.HighlightColor;

        public ElementRenderItem(ElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(parent, resourceCreator)
        {
            _vm = vm;
            if (_vm != null) { 
                Transform.LocalPosition = new Vector2((float)_vm.X, (float)_vm.Y);
                _vm.Controller.PositionChanged += ControllerOnPositionChanged;
                _vm.Controller.SizeChanged += ControllerOnSizeChanged;
                if (_vm?.Controller?.LibraryElementController != null)
                {
                    _vm.Controller.LibraryElementController.TitleChanged += LibraryElementControllerOnTitleChanged;
                }
                _tagRenderItem = new WrapRenderItem((float)_vm.Width, parent, resourceCreator);
                _vm.Tags.CollectionChanged += TagsOnCollectionChanged;

                UpdateTextFormat();

                _vm.Controller.UserAdded += Controller_UserAdded;
                _vm.Controller.UserDropped += Controller_UserDropped;

                foreach (var tag in _vm.Tags)
                {
                    _tagRenderItem.AddChild(new TagRenderItem(tag, Parent, ResourceCreator));
                }

                AddChild(_tagRenderItem);

                _userBubbles = new UserBubbles(this, ResourceCreator);
                AddChild(_userBubbles);
            }
            if (vm?.Controller?.LibraryElementController != null)
            {
                vm.Controller.LibraryElementController.HighlightChanged += LibraryElementController_HighlightChanged;
            }
            SessionController.Instance.SessionSettings.TextScaleChanged += SessionSettingsTextScaleChanged;
            SessionController.Instance.SessionSettings.ResizeElementTitlesChanged += SessionSettingsOnResizeElementTitlesChanged;
            base.Background = Colors.Transparent;
        }

        /// <summary>
        /// method called whenever the bool for scaling titles changess
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="b"></param>
        private void SessionSettingsOnResizeElementTitlesChanged(object sender, bool b)
        {
            _needsTitleUpdate = true;
        }

        private void LibraryElementController_HighlightChanged(LibraryElementController sender, bool isHighlighted)
        {
            if (sender.LibraryElementModel == SessionController.Instance.CurrentCollectionLibraryElementModel)
            {
                return;
            }

            _isHighlighted = isHighlighted;
        }

        private void SessionSettingsTextScaleChanged(object sender, double e)
        {
            UpdateTextFormat();
        }

        private void UpdateTextFormat()
        {
            _format = new CanvasTextFormat
            {
                FontSize = 17f * (float)SessionController.Instance.SessionSettings.TextScale,
                WordWrapping = CanvasWordWrapping.Wrap,
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                FontFamily = UIDefaults.TitleFont
            };
            _needsTitleUpdate = true;
        }

        /// <summary>
        /// Fired when a user stops editing this node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">UserId of the user who stopped editing this node</param>
        private void Controller_UserDropped(object sender, string e)
        {
            _userBubbles.RemoveBubble(e);
        }

        /// <summary>
        /// Fire when a user starts editing this node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">UserId of user who is editing this node</param>
        private void Controller_UserAdded(object sender, string e)
        {
            _userBubbles.InstantiateBubble(e);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _tagRenderItem?.Dispose();
            _tagRenderItem = null;
            if (_vm?.Tags != null)
            {
                _vm.Tags.CollectionChanged -= TagsOnCollectionChanged;
            }
            if (_vm?.Controller != null)
            {
                _vm.Controller.PositionChanged -= ControllerOnPositionChanged;

                _vm.Controller.SizeChanged -= ControllerOnSizeChanged;
                if (_vm.Controller.LibraryElementController != null)
                {
                    _vm.Controller.LibraryElementController.TitleChanged -= LibraryElementControllerOnTitleChanged;
                    _vm.Controller.LibraryElementController.HighlightChanged -= LibraryElementController_HighlightChanged;

                }

                _vm.Controller.UserAdded -= Controller_UserAdded;
                _vm.Controller.UserDropped -= Controller_UserDropped;
            }
            _vm = null;

            _textLayout?.Dispose();
            _textLayout = null;
            SessionController.Instance.SessionSettings.TextScaleChanged -= SessionSettingsTextScaleChanged;
            SessionController.Instance.SessionSettings.ResizeElementTitlesChanged -= SessionSettingsOnResizeElementTitlesChanged;
            base.Dispose();
        }

        public override bool IsInteractable()
        {
            return SessionController.Instance.SessionView.FreeFormViewer.Selections.Contains(this);
        }
        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            _tagRenderItem.MaxWidth = (float)width;
            _needsTitleUpdate = true;
        }

        private void TagsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _tagRenderItem.ClearChildren();
                return;
            }

            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    _tagRenderItem.RemoveChild(_tagRenderItem.GetChildren().Where( i => ((TagRenderItem)i).Text == oldItem ).First());
                }
            }

            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    _tagRenderItem.AddChild(new TagRenderItem(newItem.ToString(), Parent, ResourceCreator));
                }
            }
        }

        private void LibraryElementControllerOnTitleChanged(object sender, string s)
        {
            UpdateTextFormat();
        }

        private void ControllerOnPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            IsDirty = true;
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // have this get the list of users currently editing the node from the usercontroller
            if (IsDisposed)
                return;

            if (_vm == null)
            {
                base.Update(parentLocalToScreenTransform);
                return;
            }

            Transform.LocalPosition = new Vector2((float)_vm.X, (float)_vm.Y);
            _tagRenderItem.Transform.LocalPosition = new Vector2(0, (float)_vm.Height + 10f);
            _tagRenderItem.IsVisible = SessionController.Instance.SessionSettings.TagsVisible;

            _userBubbles.Transform.LocalPosition = new Vector2((float)_vm.Width + 10f, 0);
            _userBubbles.Height = (float)_vm.Height;

            base.Update(parentLocalToScreenTransform);

            var resizeTitles = SessionController.Instance.SessionSettings.ResizeElementTitles;

            if (!_needsTitleUpdate && _vm != null && (resizeTitles))
                return;

            if (_vm != null)
            {
                _textLayout = new CanvasTextLayout(ResourceCreator, _vm.Title, _format, (float) Math.Max(200,_vm.Width * (resizeTitles ? 1 : _transform.M22 )), 0.0f);
            }
            _needsTitleUpdate = false;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            if (_textLayout == null)
            {
                IsDirty = true;
                return;
            }
            var oldTransform = ds.Transform;

      //      _tagRenderItem?.Draw(ds);

            if (Transform.Parent != null)
                _transform = Transform.Parent.LocalToScreenMatrix;

            var sp = Vector2.Transform(new Vector2((float)_vm.X, (float)(_vm.Y)), _transform);
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height)), _transform);

            Color color = this != SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection ? Color.FromArgb(0xdd, 0, 0, 0) : Colors.White;

            var drawBoundsHeight = (float)_textLayout.DrawBounds.Height;

            if (ViewModel.Controller.Model.ShowTitle)
            {
                if (SessionController.Instance.SessionSettings.ResizeElementTitles)
                {
                    ds.Transform = Transform.LocalToScreenMatrix;
                    ds.DrawTextLayout(_textLayout,
                        new Vector2((float) Math.Min(0, -((_textLayout.LayoutBounds.Width - _vm.Width)/2)),
                            -drawBoundsHeight - 20), color);
                }
                else
                {
                    ds.Transform = Matrix3x2.Identity;

                    ds.DrawTextLayout(_textLayout,
                        new Vector2(Math.Min(sp.X, (sp.X + spr.X)/2 - 100), sp.Y - drawBoundsHeight - 18),
                        color);
                }
            }
            // draw highlights if we are highlighted
            if (_isHighlighted)
            {
                var highlightRect = GetLocalBounds();
                var margin = 3;
                highlightRect.X -= margin;
                highlightRect.Y -= margin;
                highlightRect.Width += margin * 2;
                highlightRect.Height += margin * 2;
                ds.Transform = Transform.LocalToScreenMatrix;
                ds.DrawRectangle(highlightRect, _highlightBackground, 10,new CanvasStrokeStyle() {TransformBehavior = CanvasStrokeTransformBehavior.Fixed});
            }
            ds.Transform = oldTransform;
            base.Draw(ds);
            ds.Transform = oldTransform;

        }

        /// <summary>
        /// This method is used to return the rectangle that creates the selection bounding rect
        /// </summary>
        /// <returns></returns>
        public Rect GetSelectionBoundingRect()
        {
            if (_textLayout == null)
                return new Rect();

            // we are going to transform to the screen matrix using the local to screen matrix of the parent, which is a collection
            // that is displayed to the user
            var transform = Transform.Parent.LocalToScreenMatrix;

            // get a rectangle describing the height and width of the _tagRenderItem
            var tagsMeasurement = _tagRenderItem.IsVisible ? _tagRenderItem.GetLocalBounds() :  new Rect(0,0,0,0);

            // get the screen point for the upper left point of the element render item
            var sp = Vector2.Transform(new Vector2((float)_vm.X, (float)(_vm.Y)), transform);

            // get the screen poitn for the lower right point of the element render item including the height of the tagsMeasurement
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height + tagsMeasurement.Height + 10)), transform);

            Vector2 titlePos;
            var drawBoundsHeight = (float)_textLayout.DrawBounds.Height;
            if (SessionController.Instance.SessionSettings.ResizeElementTitles)
            {
                titlePos = Vector2.Transform(new Vector2((float)Math.Min(0, -(_textLayout.LayoutBounds.Width - _vm.Width) / 2), -drawBoundsHeight - 20),Transform.LocalToScreenMatrix);
            }
            else
            {
                titlePos = new Vector2((float)Math.Min(sp.X, (sp.X + spr.X - _textLayout.DrawBounds.Width)/2), sp.Y - drawBoundsHeight - 18);
            }

            // get the position for the upper left point of the title
            //var titlePos = new Vector2(sp.X + (spr.X - sp.X - (float)_textLayout.DrawBounds.Width) /2f, sp.Y - (float) _textLayout.DrawBounds.Height - 10);

            // return the rectangle described by these objects
            var rect = new Rect
            {
                X = Math.Min(sp.X, titlePos.X), // the upper left x is the minimum of the left axis of the node and the left axis of the title
                Y = Math.Min(sp.Y, titlePos.Y), // the upper left y is the minimum of the y axis of the node and the y axis of the title
                Width = SessionController.Instance.SessionSettings.ResizeElementTitles ? Math.Max(_textLayout.DrawBounds.Width * Transform.LocalToScreenMatrix.M22, spr.X - sp.X) : Math.Max(_textLayout.DrawBounds.Width, spr.X - sp.X), // the width is the maximum of the title width, and the width of the node itself
                Height = spr.Y - titlePos.Y // the height is the bottommost point minus the topmost point, assuming the topmost point is the y position of the title
            };
            return rect;
        }

        public Vector2 GetCenterOnScreen()
        {
            var bb = GetSelectionBoundingRect();
            return new Vector2((float)(bb.X + bb.Width/2), (float)(bb.Y + bb.Height/2));
        }

        public virtual bool HitTestTitle(Vector2 point)
        {
            var rect = new Rect
            {
                X = _vm.X,
                Y = _vm.Y - 20 - _textLayout.DrawBounds.Height,
                Width = _textLayout.DrawBounds.Width + 20,
                Height = _textLayout.DrawBounds.Height + 20
            };

            return rect.Contains(new Point(point.X, point.Y));
        }

        /// <summary>
        /// Returns the rectangle the element occupies on the screen. This method is used to get the rectangle used for culling
        /// the workspace
        /// </summary>
        /// <returns></returns>
        public Rect GetScreenRect()
        {
            // we will use the local to screen matrix to transform from local coordinates to screen coordinates
            var transform = Transform.Parent.LocalToScreenMatrix;

            // transform the local position of the element render item's upper left corner to the screen matrix
            // the local position if the offset from the upper left cornder of the workspace
            var sp = Vector2.Transform(new Vector2((float)_vm.X, (float)(_vm.Y)), transform);

            // get the tagRectangle from the children, and get it's height, if there is no tag rect the height is 0
            var tagRectHeight = _tagRenderItem?.GetLocalBounds().Height ?? 0;
            tagRectHeight = _tagRenderItem?.IsVisible == true ? tagRectHeight : 0;


            // transform the local position of the element render item's lower right corner to the screen matrix
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height + tagRectHeight)), transform);
            var rect = new Rect
            {
                X = sp.X, // set the upper left corner of the new rect to the upper left corner of the element render item
                Y = sp.Y,
                Width = spr.X - sp.X, // set the width to the lower right corner X minus the upper left corner X
                Height = spr.Y - sp.Y // set the height to the lower rigth corner Y minus the upper left corner Y
            };

            return rect;
        }

        public override Rect GetLocalBounds()
        {
            if (ViewModel == null)
                return new Rect();
            return new Rect(0, 0, ViewModel.Width, ViewModel.Height);
        }

    }
}
