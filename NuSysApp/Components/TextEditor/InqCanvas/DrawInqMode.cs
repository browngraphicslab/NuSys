using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class DrawInqMode : IInqMode
    {
        //ink canvas
        //private InqCanvasView _view;

        //current strokes
        private InqLineModel _inqLineModel;

        private string _canvasId;
        private Size _canvasSize;

        private Point _offset;
        private Rect _clip;

        public DrawInqMode(Size canvasSize, string canvasID)
        {
            _canvasSize = canvasSize;
            _canvasId = canvasID;
            //_s = i;
        }

        


        public void OnPointerPressed(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            _inqLineModel = new InqLineModel(SessionController.Instance.GenerateId());
            _inqLineModel.InqCanvasId = _canvasId;
            _inqLineModel.Stroke = new SolidColorBrush(Colors.Black);
            _inqLineModel.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);


            //TODO - deal with pages
            //_inqLineModel.Page = _view.ViewModel.Page;
            
            var currentPoint = e.GetCurrentPoint(inqCanvas).Position;
            var transformedPoint = inqCanvas.Transform.Inverse.TransformPoint(new Point(currentPoint.X, currentPoint.Y));
            _inqLineModel.AddPoint(new Point2d((transformedPoint.X) / Constants.MaxCanvasSize, (transformedPoint.Y) / Constants.MaxCanvasSize));
            inqCanvas.DrawContinuousLine(currentPoint);

        }

        public void OnPointerMoved(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {

            var currentPoint = e.GetCurrentPoint(inqCanvas).Position;
            var transformedPoint = inqCanvas.Transform.Inverse.TransformPoint(new Point(currentPoint.X, currentPoint.Y));
            _inqLineModel.AddPoint(new Point2d((transformedPoint.X ) / Constants.MaxCanvasSize, (transformedPoint.Y ) / Constants.MaxCanvasSize));
            //var vv = vm.Transform.TransformPoint();
            inqCanvas.DrawContinuousLine(currentPoint);
        }

        public async void OnPointerReleased(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {

            var currentPoint = e.GetCurrentPoint(inqCanvas);

            var transformedPoint = inqCanvas.Transform.Inverse.TransformPoint(new Point(currentPoint.Position.X, currentPoint.Position.Y));
            _inqLineModel.AddPoint(new Point2d((transformedPoint.X) / Constants.MaxCanvasSize, (transformedPoint.Y) / Constants.MaxCanvasSize));

            _inqLineModel.IsGesture = currentPoint.Properties.IsBarrelButtonPressed || currentPoint.Properties.IsRightButtonPressed;

            var inqCanvasModel = inqCanvas.ViewModel.Model;

         //   var m = new InqLineModel(_inqLineModel.Id);
          //  m.Points = new ObservableCollection<Point2d>(_inqLineModel.Points);
            inqCanvasModel.FinalizeLineLocally(_inqLineModel);

            var request = new FinalizeInkRequest( new Message(await _inqLineModel.Pack()));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }
    }
}
