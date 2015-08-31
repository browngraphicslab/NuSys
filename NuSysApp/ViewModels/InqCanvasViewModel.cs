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
        public InqCanvasViewModel(InqCanvasView inqCanvasView, InqCanvasModel model)
        {
            Model = model;
            this.Model.OnPartialLineAddition += PartialLineAdditionHandler;
            inqCanvasView.ViewModel = this;
            View = inqCanvasView;
        }

        public void AddTemporaryPoint(Point p)
        {
            Model.AddTemporaryPoint(p);
        }
        public InqLine LastPartialLine { get; set; }
        private void PartialLineAdditionHandler(object source, AddPartialLineEventArgs e)
        {
            if (e.AddedLine != LastPartialLine)
            {
                LastPartialLine = e.AddedLine;
                RaisePropertyChanged("PartialLineAdded");
            }
        }
        public void RemoveLine(InqLine line)
        {
            
        }
    }
}
