using System.Collections.Generic;
using System.Diagnostics;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;

namespace NusysServer.RequestHandlers
{
    public class DeleteInkStrokeRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.DeleteInkStrokeRequest);
            var message = GetRequestMessage(request);

            //TODO not make these debug.asserts
            //Check to make sure message has everything we need
            Debug.Assert(message.ContainsKey(NusysConstants.DELETE_INK_STROKE_REQUEST_STROKE_ID_KEY));

            //Create delete query
            var deleteMessageToPassIntoQuery = new Message();
            deleteMessageToPassIntoQuery[NusysConstants.INK_TABLE_STROKE_ID] =
                message.GetString(NusysConstants.DELETE_INK_STROKE_REQUEST_STROKE_ID_KEY);
            var deleteInkQuery = new SQLDeleteQuery(Constants.SQLTableType.Ink, deleteMessageToPassIntoQuery, Constants.Operator.And);

            var success = deleteInkQuery.ExecuteCommand();

            //if inserting into the database did not work, return a new message that says we failed.
            if (!success)
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false } });
            }

            ForwardMessage(message, senderHandler);

            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage;
        }
    }
}