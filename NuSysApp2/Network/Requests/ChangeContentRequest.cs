using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Newtonsoft.Json;

namespace NuSysApp2
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

        public override async Task<bool> CheckOutgoingRequest()
        {
            var time = DateTime.UtcNow.ToString();
            _message["library_element_last_edited_timestamp"] = time;
            return true;
        }

        public override async Task ExecuteRequestFunction()
        {
            LibraryElementModel content = SessionController.Instance.ContentController.GetLibraryElementModel(_message.GetString("contentId"));
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(content.LibraryElementId);
            controller.UnPack(_message);
            if (_message.ContainsKey("favorited"))
            {
                controller.SetFavorited(bool.Parse(_message["favorited"].ToString()));
            }
            if (_message.ContainsKey("inklines"))
            {

                var inkIds = _message.GetList<string>("inklines");
                var libModel = (CollectionLibraryElementModel)content;
                var oldInkLines = libModel.InkLines;
                var added = inkIds.Except(oldInkLines).ToArray();
                var removed = oldInkLines.Except(inkIds).ToArray();

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
