using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class NewLinkRequest : Request
    {
        public enum LinkType
        {
            Node, Image, Audio, Video, Text
        }
        public NewLinkRequest(Message m) : base(RequestType.NewLinkRequest,m){}
        public NewLinkRequest(string id1, string id2, string creator, string contentId, Dictionary<string, object> inFineGrainDictionary = null, Dictionary<string, object> outFineGrainDictionary = null,   string id = null) : base(RequestType.NewLinkRequest)
        {
            _message["id1"] = id1;
            _message["id2"] = id2;
            _message["id"] = id ?? SessionController.Instance.GenerateId();
            _message["creator"] = creator;
            _message["contentId"] = contentId;
            //new
            _message["inType"] = SessionController.Instance.IdToControllers[id1];
            _message["outType"] = SessionController.Instance.IdToControllers[id2];
            _message["inFGDictionary"] = inFineGrainDictionary;
            _message["outFGDictionary"] = outFineGrainDictionary;


        }

        //public NewLinkRequest(string id1, string id2, string creator, string contentId, string id = null, Dictionary<string,string> dict, type ) : base(RequestType.NewLinkRequest)
        //{
        //    _message["id1"] = id1;
        //    _message["id2"] = id2;
        //    _message["id"] = id ?? SessionController.Instance.GenerateId();
        //    _message["creator"] = creator;
        //    _message["contentId"] = contentId;
        //    _message["type"]= type
        //    _message["fgdict"] = dict;
        //}
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
            if (SessionController.Instance.IdToControllers.ContainsKey(id1) && (SessionController.Instance.IdToControllers.ContainsKey(id2)))
            {
                var link = new LinkModel(id);
                await link.UnPack(_message);
                var linkController = new LinkElementController(link);
                SessionController.Instance.IdToControllers.Add(id, linkController);

                var parentCollectionLibraryElement = (CollectionLibraryElementModel)SessionController.Instance.ContentController.Get(creator);
                parentCollectionLibraryElement.AddChild(id);
            }
        }
    }
}
