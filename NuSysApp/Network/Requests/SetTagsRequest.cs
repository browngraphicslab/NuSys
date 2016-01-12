using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class SetTagsRequest:Request
    {
        public SetTagsRequest(Message m) : base(RequestType.SetTagsRequest, m) { }

        public SetTagsRequest(string id, List<string> tags) : base(RequestType.SetTagsRequest)
        {
            _message["id"] = id;
            _message["tags"] = tags;
        }

        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                throw new Exception("Set Tags Request must have an 'id' property");
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            List<string> tags = _message.GetList<string>("tags");
            ((NodeModel)SessionController.Instance.IdToSendables[_message.GetString("id")]).SetMetaData("tags", tags);
        }
    }
}
