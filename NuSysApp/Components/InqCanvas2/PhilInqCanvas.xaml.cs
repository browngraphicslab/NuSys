using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.Geometry;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PhilInqCanvas : UserControl
    {
        private InkManager inkManager = new InkManager();
        private InkSynchronizer _inkSynchronizer;
        private IReadOnlyList<InkStroke> _pendingDry;
        private int _deferredDryDelay;
        private CanvasRenderTarget _renderTarget;
        private List<Point> _selectionPolylinePoints;
        private bool _needToCreateSizeDependentResources;
        private bool _needToRedrawInkSurface;

        public delegate void InkStrokeEventHandler(PhilInqCanvas canvas, InkStroke stroke);
        public event InkStrokeEventHandler InkStrokeAdded;
        public event InkStrokeEventHandler InkStrokeRemoved;

        private double _prevZoom = 1;

        private bool _inkEnabled;
        public bool InkEnabled
        {
            get { return _inkEnabled; }
            set
            {
                SelectColor();
                _inkEnabled = value;
                inkCanvas.InkPresenter.IsInputEnabled = value;
            }
        }

        public enum InqCanvasMode { Ink, Erase, Disabled}

        private InqCanvasMode _mode = InqCanvasMode.Ink;
        public InqCanvasMode Mode
        {
            get { return _mode; }
            set
            {
                switch (value)
                {
                    case InqCanvasMode.Disabled:
                        InkEnabled = false;
                        inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.None;
                        break;
                    case InqCanvasMode.Ink:
                        InkEnabled = true;
                        if (inkCanvas.InkPresenter.InputProcessingConfiguration.Mode != InkInputProcessingMode.Inking)
                            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
                        break;
                    case InqCanvasMode.Erase:
                        InkEnabled = true;
                        inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.None;
                        break;
                }
            }
        }

        private CompositeTransform _transform;

        public CompositeTransform Transform
        {
            get { return _transform; }
            set
            {
                _transform = value;
                SelectColor();
            }
        }

        public CanvasControl CanvasControl
        {
            get { return canvasControl; }
        }

        public PhilInqCanvas()
        {
            this.InitializeComponent();

            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Touch;

            inkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;

            inkCanvas.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            inkCanvas.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            inkCanvas.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
            
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;

            _inkSynchronizer = inkCanvas.InkPresenter.ActivateCustomDrying();
            inkCanvas.InkPresenter.IsInputEnabled = false;
            _needToCreateSizeDependentResources = true;
        }

        public void DeleteStroke(InkStroke stroke)
        {
            stroke.Selected = true;
            inkManager.DeleteSelected();
            Invalidate(true);
        }

        public void Invalidate(bool redraw = false)
        {
            if (redraw)
            {
                _needToRedrawInkSurface = true;
            }

            canvasControl.Invalidate();
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            if (!InkEnabled)
                return;

            var inv = (MatrixTransform) Transform.Inverse;

            var m = new Matrix3x2((float) inv.Matrix.M11, (float) inv.Matrix.M12, (float) inv.Matrix.M21,
                (float) inv.Matrix.M22, (float) inv.Matrix.OffsetX, (float) inv.Matrix.OffsetY);
            InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
            foreach (var s in args.Strokes)
            {
                s.DrawingAttributes = drawingAttributes;
                s.PointTransform = m;
                inkManager.AddStroke(s);
                var ss = s.GetRenderingSegments().ToArray();
            }

            Debug.Assert(_pendingDry == null);


            _pendingDry = _inkSynchronizer.BeginDry();
            foreach (var p in _pendingDry)
            {
                p.DrawingAttributes = drawingAttributes;
                p.PointTransform = m;

                var s = p.GetRenderingSegments().ToArray();
            }

            

            canvasControl.Invalidate();
            foreach ( var inkStroke in _pendingDry) { 
                InkStrokeAdded?.Invoke(this, inkStroke);
            }
        }

        private void StrokeInput_StrokeStarted(InkStrokeInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            if (!InkEnabled)
                return;
            ClearSelection();
            canvasControl.Invalidate();
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            if (!InkEnabled)
                return;
            var removed = args.Strokes;
            var strokeList = inkManager.GetStrokes().Except(removed).ToList();

            inkManager = new InkManager();
            strokeList.ForEach(inkManager.AddStroke);

            ClearSelection();

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            if (!InkEnabled)
                return;

            _selectionPolylinePoints = new List<Point>();
            var pos = Transform.Inverse.TransformPoint(args.CurrentPoint.RawPosition);
            _selectionPolylinePoints.Add(pos);

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerMoved(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            if (!InkEnabled)
                return;
            var pos = Transform.Inverse.TransformPoint(args.CurrentPoint.RawPosition);
            _selectionPolylinePoints.Add(pos);

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerReleased(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            if (!InkEnabled)
                return;

            var pos = Transform.Inverse.TransformPoint(args.CurrentPoint.RawPosition);
            _selectionPolylinePoints.Add(pos);


            inkManager.SelectWithLine(_selectionPolylinePoints[0], _selectionPolylinePoints[_selectionPolylinePoints.Count-1]);
            var selected1 = GetSelectedStrokes();
            inkManager.DeleteSelected();
            inkManager.SelectWithPolyLine(_selectionPolylinePoints);
            var selected2 = GetSelectedStrokes();
            inkManager.DeleteSelected();
            _selectionPolylinePoints = null;
            
            foreach(var selected in selected1)
            {
                InkStrokeRemoved?.Invoke(this, selected);
            }

            foreach (var selected in selected2)
            {
                InkStrokeRemoved?.Invoke(this, selected);
            }

            Invalidate(true);
        }

        private IEnumerable<InkStroke> GetSelectedStrokes()
        {
            var selectedStrokes = new List<InkStroke>();
            return inkManager.GetStrokes().ToArray().Where(stroke => stroke.Selected == true);           
        }

        private void DrawSelectionLasso(CanvasControl sender, CanvasDrawingSession ds)
        {
            if (_selectionPolylinePoints == null) return;
            if (_selectionPolylinePoints.Count == 0) return;

            CanvasPathBuilder selectionLasso = new CanvasPathBuilder(canvasControl);
            selectionLasso.BeginFigure(_selectionPolylinePoints[0].ToSystemVector2());
            for (int i = 1; i < _selectionPolylinePoints.Count; ++i)
            {
                selectionLasso.AddLine(_selectionPolylinePoints[i].ToSystemVector2());
            }
            selectionLasso.EndFigure(CanvasFigureLoop.Open);

            CanvasGeometry pathGeometry = CanvasGeometry.CreatePath(selectionLasso);

            var inv = (MatrixTransform)Transform.Inverse.Inverse;
            var m = new Matrix3x2((float)inv.Matrix.M11, (float)inv.Matrix.M12, (float)inv.Matrix.M21,
                (float)inv.Matrix.M22, (float)inv.Matrix.OffsetX, (float)inv.Matrix.OffsetY);

            ds.Transform = m;
            ds.DrawGeometry(pathGeometry, Colors.DarkRed, 5.0f);
        }

        private void ClearSelection()
        {
            _selectionPolylinePoints = null;
        }

        private void CreateSizeDependentResources()
        {
            _renderTarget = new CanvasRenderTarget(canvasControl, canvasControl.Size);
            _needToCreateSizeDependentResources = false;
            _needToRedrawInkSurface = true;
        }


        private void canvasControl_CreateResources(CanvasControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            CreateSizeDependentResources();
        }

        private void DrawStrokeCollectionToInkSurface(IReadOnlyList<InkStroke> strokes)
        {
            var drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
            drawingAttributes.Size = new Size(2,4);
            foreach (var inkStroke in strokes)
            {
                inkStroke.DrawingAttributes = drawingAttributes;
            }

            using (var ds = _renderTarget.CreateDrawingSession())
            {
                var inv = (MatrixTransform)Transform.Inverse.Inverse;
                var m = new Matrix3x2((float)inv.Matrix.M11, (float)inv.Matrix.M12, (float)inv.Matrix.M21,
                    (float)inv.Matrix.M22, (float)inv.Matrix.OffsetX, (float)inv.Matrix.OffsetY);

                ds.Transform = m;
                ds.DrawInk(strokes);
            }
        }


        private void canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_needToCreateSizeDependentResources)
            {
                CreateSizeDependentResources();
            }

            if (_needToRedrawInkSurface)
            {
                ClearInkSurface();
                DrawStrokeCollectionToInkSurface(inkManager.GetStrokes());
                _needToRedrawInkSurface = false;
            }


            
            if (_pendingDry != null)
            {
                // Incremental draw only.
                DrawStrokeCollectionToInkSurface(_pendingDry);
                CompositionTarget.Rendering += DeferredEndDry;
            }
            
            args.DrawingSession.DrawImage(_renderTarget);
            _needToRedrawInkSurface = false;

            DrawSelectionLasso(sender, args.DrawingSession);
        }

        private void DeferredEndDry(object sender, object e)
        {
            if (_pendingDry == null)
            return;
            CompositionTarget.Rendering -= DeferredEndDry;
            _pendingDry = null;
            _inkSynchronizer.EndDry();
            Invalidate();
        }

        private void ClearInkSurface()
        {
            using (CanvasDrawingSession ds = _renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.Transparent);
            }
        }


        private void SelectColor()
        {
            InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
            drawingAttributes.PenTip = PenTipShape.Circle;
            drawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.CreateRotation((float) Math.PI/4);

            drawingAttributes.Size = new Size(2 * Transform.ScaleX, 4 * Transform.ScaleX);
            drawingAttributes.Color = Colors.Black;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
        }

        private void control_Unloaded(object sender, RoutedEventArgs e)
        {
            canvasControl.RemoveFromVisualTree();
            canvasControl = null;

            inkCanvas.InkPresenter.UnprocessedInput.PointerPressed -= UnprocessedInput_PointerPressed;
            inkCanvas.InkPresenter.UnprocessedInput.PointerMoved -= UnprocessedInput_PointerMoved;
            inkCanvas.InkPresenter.UnprocessedInput.PointerReleased -= UnprocessedInput_PointerReleased;
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted -= StrokeInput_StrokeStarted;
            inkCanvas.InkPresenter.StrokesCollected -= InkPresenter_StrokesCollected;
            inkCanvas.InkPresenter.StrokesErased -= InkPresenter_StrokesErased;
        }
    }
}