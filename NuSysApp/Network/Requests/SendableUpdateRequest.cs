using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class SendableUpdateRequest : Request
    {
        public SendableUpdateRequest(Message m, bool saveToServer = false) : base(NusysConstants.RequestType.SendableUpdateRequest, m)
        {
            //TODO ADD BACK IN SAVE TO SERVER FUNCTIONALITY.  
            //MAYBE ADD A NEW REQUEST TO MATCH OLD FUNCTIONALITY
            //VERY IMPORTANT ASK TRENT
        }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                throw new Exception("The Sendable update must have a key labeled 'id'");
            }
            _message["sender_user_id"] = SessionController.Instance.LocalUserID;
            var time = DateTime.UtcNow.ToString();
            _message["library_element_last_edited_timestamp"] = time;
        }

        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            if (SessionController.Instance.IdToControllers.ContainsKey(id))
            {
                var Controller = SessionController.Instance.IdToControllers[id];
                
                await Controller.UnPack(_message);
                if(_message.ContainsKey("sender_user_id") && SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey((string)_message["sender_user_id"]))
                {
                    var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[(string)_message["sender_user_id"]];
                    user?.SetUserController(Controller.LibraryElementController);
                }
            }
        }
    }
}
