using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class UpdateLibraryElementModelRequest : Request
    {
        public UpdateLibraryElementModelRequest(Message m) : base(NusysConstants.RequestType.UpdateLibraryElementModelRequest, m)
        {
        }

        public UpdateLibraryElementModelRequest(string contentID, string contentData) : base(NusysConstants.RequestType.UpdateLibraryElementModelRequest)
        {
            _message["contentId"] = contentID;
            _message["data"] = contentData;
        }

        public override async Task CheckOutgoingRequest()
        {
            var time = DateTime.UtcNow.ToString();
            _message["library_element_last_edited_timestamp"] = time;
        }

        public override async Task ExecuteRequestFunction()
        {
            LibraryElementModel libraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(_message.GetString("contentId"));
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModel.LibraryElementId);
            controller.UnPack(_message);
            if (_message.ContainsKey("favorited"))
            {
                controller.SetFavorited(bool.Parse(_message["favorited"].ToString()));
            }
            if (_message.ContainsKey("inklines"))
            {

                var inkIds = _message.GetList<string>("inklines");
                var collectionController = (CollectionLibraryElementController)controller;
                var oldInkLines = collectionController.InkLines;
                var added = inkIds.Except(oldInkLines).ToArray();
                var removed = oldInkLines.Except(inkIds).ToArray();

                foreach (var idremoved in removed)
                {
                    collectionController.RemoveInk(idremoved);
                }

                foreach (var idadded in added)
                {
                    collectionController.AddInk(idadded);
                }

            }
        }
    }
}
