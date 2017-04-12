using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// reuqest handler class for the GetCollaboratorCoordinatesRequest
    /// </summary>
    public class GetCollaboratorCoordinatesRequestHandler : FullArgsRequestHandler<GetCollaboratorCoordinatesRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// this handle reuqest override must forward on the request to the SINGLE other user requested.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override ServerReturnArgsBase HandleArgsRequest(GetCollaboratorCoordinatesRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var returnArgs = new ServerReturnArgsBase();
            var user = NusysClient.IDtoUsers.Keys.FirstOrDefault(u => u.UserId == args.UserId);
            if (user == null)
            {
                throw new Exception("Intended recipient wasn't found or wasn't logged on");
            }
            args.OriginalSenderId = senderHandler.UserId;
            returnArgs.WasSuccessful = true;
            ForwardToUser(args,returnArgs,user);
            return returnArgs;
        }
    }
}