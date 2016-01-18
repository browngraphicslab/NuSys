using System;
using System.Collections.Generic;
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
        private Point _start;

        private string _canvasId;
        private Size _canvasSize;

        private InqCanvasImageSource _s;

        public DrawInqMode(Size canvasSize, string canvasID, InqCanvasImageSource i)
        {
            _canvasSize = canvasSize;
            _canvasId = canvasID;
            _s = i;
        }

        public void OnPointerPressed(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            _inqLineModel = new InqLineModel(SessionController.Instance.GenerateId());
            _inqLineModel.InqCanvasId = _canvasId;
            _inqLineModel.Stroke = new SolidColorBrush(Colors.Black);
            _inqLineModel.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);


            //TODO - deal with pages
            //_inqLineModel.Page = _view.ViewModel.Page;
            
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _inqLineModel.AddPoint(new Point2d(currentPoint.Position.X / _canvasSize.Width, currentPoint.Position.Y/ _canvasSize.Height));

            _start = currentPoint.Position;

            _s.DrawContinuousLine(new Point(0, 0), new Rect(new Point(5000, 5000), new Point(8000, 8000)));
        }

        public void OnPointerMoved(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {

            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _inqLineModel.AddPoint(new Point2d(currentPoint.Position.X / _canvasSize.Width, currentPoint.Position.Y / _canvasSize.Height));

            Rect clip = new Rect(_start, currentPoint.Position);
            _s.DrawContinuousLine(new Point(0, 0), new Rect(new Point(5000, 5000), new Point(8000, 8000)));
            _s.DrawContinuousLine(new Point(500, 500), new Rect(new Point(5000, 5000), new Point(8000, 8000)));
        }

        public async void OnPointerReleased(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            _s.EndContinuousLine();

            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _inqLineModel.AddPoint(new Point2d(currentPoint.Position.X / _canvasSize.Width, currentPoint.Position.Y / _canvasSize.Height));

            var request = new FinalizeInkRequest( new Message(await _inqLineModel.Pack()));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }
    }
}
