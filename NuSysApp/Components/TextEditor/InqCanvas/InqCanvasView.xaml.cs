using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Graphics.Display;
using Windows.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Microsoft.Graphics.Canvas.Geometry;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp

//TODO: fix interaction with other UI elements
//fix size of canvas
{
    public sealed partial class InqCanvasView : UserControl
    {
        private bool _isEnabled;
        private uint _pointerId = uint.MaxValue;
        private IInqMode _mode;
        public bool IsPressed = false;
        private InqCanvasViewModel _viewModel;

        private PointerEventHandler _pointerPressedHandler;
        private PointerEventHandler _pointerMovedHandler;
        private PointerEventHandler _pointerReleasedHandler;
        private PointerEventHandler _pointerEnteredHandler;
        private List<CanvasGeometry> _inqLines;
        private BiDictionary<InqLineModel, CanvasGeometry> _modelToGeometries = new BiDictionary<InqLineModel, CanvasGeometry>();
        public InqCanvasView(InqCanvasViewModel vm)
        {
            this.InitializeComponent();
            _viewModel = vm;
            DataContext = vm;

            _pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
            _pointerMovedHandler = new PointerEventHandler(OnPointerMoved);
            _pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);
            _pointerEnteredHandler = new PointerEventHandler(OnPointerEntered);

            IsEnabled = false;
            // Initally, set mode to Inq drawing.

            _mode = new DrawInqMode(vm.CanvasSize, vm.Model.Id);
            _inqLines = new List<CanvasGeometry>();
            vm.Model.LineFinalizedLocally += delegate (InqLineModel lineModel)
            {
                CanvasPathBuilder line = new CanvasPathBuilder(win2dCanvas.Device);
                var start = new Point(lineModel.Points.First().X * Constants.MaxCanvasSize, lineModel.Points.First().Y * Constants.MaxCanvasSize);
                line.BeginFigure((float)start.X, (float)start.Y);
                List<Point> toExamine = new List<Point>();
                foreach (Point2d p in lineModel.Points.Skip(1))
                {
                    var look = new Point((float)(p.X * Constants.MaxCanvasSize), (float)(p.Y * Constants.MaxCanvasSize));
                    toExamine.Add(look);
                    line.AddLine((float)(p.X * Constants.MaxCanvasSize), (float)(p.Y * Constants.MaxCanvasSize));
                }
                line.EndFigure(CanvasFigureLoop.Open);
                CanvasGeometry geom = CanvasGeometry.CreatePath(line);
                _inqLines.Add(geom);
                _currentLine.Clear();
                _modelToGeometries.Add(lineModel, geom);
                win2dCanvas.Invalidate();
            };

            vm.Model.LineRemoved += delegate(InqLineModel model)
            {
                if (!_modelToGeometries.ContainsKey(model))
                    return;

                _inqLines.Remove(_modelToGeometries[model]);
                _modelToGeometries.Remove(model);
                win2dCanvas.Invalidate();
            };
        }

        public InqCanvasViewModel ViewModel
        {
            get { return (InqCanvasViewModel)DataContext; }
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if ((e.GetCurrentPoint(this) as PointerPoint).Properties.IsBarrelButtonPressed)
            {

            }
        }

        public void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {

            if (_pointerId != uint.MaxValue)
            {
                return;
            }

            _pointerId = e.Pointer.PointerId;
            if (_mode is DrawInqMode)
            {
                CapturePointer(e.Pointer);
            }

            AddHandler(PointerMovedEvent, _pointerMovedHandler, true);
            AddHandler(PointerReleasedEvent, _pointerReleasedHandler, true);
            IsPressed = true;
            _mode.OnPointerPressed(this, e);
            
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                return;
            }

            _mode.OnPointerMoved(this, e);
          
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                return;
            }

            RemoveHandler(PointerMovedEvent, _pointerMovedHandler);
            RemoveHandler(PointerReleasedEvent, _pointerReleasedHandler);
            _pointerId = uint.MaxValue;
            if (this.PointerCaptures != null && this.PointerCaptures.Count != 0)
            {
                ReleasePointerCapture(e.Pointer);
            }
            IsPressed = false;

            _mode.OnPointerReleased(this, e);
          

        }

        /// <summary>
        /// Turns erasing on or off
        /// </summary>
        /// <param name="erase"></param>
        public void SetErasing(bool erase)
        {
            if (erase)
            {
                _mode = new EraseInqMode();
            }
            else
            {
                _mode = new DrawInqMode(_viewModel.CanvasSize, _viewModel.Model.Id);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {

                if (value)
                {
                    AddHandler(PointerPressedEvent, _pointerPressedHandler, true);
                    AddHandler(PointerEnteredEvent, _pointerEnteredHandler, true);
                }
                else
                {
                    RemoveHandler(PointerPressedEvent, _pointerPressedHandler);
                    RemoveHandler(PointerMovedEvent, _pointerMovedHandler);
                    RemoveHandler(PointerReleasedEvent, _pointerReleasedHandler);
                    RemoveHandler(PointerEnteredEvent, _pointerEnteredHandler);
                }
                _isEnabled = value;
                IsHitTestVisible = value;
            }
        }

        public IInqMode Mode
        {
            get { return _mode; }
        }


        CompositeTransform _trans = new CompositeTransform();
        public CompositeTransform Transform
        {
            set
            {
                _trans = value;
                win2dCanvas.Invalidate();
            }
            get
            {
                return _trans;
            }
        }

        private List<Point> _currentLine = new List<Point>();
        public void DrawContinuousLine(Point next)
        {
            _currentLine.Add(Transform.Inverse.TransformPoint(next));
            win2dCanvas.Invalidate();
        }


        private void CanvasControl_Draw(CanvasControl control, CanvasDrawEventArgs args)
        {

            Matrix3x2 translation = Matrix3x2.CreateTranslation((float)_trans.TranslateX, (float)_trans.TranslateY);
            Matrix3x2 scale = Matrix3x2.CreateScale((float)_trans.ScaleX, (float)_trans.ScaleY);
            Matrix3x2 toOrigin = Matrix3x2.CreateTranslation((float)(_trans.CenterX + _trans.TranslateX), (float)(_trans.CenterY + _trans.TranslateY));
            Matrix3x2 fromOrigin = Matrix3x2.CreateTranslation((float)-(_trans.CenterX + _trans.TranslateX), (float)-(_trans.CenterY + _trans.TranslateY));
            args.DrawingSession.Transform = fromOrigin * translation * scale * toOrigin;
            if (_currentLine.Count > 1)
            {
                Point prev = _currentLine.First();
                foreach (Point p in _currentLine.Skip(1))
                {
                    args.DrawingSession.DrawLine((float)prev.X, (float)prev.Y, (float)p.X, (float)p.Y, Colors.Black, 2);
                    prev = p;
                }
            }
            foreach (CanvasGeometry line in _inqLines)
            {
                args.DrawingSession.DrawGeometry(line, Colors.Black, 2);    
            }
        }
    }
}