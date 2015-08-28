using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class InqCanvasViewModel : BaseINPC
    {
        public InqCanvasModel Model { get; }

        public InqCanvasViewModel(InqCanvasModel model)
        {
            Model = model;
            this.Model.OnPartialLineAddition += PartialLineAdditionHandler;
        }

        public void AddTemporaryPoint(Point p)
        {
            Model.AddTemporaryPoint(p);
        }
        public InqLine LastPartialLine { get; set; }
        private void PartialLineAdditionHandler(object source, AddPartialLineEventArgs e)
        {
            LastPartialLine = e.AddedLine;
            RaisePropertyChanged("PartialLineAdded");
        }
        public void RemoveLine(InqLine line)
        {
            
        }
    }
}
