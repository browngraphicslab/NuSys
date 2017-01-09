﻿using System;
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

                foreach (var tag in _vm.Tags)
                {
                    _tagRenderItem.AddChild(new TagRenderItem(tag, Parent, ResourceCreator));
                }

                AddChild(_tagRenderItem);
            }
            if (vm?.Controller?.LibraryElementController != null)
            {
                vm.Controller.LibraryElementController.HighlightChanged += LibraryElementController_HighlightChanged;
            }
            SessionController.Instance.SessionSettings.TextScaleChanged += SessionSettingsTextScaleChanged;
            base.Background = Colors.Transparent;
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
                FontFamily = "/Assets/fonts/freightsans.ttf#FreightSans BookSC"
            };
            _needsTitleUpdate = true;
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
            }
            _vm = null;

            _textLayout?.Dispose();
            _textLayout = null;
            SessionController.Instance.SessionSettings.TextScaleChanged -= SessionSettingsTextScaleChanged;
            base.Dispose();
        }

        public override bool IsInteractable()
        {
            return SessionController.Instance.SessionView.FreeFormViewer.Selections.Contains(this);
        }
        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            _tagRenderItem.MaxWidth = (float)width;
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
            _needsTitleUpdate = true;
        }

        private void ControllerOnPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            IsDirty = true;
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;

            if (_vm == null)
            {
                base.Update(parentLocalToScreenTransform);
                return;
            }

            Transform.LocalPosition = new Vector2((float)_vm.X, (float)_vm.Y);
            _tagRenderItem.Transform.LocalPosition = new Vector2(0, (float)_vm.Height + 10f);

            base.Update(parentLocalToScreenTransform);

            if (!_needsTitleUpdate && _vm != null)
                return;

            if (_vm != null)
                _textLayout = new CanvasTextLayout(ResourceCreator, _vm.Title, _format, 200, 0.0f);
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

            if (SessionController.Instance.SessionSettings.ResizeElementTitles)
            {
                ds.Transform = Transform.LocalToScreenMatrix;
                ds.DrawTextLayout(_textLayout,new Vector2(0,-drawBoundsHeight - 20),color );
            }
            else
            {
                ds.Transform = Matrix3x2.Identity;

                ds.DrawTextLayout(_textLayout,
                    new Vector2(sp.X + (spr.X - sp.X - 200f)/2f, sp.Y - drawBoundsHeight - 18),
                    color);
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
                ds.DrawRectangle(highlightRect, _highlightBackground, 10);
            }
            ds.Transform = oldTransform;
            base.Draw(ds);
            ds.Transform = oldTransform;

        }

        public Rect GetSelectionBoundingRect()
        {
            if (_textLayout == null)
                return new Rect();
            var transform = Transform.Parent.LocalToScreenMatrix;
            var sp = Vector2.Transform(new Vector2((float)_vm.X, (float)(_vm.Y)), transform);
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height)), transform);
            var titlePos = new Vector2(sp.X + (spr.X - sp.X - (float)_textLayout.DrawBounds.Width) /2f, sp.Y - (float) _textLayout.DrawBounds.Height - 10);
            var tagsMeasurement = _tagRenderItem.GetLocalBounds();
            var rect = new Rect
            {
                X = Math.Min(sp.X, titlePos.X),
                Y = Math.Min(sp.Y, titlePos.Y),
                Width = Math.Max(_textLayout.DrawBounds.Width, spr.X - sp.X),
                Height = spr.Y- titlePos.Y + tagsMeasurement.Height + 10
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

        public Rect GetScreenRect()
        {
            var transform = Transform.Parent.LocalToScreenMatrix;
            var sp = Vector2.Transform(new Vector2((float)_vm.X, (float)(_vm.Y)), transform);
            var spr = Vector2.Transform(new Vector2((float)(_vm.X + _vm.Width), (float)(_vm.Y + _vm.Height)), transform);
            var rect = new Rect
            {
                X = sp.X,
                Y = sp.Y,
                Width = spr.X - sp.X,
                Height = spr.Y - sp.Y
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
