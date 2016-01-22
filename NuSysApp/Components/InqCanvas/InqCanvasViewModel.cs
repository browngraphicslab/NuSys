using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class InqCanvasViewModel : BaseINPC
    {
        //TODO: deal with pages better!!
        public InqCanvasModel Model { get; }

        private Size _canvasSize;

        private List<SharpDX.Direct2D1.PathGeometry> _lines = new List<SharpDX.Direct2D1.PathGeometry>();

        public InqCanvasViewModel(InqCanvasModel model, Size canvasSize)
        {
            _canvasSize = canvasSize;
            Model = model;
            Model.LineFinalized += OnLineFinalized;
            Model.LineRemoved += OnLineRemoved;
            Model.LineAdded += OnLineAdded;
            Model.PageChanged +=OnPageChanged;

            //_source = new InqCanvasImageSource((int)canvasSize.Width, (int)canvasSize.Height, true);
            //_source.BeginDraw();
            //_source.Clear(Windows.UI.Colors.White);
            //_source.EndDraw();

            if (model.Lines == null)
                return;

        }

        private void OnPageChanged(int page)
        {
            //what to do here???
        }

        private void OnLineAdded(InqLineModel lineModel)
        {
            //AddLine(new InqLineView(new InqLineViewModel(lineModel, _canvasSize)));
        }

        private void OnLineRemoved(InqLineModel lineModel)
        {
            //var ls = _lines.Where(l => (l.DataContext as InqLineViewModel).Model == lineModel);
            //if (!ls.Any())
            //    return;
            //RemoveLine(ls.First());
        }
        
        private async void OnLineFinalized(InqLineModel lineModel)
        {

            SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(RenderTarget.Factory);
            GeometrySink sink = geometry.Open();
            RawVector2 start = new RawVector2();
            Windows.Foundation.Point first = Transform.Inverse.TransformPoint(new Point(lineModel.Points.First().X * Constants.MaxCanvasSize, lineModel.Points.First().Y * Constants.MaxCanvasSize));
            start.X = (float)(first.X);
            start.Y = (float)(first.Y);
            sink.BeginFigure(start, new FigureBegin());
            foreach (Point2d p in lineModel.Points.Skip(1))
            {
                Point transformed = new Point(p.X * Constants.MaxCanvasSize, p.Y * Constants.MaxCanvasSize);
                transformed = Transform.Inverse.TransformPoint(transformed);
                RawVector2 vec = new RawVector2();
                vec.X = (float)(transformed.X);
                vec.Y = (float)(transformed.Y);
                sink.AddLine(vec);
            }
            sink.EndFigure(new FigureEnd());
            sink.Close();
            sink.Dispose();
            _lines.Add(geometry);

        }

        public List<SharpDX.Direct2D1.PathGeometry> Lines
        {
            get { return _lines; }
        }

        public SharpDX.Direct2D1.RenderTarget RenderTarget
        {
            get; set;
        }

        //transform the draw
        public CompositeTransform Transform
        {
            get; set;
        }

        public Size CanvasSize {
            get { return _canvasSize; }
            set
            {
                _canvasSize = value;

                RaisePropertyChanged("CanvasSize.Height");
                RaisePropertyChanged("CanvasSize.Height");
                RaisePropertyChanged("CanvasSize");

            }
        }
    }
}
