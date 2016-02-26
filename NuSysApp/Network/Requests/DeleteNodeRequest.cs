using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class DeleteSendableRequest : Request
    {
        public string Id;
        public DeleteSendableRequest(string id) : base(RequestType.DeleteSendableRequest)//maybe make an abstract delete sendable class and have this extend that
        {
            Id = id;
            _message["id"] = id;
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
        }
        public override async Task ExecuteRequestFunction()
        {
            if (!SessionController.Instance.IdToControllers.ContainsKey(Id))
                return;

            var atomModel = SessionController.Instance.IdToControllers[Id].Model;
            atomModel.Delete();
            SessionController.Instance.IdToControllers.Remove(Id);
        }
    }
    public class DeleteSendableRequestException : Exception
    {
        public DeleteSendableRequestException(string message) : base(message) { }
        public DeleteSendableRequestException() : base("There was an error in the Delete Node Request") { }
    }
}
