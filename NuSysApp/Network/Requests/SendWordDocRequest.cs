using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class SendWordDocRequest : Request
    {
        public SendWordDocRequest(string filepath) : base(RequestType.SendWordDocRequest)
        {
            _message["filepath"] = filepath;
            _message["data"] = System.IO.File.ReadAllBytes(filepath);
        }

        public SendWordDocRequest(Message m) : base(RequestType.SendWordDocRequest, m){}
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("filepath") || !_message.ContainsKey("data"))
            {
                throw new Exception("Send Word Doc Requests must have at least a filepath and data property");
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            var bytes = _message.GetByteArray("data");
            var filepath = _message.GetString("filepath");
            File.WriteAllBytes(filepath, bytes);
        }
    }
}
