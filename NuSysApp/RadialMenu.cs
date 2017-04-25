using Microsoft.Graphics.Canvas;
using NuSysApp.NusysRenderer;
using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
  

        public class RadialMenu : EllipseUIElement
        {
            private ButtonUIElement _addElementButton;
            private ButtonUIElement _openLibraryButton;
            private StackLayoutManager _buttonLayoutManager;

            private AddElementMenuUIElement _addElementMenu;

            private bool _disabled = false;

            private bool _movable = false;

            private int _selectedButtonIndex;
            /// <summary>
            ///  the initial drag position of the floating menu view
            /// </summary>
            private Vector2 _initialDragPosition;

            private float _originalHeight;
            private float _originalWidth;

        private EllipseButtonUIElement _selectedHighlight;

        private EllipseUIElement _highlight;
        private RadialMenuButtons _radialMenuButtons;

        public RadialMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, List<RadialMenuButtonContainer> buttonContainers) : base(parent, resourceCreator)
            {
                // set the default height and width of the floating menu view
                Height = 100;
                Width = 100;


            // set the default background
            Background = Colors.Transparent;

            _selectedHighlight = new EllipseButtonUIElement(this, Canvas)
            {
                Width = 90,
                Height = 90,
                Background = Colors.SkyBlue
                //IsVisible = false
            };
            _selectedHighlight.IsVisible = false;
            //_selectedHighlight.Transform.LocalPosition = new Vector2(-10000, -10000); //ector2(thing.Transform.Parent.LocalPosition.X, thing.Transform.Parent.LocalPosition.Y);
            AddChild(_selectedHighlight);

            _highlight = new EllipseUIElement(this, Canvas)
            {
                Background = Colors.Yellow,
                Width = 50,
                Height = 50,
                IsVisible = false
            };
            AddChild(_highlight);

            _addElementButton = new EllipseButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "add element")
                {
                    IsVisible = false
                };
                AddChild(_addElementButton);

                _openLibraryButton = new EllipseButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "open library")
                {
                    IsVisible = false
                };

                AddChild(_openLibraryButton);

            

          


            _radialMenuButtons = new RadialMenuButtons(this, Canvas, buttonContainers)
                {
                    IsVisible = false
                };

                AddChild(_radialMenuButtons);

                // initialize the layout manager
                _buttonLayoutManager = new StackLayoutManager
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Spacing = 20
                };
                _buttonLayoutManager.SetSize(Width, Height);
                _buttonLayoutManager.SetMargins(5, 5);
                _buttonLayoutManager.AddElement(_addElementButton);
                _buttonLayoutManager.AddElement(_openLibraryButton);
                _buttonLayoutManager.ArrangeItems(); // arrange the items only so widths and heights are properly instantiated

                _originalWidth = Width;
                _originalHeight = Height;

            //SetAccessibilitySize(SessionController.Instance.SessionSettings.TextScale);

                DragStarted += RadialMenu_DragStarted;
                Dragged += RadialMenuOnDragged;
                
                //Tapped += OpenLibraryButtonOnTapped;
                DragCompleted += RadialMenuOnDragCompleted;
                
        }


        private async void RadialMenuOnDragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (!_movable)
            {


                //await StaticServerCalls.AddElementToWorkSpace(pointer.CurrentPoint, NusysConstants.ElementType.Text).ConfigureAwait(false);
                var thing = this.HitTest(pointer.CurrentPoint);
                var casted = thing?.Parent as RadialMenuButtons;
                if (casted != null && thing != this && thing != null)
                {
                    //thing.IsVisible = false;
                    _radialMenuButtons.getActionFromShape(thing as RectangleUIElement).Invoke(this.Transform.LocalPosition);
                    //_selectedHighlight.IsVisible = true;
                   // _selectedHighlight.IsVisible = true;
                   // _selectedHighlight.Transform.LocalPosition = new Vector2(thing.Transform.Parent.LocalPosition.X - 20, thing.Transform.Parent.LocalPosition.Y - 3); //only transforms, visible doesn't work?

                }
                else
                {
                   // _selectedHighlight.IsVisible = false;

                    //_selectedHighlight.Transform.LocalPosition = new Vector2(-10000,-10000); //ector2(thing.Transform.Parent.LocalPosition.X, thing.Transform.Parent.LocalPosition.Y);

                }
                _selectedHighlight.IsVisible = false;
                IsVisible = false;
                //System.Diagnostics.Debug.WriteLine(item.type);
                _radialMenuButtons.IsVisible = false;

            }
        }

        private void RadialMenu_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            
                _initialDragPosition = Transform.LocalPosition;
            
                _radialMenuButtons.IsVisible = true;
            
        }

    

        private void RadialMenuOnDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //if (_movable)
            //{
            //Transform.LocalPosition = _initialDragPosition + pointer.Delta;
            
            //
            //var x = pointer.CurrentPoint.X - _initialDragPosition.X;
            //var y = pointer.CurrentPoint.Y - _initialDragPosition.Y;
            //_highlight.IsVisible = false;
            //_highlight.Transform.LocalPosition = new Vector2(x, y);

            //var distance = DistanceFromPoints(x, y, 50, 50);
            //if (distance > 100 && distance < 150)
            //{

            //    //if (AngleFromPoints(50, 50, x, y) < 3.14 / 2)
            //    //{
            //        _highlight.IsVisible = true;

            //        var centerX = 0;
            //        var centerY = 0;
            //        var radius = 120;
            //        var angle = AngleFromPoints(50,50,x,y);
            //        _highlight.Transform.LocalPosition = new Vector2((float)(centerX + Math.Cos(angle) * radius), (float)(centerY + Math.Sin(angle) * radius));

            //   // }
            //}


            var thing = this.HitTest(pointer.CurrentPoint);
            var casted = thing?.Parent as RadialMenuButtons;
            if (casted != null && thing != this && thing != null)
            {
                //thing.IsVisible = false;
                //_selectedHighlight.IsVisible = true;
                _selectedHighlight.IsVisible = true;
                _selectedHighlight.Transform.LocalPosition = new Vector2(thing.Transform.Parent.LocalPosition.X - 20, thing.Transform.Parent.LocalPosition.Y-3); //only transforms, visible doesn't work?

            } else
            {
                _selectedHighlight.IsVisible = false;

                //_selectedHighlight.Transform.LocalPosition = new Vector2(-10000,-10000); //ector2(thing.Transform.Parent.LocalPosition.X, thing.Transform.Parent.LocalPosition.Y);

            }
            /*
            for (var i = 0; i < 8; i++)
            {
                var centerX = 0;
                var centerY = 0;
                var radius = 120;
                var rect = new EllipseUIElement(this, Canvas);
                rect.Width = 100;
                rect.Height = 100;
                rect.Background = Colors.LightSkyBlue;
                AddChild(rect);
                var factor = .25 * i;
                rect.Transform.LocalPosition = new Vector2((float)(centerX + Math.Cos(Math.PI * factor) * radius), (float)(centerY + Math.Sin(Math.PI * factor) * radius));
            }*/
            //}
        }

        /// <summary>
        /// Get distance between 2 points
        /// </summary>
        private double DistanceFromPoints(double x1, double y1, double x2, double y2)
        {
            var x = x2 - x1;
            var y = y2 - y1;
            return Math.Sqrt(x * x + y * y);
        }

        private double AngleFromPoints(double x1, double y1, double x2, double y2)
        {
            var x = x2 - x1;
            var y = y2 - y1;
            return Math.Atan2(y,x);
        }

        private void OpenLibraryButtonOnTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            if (_movable)
            {
                Background = Colors.Yellow;
                _movable = false;
            } else
            {
                Background = Colors.Transparent;
                _movable = true;
            }
        }

    }
    }
