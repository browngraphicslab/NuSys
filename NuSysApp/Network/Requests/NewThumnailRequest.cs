using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewThumbnailRequest : Request
    {
        public NewThumbnailRequest(Message m) : base(RequestType.NewThumbnailRequest, m) { }
        public NewThumbnailRequest(string data,string id) : base(RequestType.NewThumbnailRequest)
        {
            _message["data"] = data;
            _message["id"] = id;
        }
        public override async Task CheckOutgoingRequest()
        {
            if(!_message.ContainsKey("id") || !_message.ContainsKey("data"))
            {
                throw new Exception("NewThumbnailRequests require at least 'data' and 'id'");
            }
        }
        public async override Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            var data = _message.GetString("data");
            if (id != null && data != null)
            {
                await SessionController.Instance.SaveThumb(id, Convert.FromBase64String(data));
            }
        }
    }
}
