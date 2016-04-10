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

namespace App2
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

        private double _prevZoom = 1;

        public bool InkEnabled
        {
            get { return inkCanvas.InkPresenter.IsInputEnabled; }
            set
            {
                SelectColor();
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
                        break;
                    case InqCanvasMode.Ink:
                        InkEnabled = true;
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

            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                                                      Windows.UI.Core.CoreInputDeviceTypes.Pen |
                                                      Windows.UI.Core.CoreInputDeviceTypes.Touch;

            // By default, pen barrel button or right mouse button is processed for inking
            // Set the configuration to instead allow processing these input on the UI thread
           
            inkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction =
                InkInputRightDragAction.LeaveUnprocessed;


           
            inkCanvas.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            inkCanvas.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            inkCanvas.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
            
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
            
            _inkSynchronizer = inkCanvas.InkPresenter.ActivateCustomDrying();
 
            _needToCreateSizeDependentResources = true;
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
            }

            Debug.Assert(_pendingDry == null);


            _pendingDry = _inkSynchronizer.BeginDry();
            foreach (var p in _pendingDry)
            {
                p.DrawingAttributes = drawingAttributes;
                p.PointTransform = m;
            }

            canvasControl.Invalidate();
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

            Debug.WriteLine("--------------------------");
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
            inkManager.DeleteSelected();
            inkManager.SelectWithPolyLine(_selectionPolylinePoints);
            inkManager.DeleteSelected();
            // Debug.WriteLine(rect);
            _selectionPolylinePoints = null;
            

            Invalidate(true);
        }

        private void DrawSelectionLasso(CanvasControl sender, CanvasDrawingSession ds)
        {
            if (_selectionPolylinePoints == null) return;
            if (_selectionPolylinePoints.Count == 0) return;

            CanvasPathBuilder selectionLasso = new CanvasPathBuilder(canvasControl);
            selectionLasso.BeginFigure(_selectionPolylinePoints[0].ToVector2());
            for (int i = 1; i < _selectionPolylinePoints.Count; ++i)
            {
                selectionLasso.AddLine(_selectionPolylinePoints[i].ToVector2());
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


            if (_pendingDry != null && _deferredDryDelay == 0)
            {
                // Incremental draw only.
                DrawStrokeCollectionToInkSurface(_pendingDry);

                // Register to call EndDry on the next-but-one draw,
                // by which time our dry ink will be visible.
                _deferredDryDelay = 1;
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

            Debug.Assert(_pendingDry != null);

            if (_deferredDryDelay > 0)
            {
                _deferredDryDelay--;
            }
            else
            {
                CompositionTarget.Rendering -= DeferredEndDry;

                _pendingDry = null;

                _inkSynchronizer.EndDry();
            }
            Invalidate();
        }

        private void DeleteSelected_Clicked(object sender, RoutedEventArgs e)
        {
            inkManager.DeleteSelected();
            _needToRedrawInkSurface = true;
            canvasControl.Invalidate();
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
            // Explicitly remove references to allow the Win2D controls to get garbage collected
            canvasControl.RemoveFromVisualTree();
            canvasControl = null;

            // If we don't unregister these events, the control will leak.
            inkCanvas.InkPresenter.UnprocessedInput.PointerPressed -= UnprocessedInput_PointerPressed;
            inkCanvas.InkPresenter.UnprocessedInput.PointerMoved -= UnprocessedInput_PointerMoved;
            inkCanvas.InkPresenter.UnprocessedInput.PointerReleased -= UnprocessedInput_PointerReleased;
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted -= StrokeInput_StrokeStarted;
            inkCanvas.InkPresenter.StrokesCollected -= InkPresenter_StrokesCollected;
            inkCanvas.InkPresenter.StrokesErased -= InkPresenter_StrokesErased;
        }
    }
}