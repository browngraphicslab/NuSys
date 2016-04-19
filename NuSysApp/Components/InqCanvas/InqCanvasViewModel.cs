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

            //CurrentLine = new List<RawVector2>();
            //_source = new InqCanvasImageSource((int)canvasSize.Width, (int)canvasSize.Height, true);
            //_source.BeginDraw();
            //_source.Clear(Windows.UI.Constants.color6);
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
