﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AddInkRequest : Request
    {

        public AddInkRequest(Message message) : base(Request.RequestType.AddInkRequest, message){}

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

            SetServerEchoType(ServerEchoType.ForcedEveryone);
            SetServerItemType(ServerItemType.Ink);
            SetServerRequestType(ServerRequestType.Add);
            SetServerIgnore(false);
        }
        public override async Task ExecuteRequestFunction()
        {
            var props = _message;
            var id = _message.Get("id");
            var data = JsonConvert.DeserializeObject<Dictionary<string,string>>(props.GetString("data"));
            var inkpoints = JsonConvert.DeserializeObject<List<InkPoint>>(data["inkpoints"]);
            var type = JsonConvert.DeserializeObject<string>(data["type"]);

            if (type == "adornment")
            {
                if (!InkStorage._inkStrokes.ContainsKey("id"))
                    SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.AddAdorment(inkpoints, false);
            }
            else
            {
                if (!InkStorage._inkStrokes.ContainsKey("id"))
                    SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.AddStroke(inkpoints);
            }

        }
    }
}
