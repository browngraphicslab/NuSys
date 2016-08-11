using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class CreateNewLibraryElementRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateNewLibraryElementRequest);

            var message = GetRequestMessage(request);
            if (!message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY))
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = NusysConstants.GenerateId();
            }
            //Debug.Assert(message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY));
            /*
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY));
            */

            var libraryId = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY);

            //TODO send notification to everyone
            var addLibraryElementMessage = RequestToSqlKeyMappings.LibraryElementRequestKeysToDatabaseKeys(message);

            //create thumbnails and add the paths to the sql database
            var smallIconPath = FileHelper.CreateThumbnailFile(libraryId,NusysConstants.ThumbnailSize.Small, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SMALL_ICON_BYTE_STRING_KEY));
            var mediumIconPath = FileHelper.CreateThumbnailFile(libraryId, NusysConstants.ThumbnailSize.Medium, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_MEDIUM_ICON_BYTE_STRING_KEY));
            var largeIconPath = FileHelper.CreateThumbnailFile(libraryId, NusysConstants.ThumbnailSize.Large, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LARGE_ICON_BYTE_STRING_KEY));
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY] = smallIconPath;
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY] = mediumIconPath;
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY] = largeIconPath;
            if (!addLibraryElementMessage.ContainsKey(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY) ||
                addLibraryElementMessage.GetString(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY).Equals(""))
            {
                addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY] =
                    NusysConstants.AccessType.Private.ToString();
            }
            var success = ContentController.Instance.SqlConnector.AddLibraryElement(addLibraryElementMessage);

            //create a libraryElementModel as requested and serialize it
            var model = LibraryElementModelFactory.CreateFromMessage(addLibraryElementMessage);
            var modelJson = JsonConvert.SerializeObject(model);

            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);// This step is a must since the client must recieve this message an not try to resume an awaiting thread
            forwardMessage[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY] = modelJson;
            NuWebSocketHandler.BroadcastToSubset(forwardMessage,new HashSet<NuWebSocketHandler>() {senderHandler});

            var returnMessage = new Message();
            returnMessage[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY] = modelJson;
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;

            //TESTING STUFF DELETE AFTER FINISHED TESTING***************************
            //ContentController.Instance.SqlConnector.AddStringProperty(
            //    message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY), "test key 1",
            //    "test value 1");
            //ContentController.Instance.SqlConnector.AddStringProperty(
            //    message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY), "test key 2",
            //    "test value 2");
            //ContentController.Instance.SqlConnector.AddStringProperty(
            //    message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY), "test key 3",
            //    "test value 3");
            //****************************************************************    

            return returnMessage;
        }
    }
}