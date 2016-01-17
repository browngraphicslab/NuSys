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

namespace NuSysApp
{
    public class InqCanvasViewModel : BaseINPC
    {
        //TODO: deal with pages better!!
        public InqCanvasModel Model { get; }

        private Size _canvasSize;

        private InqCanvasImageSource _source;

        public InqCanvasViewModel(InqCanvasModel model, Size canvasSize)
        {
            _canvasSize = canvasSize;
            Model = model;
            Model.LineFinalized += OnLineFinalized;
            Model.LineRemoved += OnLineRemoved;
            Model.LineAdded += OnLineAdded;
            Model.PageChanged +=OnPageChanged;

            _source = new InqCanvasImageSource((int)canvasSize.Width, (int)canvasSize.Height, true);
            _source.BeginDraw();
            _source.Clear(Windows.UI.Colors.White);
            _source.EndDraw();

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

            //List<Point> allP = new List<Point>();
            //List<InqLineModel> ll = Model.Lines.ToList();

            //foreach (InqLineModel ilm in ll)
            //{
            //    List<Point> currLine = new List<Point>();
            //    foreach(Point2d p in ilm.Points) {
            //        currLine.Add(new Point(p.X * Constants.MaxCanvasSize, p.Y * Constants.MaxCanvasSize));
            //    }
            //    _source.RenderLines();
            //}

            List<Point> currLine = new List<Point>();
            foreach (Point2d p in lineModel.Points)
            {
                currLine.Add(new Point(p.X * Constants.MaxCanvasSize, p.Y * Constants.MaxCanvasSize));
            }

            _source.AddLine(Windows.UI.Colors.Black, currLine.ToArray());
            _source.RenderLines();

            RaisePropertyChanged("FinalLineAdded");
        }

        public InqCanvasImageSource CanvasSource
        {
            get { return _source; }
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
