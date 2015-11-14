﻿using System;
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

        public async Task<string> InkToText(List<InqLineModel> inqLineModels )
        {
            if (inqLineModels.Count == 0)
                return string.Empty;

            var im = new InkManager();

            var b = new InkStrokeBuilder();

            foreach (var inqLineModel in inqLineModels)
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
        private async void FinalLineAdditionHandler(InqLineModel lineModel)
        {
    
            if (lineModel != FinalLineModel)
            {
            
                FinalLineModel = lineModel;
                RaisePropertyChanged("FinalLineAdded");
               
            }

            await CheckForGroupCreation(lineModel);
        }

        private async Task<bool> CheckForGroupCreation(InqLineModel line)
        {
            
            var outerRect = Geometry.PointCollecionToBoundingRect(line.Points);

            var idsToDelete = new List<string>();
            var encompassedLines = new List<InqLineModel>();
            foreach (var otherLine in Model.Lines.Where(l => l != line))
            {
                var innerRect = Geometry.PointCollecionToBoundingRect(otherLine.Points);
                var innerRect2 = new Rect(innerRect.X,innerRect.Y, innerRect.Width, innerRect.Height);
                innerRect.Intersect(outerRect);
                if (Math.Abs(innerRect2.Width-innerRect.Width) < 50 && Math.Abs(innerRect2.Height - innerRect.Height) < 50)
                {

                    idsToDelete.Add(otherLine.ID);
                    InqLineModel newModel = new InqLineModel(DateTime.UtcNow.Ticks.ToString())
                    {
                        Stroke = otherLine.Stroke,
                        StrokeThickness = otherLine.StrokeThickness
                    };

                    foreach (var point in otherLine.Points)
                    {
                        newModel.AddPoint(new Point(point.X - outerRect.X, point.Y - outerRect.Y));
                    }
                    encompassedLines.Add(newModel);
                }
            }


            var first = line.Points.First();
            var last = line.Points.Last();
            if (encompassedLines.Count == 0 || (Math.Abs(first.X -last.X) > 25 && Math.Abs(first.Y - last.Y) > 25) )
            {
                return false;
            }

            
            foreach (var idToDelete in idsToDelete)
            {
                NetworkConnector.Instance.RequestDeleteSendable(idToDelete);
            }
            
            var title = await InkToText(encompassedLines);
            var dict = new Dictionary<string, string>();
            dict["width"] = outerRect.Width.ToString();
            dict["height"] = outerRect.Height.ToString();
            dict["title"] = title;
            Action<string> addCallback = delegate (string s)
            {
                NetworkConnector.Instance.RequestDeleteSendable(line.ID);
                var v = SessionController.Instance.IdToSendables[s] as TextNodeModel;
                if (v != null)
                {
                    Debug.Assert(encompassedLines.Count > 0);
                    foreach (var model in encompassedLines)
                    {
                        UITask.Run(async () =>
                        {
                            //NetworkConnector.Instance.RequestLock(v.ID);
                            NetworkConnector.Instance.RequestFinalizeGlobalInk(model.ID, v.InqCanvas.ID, model.GetString());
                            //is the model being deleted and then trying to be added? is the canvas fully there when we try to add?
                        });
                    }
                }
            };
            await NetworkConnector.Instance.RequestNewGroupTag(outerRect.X.ToString(), outerRect.Y.ToString(), title, dict, addCallback);
            return true;
        }

        public void RemoveLine(InqLineView lineView)
        {
            
        }
    }
}
