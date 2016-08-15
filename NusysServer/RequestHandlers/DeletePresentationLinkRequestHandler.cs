using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;

namespace NusysServer.RequestHandlers
{
    public class DeletePresentationLinkRequestHandler:RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.DeletePresentationLinkRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));

            //create new message to pass into the delete presentation link query
            var deletePresentationLinkMessage = new Message();
            deletePresentationLinkMessage[NusysConstants.PRESENTATION_LINKS_TABLE_LINK_ID_KEY] = message[NusysConstants.DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY];

            SQLDeleteQuery deletePresentationLinkQuery = new SQLDeleteQuery(Constants.SQLTableType.PresentationLink, deletePresentationLinkMessage, Constants.Operator.And);
            //delete the presnetation link
            var success = deletePresentationLinkQuery.ExecuteCommand();

            ForwardMessage(message, senderHandler);

            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage;
        }
    }
}