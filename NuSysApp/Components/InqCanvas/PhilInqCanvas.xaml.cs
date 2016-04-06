﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PhilInqCanvas : UserControl
    {
        InkManager inkManager = new InkManager();

        InkSynchronizer inkSynchronizer;

        IReadOnlyList<InkStroke> pendingDry;
        int deferredDryDelay;

        CanvasRenderTarget renderTarget;

        List<Point> selectionPolylinePoints;
        Rect? selectionBoundingRect;

        CanvasTextFormat textFormat;

        bool needToCreateSizeDependentResources;
        bool needToRedrawInkSurface;

        bool showTextLabels;

        public enum DryInkRenderingType
        {
            BuiltIn,
            Lines,
            Geometry,
        }

        public List<DryInkRenderingType> DryInkRenderingTypes { get { return Utils.GetEnumAsList<DryInkRenderingType>(); } }
        public DryInkRenderingType SelectedDryInkRenderingType { get; set; }

        private double _prevZoom = 1;

        public PhilInqCanvas()
        {
            this.InitializeComponent();

            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;

            // By default, pen barrel button or right mouse button is processed for inking
            // Set the configuration to instead allow processing these input on the UI thread
            inkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;

            inkCanvas.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            inkCanvas.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            inkCanvas.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;

            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;

            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;

            inkCanvas.InkPresenter.IsInputEnabled = false;

            inkSynchronizer = inkCanvas.InkPresenter.ActivateCustomDrying();

            textFormat = new CanvasTextFormat();

            // Set defaults
            SelectedDryInkRenderingType = DryInkRenderingType.BuiltIn;
            SelectColor();
            showTextLabels = true;

            needToCreateSizeDependentResources = true;

        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            foreach (var s in args.Strokes)
            {
                inkManager.AddStroke(s);
            }

            Debug.Assert(pendingDry == null);

            pendingDry = inkSynchronizer.BeginDry();

            canvasControl.Invalidate();
        }

        private void StrokeInput_StrokeStarted(InkStrokeInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            ClearSelection();

            canvasControl.Invalidate();
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            var removed = args.Strokes;
            var strokeList = inkManager.GetStrokes().Except(removed).ToList();

            inkManager = new InkManager();
            strokeList.ForEach(inkManager.AddStroke);

            ClearSelection();

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            selectionPolylinePoints = new List<Point>();
            selectionPolylinePoints.Add(args.CurrentPoint.RawPosition);

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerMoved(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            selectionPolylinePoints.Add(args.CurrentPoint.RawPosition);

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerReleased(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            selectionPolylinePoints.Add(args.CurrentPoint.RawPosition);

            selectionBoundingRect = inkManager.SelectWithPolyLine(selectionPolylinePoints);

            selectionPolylinePoints = null;

            canvasControl.Invalidate();
        }

        private void ClearSelection()
        {
            selectionPolylinePoints = null;
            selectionBoundingRect = null;
        }

        private void CreateSizeDependentResources()
        {
            renderTarget = new CanvasRenderTarget(canvasControl, canvasControl.Size);

            textFormat.FontSize = (float)canvasControl.Size.Width / 10.0f;

            needToCreateSizeDependentResources = false;
            needToRedrawInkSurface = true;
        }

        private async Task LoadThumbnailResources(CanvasControl sender)
        {
            var thumbnailFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/InkThumbnailStrokes.bin"));
            using (var stream = await thumbnailFile.OpenReadAsync())
            {
                LoadStrokesFromStream(stream.AsStreamForRead());
            }
        }

        private void canvasControl_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            CreateSizeDependentResources();

        }



        private void DrawSelectionBoundingRect(CanvasDrawingSession ds)
        {
            if (selectionBoundingRect == null ||
                selectionBoundingRect.Value.Width == 0 ||
                selectionBoundingRect.Value.Height == 0 ||
                selectionBoundingRect.Value.IsEmpty)
            {
                return;
            }

            ds.DrawRectangle(selectionBoundingRect.Value, Colors.Magenta);
        }

        private void DrawDryInk_LinesMethod(CanvasDrawingSession ds, IReadOnlyList<InkStroke> strokes)
        {
            //
            // This shows off the fact that apps can use the custom drying path
            // to render dry ink using Win2D, and not necessarily 
            // rely on the built-in rendering in CanvasDrawingSession.DrawInk.
            //
            foreach (var stroke in strokes)
            {
                var color = stroke.DrawingAttributes.Color;

                var inkPoints = stroke.GetInkPoints().Select(point => point.Position.ToSystemVector2()).ToList();

                for (int i = 1; i < inkPoints.Count; i++)
                {
          //          ds.DrawLine(inkPoints[i - 1], inkPoints[i], color);
          //          ds.DrawCircle(inkPoints[i], 3, color);
                }
            }
        }

        private void DrawDryInk_GeometryMethod(CanvasDrawingSession ds, IReadOnlyList<InkStroke> strokes)
        {
            //
            // This converts the ink strokes to geometry, then draws the geometry outline
            // with a dotted stroke style.
            //
            var strokeStyle = new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dot };

            var strokesGroupedByColor = from stroke in strokes
                                        group stroke by stroke.DrawingAttributes.Color into strokesOfColor
                                        select strokesOfColor;

            foreach (var strokesOfColor in strokesGroupedByColor)
            {
         //       var geometry = CanvasGeometry.CreateInk(ds, strokesOfColor.ToList()).Outline();

           //     ds.DrawGeometry(geometry, strokesOfColor.Key, 1, strokeStyle);
            }
        }

        private void DrawStrokeCollectionToInkSurface(IReadOnlyList<InkStroke> strokes)
        {
            using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
            {
                switch (SelectedDryInkRenderingType)
                {
                    case DryInkRenderingType.BuiltIn:
                        ds.DrawInk(strokes);
                        break;

                    case DryInkRenderingType.Lines:
                        //DrawDryInk_LinesMethod(ds, strokes);
                        break;

                    case DryInkRenderingType.Geometry:
                      //  DrawDryInk_GeometryMethod(ds, strokes);
                        break;
                }
            }
        }




        private void canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (needToCreateSizeDependentResources)
            {
                CreateSizeDependentResources();
            }

            if (needToRedrawInkSurface)
            {
                ClearInkSurface();
                DrawStrokeCollectionToInkSurface(inkManager.GetStrokes());

                needToRedrawInkSurface = false;
            }

         

            if (pendingDry != null && deferredDryDelay == 0)
            {
                // Incremental draw only.
                DrawStrokeCollectionToInkSurface(pendingDry);

                // Register to call EndDry on the next-but-one draw,
                // by which time our dry ink will be visible.
                deferredDryDelay = 1;
                CompositionTarget.Rendering += DeferredEndDry;
            }

            args.DrawingSession.DrawImage(renderTarget);

            DrawSelectionBoundingRect(args.DrawingSession);
           // DrawSelectionLasso(sender, args.DrawingSession);
        }

        private void DeferredEndDry(object sender, object e)
        {
            Debug.Assert(pendingDry != null);

            if (deferredDryDelay > 0)
            {
                deferredDryDelay--;
            }
            else
            {
                CompositionTarget.Rendering -= DeferredEndDry;

                pendingDry = null;

                inkSynchronizer.EndDry();
            }
        }

        const string saveFileName = "savedFile.bin";

        private void DeleteSelected_Clicked(object sender, RoutedEventArgs e)
        {
            inkManager.DeleteSelected();

            selectionBoundingRect = null;

            needToRedrawInkSurface = true;

            canvasControl.Invalidate();
        }

        private async void LoadStrokesFromStream(Stream stream)
        {
            await inkManager.LoadAsync(stream.AsInputStream());

            needToRedrawInkSurface = true;
        }

        private async void Load_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(saveFileName))
                {
                    LoadStrokesFromStream(stream);
                }
            }
            catch (FileNotFoundException)
            {
                MessageDialog dialog = new MessageDialog("No saved data was found.");
                await dialog.ShowAsync();
            }
            canvasControl.Invalidate();
        }

        private async void Save_Clicked(object sender, RoutedEventArgs e)
        {
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(saveFileName, CreationCollisionOption.ReplaceExisting))
            {
                await inkManager.SaveAsync(stream.AsOutputStream());
            }
        }

        void ClearInkSurface()
        {
            using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.Transparent);
            }
        }

        private void Clear_Clicked(object sender, RoutedEventArgs e)
        {
            inkManager = new InkManager();

            needToRedrawInkSurface = true;

            canvasControl.Invalidate();
        }



        void SelectColor()
        {
     


            InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Crimson;
            drawingAttributes.PenTip = PenTipShape.Rectangle;
            drawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.CreateRotation((float)Math.PI / 4);
            drawingAttributes.Size = new Size(2, 6);
            drawingAttributes.Color = Colors.Chartreuse;
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
