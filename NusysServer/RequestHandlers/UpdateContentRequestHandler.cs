using System;
using System.Diagnostics;
using NusysIntermediate;

namespace NusysServer
{
    public class UpdateContentRequestHandler: RequestHandler
    {
        /// <summary>
        /// This will update the content file on the server
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.UpdateContentRequest);
            var message = GetRequestMessage(request);

            Debug.Assert(message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_TYPE_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY));
            if (message.GetEnum<NusysConstants.ContentType>(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_TYPE_KEY) !=
                NusysConstants.ContentType.Text)
            {
                throw new Exception("You can only update content of type text");
            }
            var success = true;
            if (message.GetBool(NusysConstants.UPDATE_CONTENT_REQUEST_SAVE_TO_SERVER_BOOLEAN, true))
            {
                 success =
                    FileHelper.UpdateContentDataFile(
                        message.GetString(NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY),
                        message.GetEnum<NusysConstants.ContentType>(
                            NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_TYPE_KEY),
                        message.GetString(NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY));
            }
            if (success)
            {
                ForwardMessage(message, senderHandler);
            }
            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage; 
        }
    }
}