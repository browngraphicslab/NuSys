﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;
using NusysConstants;

namespace NuSysApp
{
    public class RemoveInkRequest : Request
    {
        public RemoveInkRequest(Message message) : base(ServerConstants.RequestType.RemoveInkRequest, message){}

        public async override Task<bool> CheckOutgoingRequest()
        {
            if (_message.GetString("id", null) == null)
            {
                throw new Exception("AddInkRequest must contain 'id'");
            }
  
            SetServerEchoType(ServerEchoType.ForcedEveryone);
            SetServerItemType(ServerItemType.Ink);
            SetServerRequestType(ServerRequestType.Remove);
            SetServerIgnore(false);
            return true;
        }
        public override async Task ExecuteRequestFunction()
        {
            var props = _message;
            var id = _message.Get("id");
            
            if (InkStorage._inkStrokes.ContainsKey(id))
            {
                var stroke = InkStorage._inkStrokes[id];
                SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.RemoveAdorment(stroke.Stroke, false);
                SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.RemoveStroke(stroke.Stroke);
            }
        }
    }
}
