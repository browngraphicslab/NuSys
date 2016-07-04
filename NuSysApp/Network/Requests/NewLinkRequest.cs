using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using NuSysApp.Components.Nodes;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Controller;
using NuSysApp.Nodes.AudioNode;
using NuSysApp.Viewers;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class NewLinkRequest : Request
    {
        public NewLinkRequest(Message m) : base(RequestType.NewLinkRequest,m){}
        public NewLinkRequest(LinkId id1, LinkId id2, string creator, string contentId, Dictionary<string,MetadataEntry> metadata ,UserControl regionView, RectangleView rectangle, Dictionary<string, object> inFineGrainDictionary, Dictionary<string, object> outFineGrainDictionary, string id = null, bool IsPresentationLink = false) : base(RequestType.NewLinkRequest)
        {
            _message["id1"] = JsonConvert.SerializeObject(id1);
            _message["id2"] = JsonConvert.SerializeObject(id2);
            _message["id"] = id ?? SessionController.Instance.GenerateId();
            _message["creator"] = creator;
            _message["contentId"] = contentId;
            _message["isPresentationLink"] = IsPresentationLink;

            if (metadata != null)
            {
                _message["metadata"] = metadata;
            }
            if (inFineGrainDictionary != null)
            {
                _message["inFGDictionary"] = inFineGrainDictionary;
            }
            if (outFineGrainDictionary != null)
            {
                _message["outFGDictionary"] = outFineGrainDictionary;
            }
            if (regionView != null)
            {
                _message["inFineGrain"] = (regionView.DataContext as RegionViewModel).Model;
                //_message["inFineGrain"] = regionView;
            }
            if (rectangle != null)
            {
                _message["rectangleMod"] = (rectangle.DataContext as RectangleViewModel);
            }
            SetServerSettings();
        }

        private void SetServerSettings()
        {
            SetServerEchoType(ServerEchoType.None);
            SetServerItemType(ServerItemType.Content);
            SetServerRequestType(ServerRequestType.Add);
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
            _message["type"] = ElementType.Link.ToString();
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
            ElementType type = (ElementType) Enum.Parse(typeof (ElementType), (string) _message["type"], true);


            var libraryElement = LibraryElementModelFactory.CreateFromMessage(_message);

            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElement.LibraryElementId);
            libraryElement.Timestamp = time;
            var loadEventArgs = new LoadContentEventArgs(_message["data"]?.ToString());
            if (_message.ContainsKey("data") && _message["data"] != null)
            {
                controller.Load(loadEventArgs);
            }
            libraryElement.ServerUrl = url;
            SessionController.Instance.LinkController.AddLink(_message.GetString("id"));

            AddLinks(JsonConvert.DeserializeObject<LinkId>((string) _message["id1"]),
                JsonConvert.DeserializeObject<LinkId>((string) _message["id2"]),
                _message.GetString("id"));
        }

        public override async Task ExecuteRequestFunction()
        {
            var id1 = JsonConvert.DeserializeObject<LinkId>((string)_message["id1"]);
            var id2 = JsonConvert.DeserializeObject<LinkId>((string)_message["id2"]);
            var id = _message.GetString("id");
            var creator = _message.GetString("creator");
            //var contentId = _message.GetString("contentId");

            var link = LibraryElementModelFactory.CreateFromMessage(_message);

            var parentCollectionLibraryElement = (CollectionLibraryElementModel)SessionController.Instance.ContentController.GetContent(creator);
            parentCollectionLibraryElement.AddChild(id);
            
            AddLinks(id1,id2,id);

        }

        private void AddLinks(LinkId id1, LinkId id2, string contentId)
        {
            var controller1 = SessionController.Instance.ContentController.GetLibraryElementController(id1.LibraryElementId);
            var controller2 = SessionController.Instance.ContentController.GetLibraryElementController(id2.LibraryElementId);
            var linkController = SessionController.Instance.ContentController.GetLibraryElementController(contentId);
            Debug.Assert(controller1 != null && controller2 != null && linkController != null && linkController is LinkLibraryElementController);
            controller1.AddLink(linkController as LinkLibraryElementController);
            controller2.AddLink(linkController as LinkLibraryElementController);
        }
    }
}
