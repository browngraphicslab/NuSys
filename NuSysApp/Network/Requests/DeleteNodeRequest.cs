using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysConstants;

namespace NuSysApp
{
    public class DeleteSendableRequest : Request
    {
        public string Id;
        public DeleteSendableRequest(string id) : base(ServerConstants.RequestType.DeleteSendableRequest)//maybe make an abstract delete sendable class and have this extend that
        {
            Id = id;
            _message["id"] = id;
            SetServerSettings();
        }

        public DeleteSendableRequest(Message message) : base(message)
        {
            if (message.ContainsKey("id"))
            {
                Id = message.GetString("id");
            }
            else
            {
                throw new DeleteSendableRequestException("No ID was found in the recieved message ");
            }
            SetServerSettings();
        }

        private void SetServerSettings()
        {
            SetServerEchoType(ServerEchoType.Everyone);
            SetServerItemType(ServerItemType.Alias);
            SetServerIgnore(false);
            SetServerRequestType(ServerRequestType.Remove);
        }
        public override async Task ExecuteRequestFunction()
        {
            if (!SessionController.Instance.IdToControllers.ContainsKey(Id))
            {
                return;
            }

            var controller = SessionController.Instance.IdToControllers[Id];
            if (controller.Model != null) { 
                var parent = SessionController.Instance.ContentController.GetContent(controller.Model.ParentCollectionId) as CollectionLibraryElementModel;
                parent?.Children.Remove(Id);
            }
            controller.Delete(this);
            ElementController removed;
            SessionController.Instance.IdToControllers.TryRemove(Id, out removed);
        }
    }
    public class DeleteSendableRequestException : Exception
    {
        public DeleteSendableRequestException(string message) : base(message) { }
        public DeleteSendableRequestException() : base("There was an error in the Delete Node Request") { }
    }
}
