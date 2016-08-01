using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class GetEntireWorkspaceRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetEntireWorkspaceRequest);

            var message = GetRequestMessage(request);

            Debug.Assert(message.ContainsKey(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY));

            //todo actually get the info

            //create new args to return
            var returnArgs = new GetEntireWorkspaceRequestArgs();
            returnArgs.AliasMessages = new HashSet<Message>() {new Message( new Dictionary<string,object>() { { "test", 1}})};

            returnArgs.AliasMessages = null; //TODO fill this in
            returnArgs.ContentMessages = null; //TODO fill this in
            returnArgs.LibraryElementMessages = null; //TODO fill this in

            var returnMessage = new Message();
            returnMessage[NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY] = returnArgs;
            return returnMessage;
        }
    }
}