using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;
using NusysIntermediate;

namespace NuSysApp
{
    public class AddInkRequest : Request
    {

        public AddInkRequest(Message message) : base(NusysConstants.RequestType.AddInkRequest, message){}

        public async override Task CheckOutgoingRequest()
        {
            if (_message.GetString("id", null) == null)
            {
                throw new Exception("AddInkRequest must contain 'id'");
            }

            if (_message.GetString("data", null) == null)
            {
                throw new Exception("AddInkRequest must contain 'data'");
            }

            _message["contentId"] = _message.GetString("id", null);
        }
        public override async Task ExecuteRequestFunction()
        {
            var props = _message;
            var id = _message.Get("id");
            var data = JsonConvert.DeserializeObject<Dictionary<string,string>>(props.GetString("data"));
            var inkpoints = JsonConvert.DeserializeObject<List<InkPoint>>(data["inkpoints"]);
            var type = data["type"];
            var rgb = JsonConvert.DeserializeObject<byte[]>(data["color"]);
            var color = Color.FromArgb(255, rgb[0], rgb[1], rgb[2]);

            if (InkStorage._inkStrokes.ContainsKey(id) || SessionController.Instance.ActiveFreeFormViewer == null || SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel == null)
            {
                return;
            }
            var collectionController = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController as CollectionLibraryElementController;

            if (collectionController != null && collectionController.InkLines.Contains(id))
            {

                if (type == "adornment")
                {

                    if (!InkStorage._inkStrokes.ContainsKey("id") && SessionController.Instance.SessionView != null)
                    {
                        var stroke = SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.AddAdorment(inkpoints, color, false);
                        InkStorage._inkStrokes.Add(id, new InkWrapper(stroke, "adornment", color));
                    }
                }
                else
                {

                    if (!InkStorage._inkStrokes.ContainsKey("id") && SessionController.Instance.SessionView != null)
                    {
                        var stroke = SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.AddStroke(inkpoints);
                        InkStorage._inkStrokes.Add(id, new InkWrapper(stroke, "ink", color));
                    }
                }
            }

        }
    }
}
