using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class NewLinkRequest : Request
    {
        public NewLinkRequest(Message m) : base(NusysConstants.RequestType.NewLinkRequest,m){}
        public NewLinkRequest(string id1, string id2, string creator, string contentId, string id = null) : base(NusysConstants.RequestType.NewLinkRequest)
        {
            _message["id1"] = id1;
            _message["id2"] = id2;
            _message["id"] = id ?? SessionController.Instance.GenerateId();
            _message["creator"] = creator;
            _message["contentId"] = contentId;
        }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                _message["id"] = SessionController.Instance.GenerateId();
            }
            if (!_message.ContainsKey("contentId"))
            {
                throw new Exception("new link request requires a contentId");
            }
            _message["type"] = NusysConstants.ElementType.Link.ToString();
            /*
            SetServerEchoType(ServerEchoType.Everyone);
            SetServerItemType(ServerItemType.Alias);
            SetServerRequestType(ServerRequestType.Add);
            */


            var time = DateTime.UtcNow.ToString();
            _message["library_element_creation_timestamp"] = time;
            string url = null;
            if (_message.ContainsKey("server_url"))
            {
                url = _message["server_url"].ToString();
            }
            Random rand = new Random();
            Color c = Constants.linkColors[rand.Next(0, Constants.linkColors.Count)];
            _message["color"] = c.ToString();
            NusysConstants.ElementType type = (NusysConstants.ElementType)Enum.Parse(typeof(NusysConstants.ElementType), (string)_message["type"], true);

            var libraryElement = SessionController.Instance.ContentController.CreateAndAddModelFromMessage(_message);

            var controller =
                SessionController.Instance.ContentController.GetLibraryElementController(
                    libraryElement.LibraryElementId);
            //var linkLibElemCont = controller as LinkLibraryElementController;
            //Debug.WriteLine(linkLibElemCont != null);

            libraryElement.Timestamp = time;

            libraryElement.ServerUrl = url;
            //SessionController.Instance.LinksController.AddLink(_message.GetString("id"));
            var id1 = (string)_message["id1"];
            var id2 = (string)_message["id2"];
            var id = _message.GetString("id");
            AddLinks(id1, id2, id);
        }

        public override async Task ExecuteRequestFunction()
        {
            var id1 = (string)_message["id1"];
            var id2 = (string)_message["id2"];
            var id = _message.GetString("id");
            var creator = _message.GetString("creator");
            //var contentId = _message.GetString("contentId");

            //var link = LibraryElementModelFactory.CreateFromMessage(_message);

            var parentCollectionLibraryElement = (CollectionLibraryElementController)SessionController.Instance.ContentController.GetLibraryElementController(creator);
            parentCollectionLibraryElement.AddChild(id);

            AddLinks(id1, id2, id);

        }

        private void AddLinks(string id1, string id2, string contentId)
        {
            var controller1 = SessionController.Instance.ContentController.GetLibraryElementController(id1);
            var controller2 = SessionController.Instance.ContentController.GetLibraryElementController(id2);
            var linkController = SessionController.Instance.ContentController.GetLibraryElementController(contentId);
            //Debug.Assert(controller1 != null && controller2 != null && linkController != null && linkController is LinkLibraryElementController);
            controller1?.AddLink(linkController as LinkLibraryElementController);
            controller2?.AddLink(linkController as LinkLibraryElementController);


            if (controller1 != null && controller2 != null)
            {
                SessionController.Instance.LinksController.CreateVisualLinks(
                linkController as LinkLibraryElementController);
            }

        }

    }
}

