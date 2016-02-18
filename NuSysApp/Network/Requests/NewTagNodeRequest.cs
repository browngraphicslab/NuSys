using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewTagNodeRequest : Request
    {
        public NewTagNodeRequest(Message m) : base(RequestType.NewContentRequest, m) { }

        public NewTagNodeRequest(string id, string data) : base(RequestType.NewContentRequest)
        {
            _message["id"] = id;
            _message["data"] = data;
        }
    
        public override async Task ExecuteRequestFunction()
        {
            /*
          //  var data = _message.GetString("data");
            var id = _message.GetString("id");
            var tag = _message.GetString("tag");

            var tagNode = new NodeContainerModel(id)
            {
                X = _message.GetDouble("x", 0),
                Y = _message.GetDouble("x", 0),,
                Width = _message.GetDouble("width", 0),,
                Height = height,
                NodeType = NodeType.GroupTag.ToString(),
                Title = tag
            };

            IdToSendables.Add(id, group);

            NodeModel node = await SessionController.Instance.CreateTagNode(_message.GetString("id"), (NodeType)Enum.Parse(typeof(NodeType), _message.GetString("nodeType")));
            SessionController.Instance.IdToSendables[_message.GetString("id")] = node;
            await node.UnPack(_message);

            if (!_message.GetBool("autoCreate"))
                return;

            var creators = node.Creator;
            var addedModels = new List<AtomModel>();
            SessionController.Instance.RecursiveCreate(node, addedModels);
            */

        }
    }
}
