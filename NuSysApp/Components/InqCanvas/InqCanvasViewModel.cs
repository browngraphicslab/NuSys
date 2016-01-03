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
        public InqCanvasModel Model { get; }

        public ObservableCollection<InqLineView> Lines { get; set; }

        private Size _canvasSize;

        public InqCanvasViewModel(InqCanvasModel model, Size canvasSize)
        {
            _canvasSize = canvasSize;
            Model = model;
         //   Model.PartialLineAdded += OnPartialLineAdded;
            Model.LineFinalized += OnLineFinalized;
            Model.LineRemoved += OnLineRemoved;
            Model.LineAdded += OnLineAdded;
            
            Lines = new ObservableCollection<InqLineView>();

            if (model.Lines == null)
                return;

            foreach (var inqLineModel in model.Lines)
            {
                var lineView = new InqLineView(new InqLineViewModel(inqLineModel, _canvasSize));;
                Lines.Add(lineView);
            }
        }

        private void OnLineAdded(InqLineModel lineModel)
        {
            Lines.Add(new InqLineView(new InqLineViewModel(lineModel, _canvasSize)));
        }

        private void OnLineRemoved(InqLineModel lineModel)
        {
            var ls = Lines.Where(l => (l.DataContext as InqLineViewModel).Model == lineModel);
            if (!ls.Any())
                return;
            Lines.Remove(ls.First());
        }
        
        private async void OnLineFinalized(InqLineModel lineModel)
        {
            var lineView = new InqLineView(new InqLineViewModel(lineModel, _canvasSize));
            Lines.Add(lineView);

      
            RaisePropertyChanged("FinalLineAdded");
        }
        
    }
}
