using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuSysApp.Components.Nodes;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Controller;
using NuSysApp.Nodes.AudioNode;
using NuSysApp.Viewers;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class NewPresentationLinkRequest : Request
    {
        public NewPresentationLinkRequest(Message m) : base(RequestType.NewPresentationLinkRequest, m) { }
        public NewPresentationLinkRequest(string id1, string id2, string creator, string contentId, UserControl regionView, RectangleView rectangle, Dictionary<string, object> inFineGrainDictionary, Dictionary<string, object> outFineGrainDictionary, string id = null, bool IsPresentationLink = false) : base(RequestType.NewPresentationLinkRequest)
        {
            _message["id1"] = id1;
            _message["id2"] = id2;
            _message["id"] = id ?? SessionController.Instance.GenerateId();
            _message["creator"] = creator;
            _message["contentId"] = contentId;
            _message["isPresentationLink"] = IsPresentationLink;

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
            SetServerEchoType(ServerEchoType.Everyone);
            SetServerItemType(ServerItemType.Alias);
            SetServerRequestType(ServerRequestType.Add);
        }

        public override async Task ExecuteRequestFunction()
        {
            var id1 = _message.GetString("id1");
            var id2 = _message.GetString("id2");
            var id = _message.GetString("id");
            var creator = _message.GetString("creator");
            //var contentId = _message.GetString("contentId");
            if (SessionController.Instance.IdToControllers.ContainsKey(id1) &&
                SessionController.Instance.IdToControllers.ContainsKey(id2))
            {
                var link = new LinkModel(id);
                await link.UnPack(_message);
                var linkController = new LinkElementController(link);
                SessionController.Instance.IdToControllers[id] = linkController;

                var parentCollectionLibraryElement = (CollectionLibraryElementModel)SessionController.Instance.ContentController.GetContent(creator);
                parentCollectionLibraryElement.AddChild(id);
            }
        }
    }
}
