using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class InqCanvasViewModel : BaseINPC
    {
        public InqCanvasModel Model { get; }
        public InqCanvasView View { get; }
        public InqCanvasViewModel(InqCanvasView inqCanvasView, InqCanvasModel model)
        {
            Model = model;
            this.Model.OnPartialLineAddition += PartialLineAdditionHandler;
            this.Model.OnFinalizedLine += FinalLineAdditionHandler;
            inqCanvasView.ViewModel = this;
            View = inqCanvasView;
        }

        public void AddTemporaryPoint(Point p)
        {
            Model.AddTemporaryPoint(p);
        }
        public InqLineModel LastPartialLineModel { get; set; }
        private void PartialLineAdditionHandler(object source, AddLineEventArgs e)
        {
            if (e.AddedLineModel != LastPartialLineModel)
            {
                LastPartialLineModel = e.AddedLineModel;
                RaisePropertyChanged("PartialLineAdded");
            }
        }

        public InqLineModel FinalLineModel { get; set; }
        private void FinalLineAdditionHandler(InqLineModel lineModel)
        {
            if (lineModel != FinalLineModel)
            {
                FinalLineModel = lineModel;
                RaisePropertyChanged("FinalLineAdded");
            }
        }
        public void RemoveLine(InqLineView lineView)
        {
            
        }
    }
}
