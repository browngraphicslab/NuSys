using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CreateNewContentRequest : Request
    {
        public CreateNewContentRequest(Message m) : base(RequestType.CreateNewContentRequest, m)
        {
            SetServerSettings();
        }

        public CreateNewContentRequest(string id, string data, string type = null, string title = null)
            : base(RequestType.CreateNewContentRequest)
        {
            _message["id"] = id;
            _message["data"] = data;
            if (type != null)
            {
                _message["type"] = type;
            }
            if (title != null)
            {
                _message["title"] = title;
            }
            SetServerSettings();
        }

        private void SetServerSettings()
        {
            SetServerEchoType(ServerEchoType.None);
            SetServerItemType(ServerItemType.Content);
            SetServerRequestType(ServerRequestType.Add);
        }
    }
}
