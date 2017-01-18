using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the reuqest handler class for sending coordinates to another user.
    /// </summary>
    public class SendCollaboratorCoordinatesRequestHandler : FullArgsRequestHandler<SendCollaboratorCoordinatesRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// this handle request override should simply forward the request onto the intended recipient.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override ServerReturnArgsBase HandleArgsRequest(SendCollaboratorCoordinatesRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var user = NusysClient.IDtoUsers.Keys.FirstOrDefault(u => u.UserId == args.RecipientUserId);
            if (user == null)
            {
                throw new Exception("the intended recipient could not be found!");
            }
            args.OriginalSenderId = senderHandler.UserId;
            ForwardToUser(args,new ServerReturnArgsBase() {WasSuccessful = true}, user);
            return new ServerReturnArgsBase() {WasSuccessful = true};
        }
    }
}