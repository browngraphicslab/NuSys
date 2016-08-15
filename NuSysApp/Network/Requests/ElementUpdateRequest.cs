using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class ElementUpdateRequest : Request
    {
        /// <summary>
        /// Preferred constructor. 
        /// request takes in a message whose keys should be the database keys. 
        /// It also takes in a boolean to tell the server whether to save the changes or just forward to everyone else
        /// </summary>
        /// <param name="m"></param>
        /// <param name="saveToServer"></param>
        public ElementUpdateRequest(Message m, bool saveToServer = false) : base(NusysConstants.RequestType.ElementUpdateRequest, m)
        {
            _message[NusysConstants.ELEMENT_UPDATE_REQUEST_SAVE_TO_SERVER_BOOLEAN] = saveToServer;
        }

        /// <summary>
        /// this checkout outgoing request just checks to make sure the element being updated has a key. 
        /// It also updates the last-editted timestamp value
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey(NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY))
            {
                throw new Exception("The Sendable update must have a key labeled 'id'");
            }
            _message["sender_user_id"] = SessionController.Instance.LocalUserID;//TODO refactor
            var time = DateTime.UtcNow.ToString();
            _message[NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY] = time;
        }

        /// <summary>
        /// this will be called when another client updates an element model.  
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString(NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY);
            if (SessionController.Instance.IdToControllers.ContainsKey(id))
            {
                var controller = SessionController.Instance.IdToControllers[id];
                
                await controller.UnPack(_message);
                if(_message.ContainsKey("sender_user_id") && SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey((string)_message["sender_user_id"]))
                {
                    var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[(string)_message["sender_user_id"]];
                    user?.SetUserController(controller.LibraryElementController);
                }
            }
        }
    }
}
