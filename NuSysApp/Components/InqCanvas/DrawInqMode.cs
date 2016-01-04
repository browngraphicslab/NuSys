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
        private InqCanvasView _view;
        private InqLineModel _inqLineModel;
        private InqLineView _inqLineView;
        private InkManager _inkManager = new InkManager();

        public DrawInqMode(InqCanvasView view)
        {
            _view = view;
            // This adds the final line to the canvas, after the host send it to this client
            //(((InqCanvasViewModel)view.DataContext).Model).OnFinalizedLine += delegate(InqLineModel lineModel)
            //{
            //    var lineView = new InqLineView(new InqLineViewModel(lineModel));
            //    var points = lineModel.Points;
            //    view.Children.Add(lineView);
            //};
        }

        public void OnPointerPressed(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            _inkManager.ProcessPointerDown(e.GetCurrentPoint(inqCanvas));
            _inqLineModel = new InqLineModel(SessionController.Instance.GenerateId());
            _inqLineModel.InqCanvasId = inqCanvas.ViewModel.Model.Id;
            _inqLineModel.Stroke = new SolidColorBrush(Colors.Black);
            _inqLineModel.StrokeThickness = Math.Max(4.0 * e.GetCurrentPoint(inqCanvas).Properties.Pressure, 2);


            //TODO: add data binding for thickness and color
            var inqLineVm = new InqLineViewModel(_inqLineModel, new Size(_view.Width, _view.Height));
            _inqLineModel.Page = _view.ViewModel.Page;
            _inqLineView = new InqLineView(inqLineVm);
            
            _inqLineView.StrokeThickness = _inqLineModel.StrokeThickness;
            

            inqCanvas.ViewModel.AddLine(_inqLineView);
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _inqLineModel.AddPoint(new Point2d(currentPoint.Position.X / _view.Width, currentPoint.Position.Y/ _view.Height));
        }

        public void OnPointerMoved(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {   
            _inkManager.ProcessPointerUpdate(e.GetCurrentPoint(inqCanvas));
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _inqLineModel.AddPoint(new Point2d(currentPoint.Position.X / _view.Width, currentPoint.Position.Y / _view.Height));
                if (_inqLineModel.Points.Count > 1)
                {
                    NetworkConnector.Instance.RequestSendPartialLine(_inqLineModel.Id, ((InqCanvasViewModel)inqCanvas.DataContext).Model.Id,
                        _inqLineModel.Points[_inqLineModel.Points.Count - 2].X.ToString(),
                        _inqLineModel.Points[_inqLineModel.Points.Count - 2].Y.ToString(),
                        _inqLineModel.Points[_inqLineModel.Points.Count - 1].X.ToString(),
                        _inqLineModel.Points[_inqLineModel.Points.Count - 1].Y.ToString());
                }
        }

        public async void OnPointerReleased(InqCanvasView inqCanvas, PointerRoutedEventArgs e)
        {
            _inkManager.ProcessPointerUp(e.GetCurrentPoint(inqCanvas));
            var currentPoint = e.GetCurrentPoint(inqCanvas);
            _inqLineModel.AddPoint(new Point2d(currentPoint.Position.X / _view.Width, currentPoint.Position.Y / _view.Height));
            NetworkConnector.Instance.RequestFinalizeGlobalInk(await _inqLineModel.Pack());
            (((InqCanvasViewModel) inqCanvas.DataContext).Model).LineFinalized += delegate
            {
                inqCanvas.ViewModel.RemoveLine(_inqLineView);
            };
        }
    }
}
