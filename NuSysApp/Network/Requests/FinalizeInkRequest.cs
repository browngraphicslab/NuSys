using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using NusysIntermediate;

namespace NuSysApp
{
    public class FinalizeInkRequest : Request
    {
        public FinalizeInkRequest(Message message) : base(NusysConstants.RequestType.FinalizeInkRequest, message){}

        public async override Task CheckOutgoingRequest()
        {
            if (_message.GetString("id", null) == null)
            {
                throw new Exception("FinalizeInkRequest must contain 'id'");
            }
            _message["contentId"] = _message.GetString("id", null);
        }
        public override async Task ExecuteRequestFunction()
        {
            var props = _message;
            var id = _message.Get("id");

            var has = SessionController.Instance.IdToControllers.ContainsKey(props.GetString("canvasNodeID"));
            if (!has)
                return;
            if (props.ContainsKey("inkType") && props["inkType"] == "partial")
            {
                var one = new Point2d(Double.Parse(props.GetString("x1")), Double.Parse(props.GetString("y1")));
                var two = new Point2d(Double.Parse(props.GetString("x2")), Double.Parse(props.GetString("y2")));
        
                var lineModel = new InqLineModel(props.GetString("canvasNodeID"));
                                    //  var line = new InqLineView(new InqLineViewModel(lineModel), 2, new SolidColorBrush(Colors.Black));
                var pc = new ObservableCollection<Point2d>();
                pc.Add(one);
                pc.Add(two);
                lineModel.Points = pc;
                lineModel.Stroke = new SolidColorBrush(Colors.Black);
                if (props.ContainsKey("stroke") && props["stroke"] != "black")
                {
                    lineModel.Stroke = new SolidColorBrush(Colors.Yellow);
                }
                    
            }
            else if (props.GetString("inkType") == "full" )
            {
                var lineModel = new InqLineModel(id);
                await lineModel.UnPackFromDatabaseMessage(props);
            }
        }
    }
}
