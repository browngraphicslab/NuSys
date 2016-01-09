using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewContentRequest : Request
    {
        public NewContentRequest(Message m) : base(RequestType.NewContentRequest, m) { }

        public NewContentRequest(string id, string data) : base(RequestType.NewContentRequest)
        {
            _message["id"] = id;
            _message["data"] = data;
        }
        public async override Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id") || !_message.ContainsKey("data"))
            {
                throw new Exception("New Content requests require at least 'id' and 'data'");
            }
        }
        public override async Task ExecuteRequestFunction()
        {
            var data = _message.GetString("data");
            var id = _message.GetString("id");
            SessionController.Instance.ContentController.Add(data, id);
        }
    }
}
