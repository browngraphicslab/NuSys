﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class ChangeContentRequest : Request
    {
        public ChangeContentRequest(Message m) : base(RequestType.ChangeContentRequest, m)
        {
            SetServerSettings();
        }

        public ChangeContentRequest(string contentID, string contentData) : base(RequestType.ChangeContentRequest)
        {
            _message["contentId"] = contentID;
            _message["data"] = contentData;
            SetServerSettings();
        }

        private void SetServerSettings()
        {
            SetServerEchoType(ServerEchoType.EveryoneButSender);//TODO maybe have this be everyone again
            SetServerItemType(ServerItemType.Content);
            SetServerRequestType(ServerRequestType.Update);
        }

        public override async Task CheckOutgoingRequest()
        {
            if (_message.ContainsKey("inklines"))
            {
                var lines = _message.GetObject("inklines") as HashSet<string>;
                var s = _message.GetObject("inklines");
                var model =
                    SessionController.Instance.ContentController.Get(_message["contentId"].ToString()) as
                        CollectionLibraryElementModel;
                if (Math.Abs(model.LastInkCount - lines.Count) > 1)
                {
                    
                }
                model.LastInkCount = lines.Count;
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            LibraryElementModel content = SessionController.Instance.ContentController.Get(_message.GetString("contentId"));
            if (_message.ContainsKey("title"))
            {
                content.Title = _message.GetString("title");
            }
            if (_message.ContainsKey("data"))
            {
                content.Data = _message.GetString("data");
            }
            if (_message.ContainsKey("inklines"))
            {

                var inkIds = _message.GetList<string>("inklines");
                var libModel = (CollectionLibraryElementModel)content;
                var oldInkLines = libModel.InkLines;
                var added = inkIds.Except(oldInkLines);
                var removed = oldInkLines.Except(inkIds);

                foreach (var idremoved in removed)
                {
                    libModel.RemoveInk(idremoved);
                }

                foreach (var idadded in added)
                {
                    libModel.AddInk(idadded);
                }

            }
        }
    }
}
