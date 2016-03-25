using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class DeleteLibraryElementRequest : Request
    {
        public DeleteLibraryElementRequest(string id) : base(RequestType.DeleteLibraryElementRequest)
        {
            _message["id"] = id;
            SetServerSettings();
        }
        public DeleteLibraryElementRequest(Message m) : base(RequestType.DeleteLibraryElementRequest,m)
        {
            SetServerSettings();
        }

        private void SetServerSettings()
        {
            SetServerEchoType(ServerEchoType.ForcedEveryone);
            SetServerIgnore(false);
            SetServerItemType(ServerItemType.Content);
            SetServerRequestType(ServerRequestType.Remove);
        }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                throw new Exception("Library Element Delete requests must contains a library 'id' to delete");
            }
        }
        public override async Task ExecuteRequestFunction()
        {
            var libraryElement = SessionController.Instance.ContentController.Get(_message.GetString("id"));
            libraryElement.FireDelete();
        }
    }
}
