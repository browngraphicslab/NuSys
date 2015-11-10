using System;
using System.Collections.Generic;
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

        public InqCanvasView View { get; }
        public InqCanvasViewModel(InqCanvasView inqCanvasView, InqCanvasModel model)
        {
            Model = model;
            this.Model.OnPartialLineAddition += PartialLineAdditionHandler;
            this.Model.OnFinalizedLine += FinalLineAdditionHandler;
            inqCanvasView.ViewModel = this;
            View = inqCanvasView;

            foreach (var inqLineModel in model.Lines)
            {
                var lineView = new InqLineView(new InqLineViewModel(inqLineModel));;
                inqCanvasView.Children.Add(lineView);
            }
            
        }

        public async Task<string> InkToText()
        {
            if (Model.Lines.Count == 0)
                return string.Empty;

            var im = new InkManager();

            var b = new InkStrokeBuilder();

            foreach (var inqLineModel in Model.Lines)
            {
                var stroke = b.CreateStroke(inqLineModel.Points);
                im.AddStroke(stroke);
            }

            var result = await im.RecognizeAsync(InkRecognitionTarget.All);
            return result[0].GetTextCandidates()[0];
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
