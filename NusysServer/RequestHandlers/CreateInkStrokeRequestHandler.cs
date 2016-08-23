using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class CreateInkStrokeRequestHandler: RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateInkStrokeRequest);
            var message = GetRequestMessage(request);

            //TODO not make these debug.asserts
            //Check to make sure message has everything we need
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_INK_STROKE_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_INK_STROKE_REQUEST_POINTS_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_INK_STROKE_REQUEST_STROKE_ID_KEY));

            var messageToPassIntoQuery = new Message();
            messageToPassIntoQuery[NusysConstants.INK_TABLE_STROKE_ID] =
                message.GetString(NusysConstants.CREATE_INK_STROKE_REQUEST_STROKE_ID_KEY);
            messageToPassIntoQuery[NusysConstants.INK_TABLE_CONTENT_ID] =
                message.GetString(NusysConstants.CREATE_INK_STROKE_REQUEST_CONTENT_ID_KEY);
            messageToPassIntoQuery[NusysConstants.INK_TABLE_POINTS] =
                message.GetString(NusysConstants.CREATE_INK_STROKE_REQUEST_POINTS_KEY);
            SQLInsertQuery insertInkQuery = new SQLInsertQuery(Constants.SQLTableType.Ink, messageToPassIntoQuery);

            var success = insertInkQuery.ExecuteCommand();

            //if inserting into the database did not work, return a new message that says we failed.
            if (!success)
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false } });
            }

            //Create the ink model and forward it to everyone except the original sender
            var inkModel = new InkModel();
            inkModel.UnPackFromDatabaseMessage(message);
            var modelJson = JsonConvert.SerializeObject(inkModel);
            var forwardMessage = new Message(message);
            forwardMessage[NusysConstants.CREATE_INK_STROKE_REQUEST_RETURNED_INK_MODEL_KEY] = modelJson;
            ForwardMessage(forwardMessage, senderHandler);

            //Create message to return to original sender
            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage;
        }
    }
}