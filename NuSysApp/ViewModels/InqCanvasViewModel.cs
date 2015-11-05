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
        public Panel View { get; }
        public InqCanvasViewModel(InqCanvasModel model)
        {
            Model = model;
            Model.OnPartialLineAddition += PartialLineAdditionHandler;
        }

        public void AddTemporaryPoint(Point p)
        {
            Model.AddTemporaryPoint(p);
        }
        public InqLineModel LastPartialLineModel { get; set; }
        private void PartialLineAdditionHandler(object source, AddPartialLineEventArgs e)
        {
            if (e.AddedLineModel != LastPartialLineModel)
            {
                LastPartialLineModel = e.AddedLineModel;
                RaisePropertyChanged("PartialLineAdded");
            }
        }
        public void RemoveLine(InqLineView lineView)
        {
            
        }
    }
}
