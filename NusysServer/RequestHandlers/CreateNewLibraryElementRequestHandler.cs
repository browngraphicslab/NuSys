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

            var libraryId = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY);

            //create message of database keys from request keys
            var addLibraryElementMessage = RequestToSqlKeyMappings.LibraryElementRequestKeysToDatabaseKeys(message);

            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY] = NusysClient.IDtoUsers[senderHandler]?.UserID;

            //create thumbnails and add the paths to the sql database
            var smallIconPath = FileHelper.CreateThumbnailFile(libraryId,NusysConstants.ThumbnailSize.Small, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SMALL_ICON_BYTE_STRING_KEY));
            var mediumIconPath = FileHelper.CreateThumbnailFile(libraryId, NusysConstants.ThumbnailSize.Medium, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_MEDIUM_ICON_BYTE_STRING_KEY));
            var largeIconPath = FileHelper.CreateThumbnailFile(libraryId, NusysConstants.ThumbnailSize.Large, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LARGE_ICON_BYTE_STRING_KEY));
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY] = smallIconPath;
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY] = mediumIconPath;
            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY] = largeIconPath;

            //if the request didn't specify a access Control type,
            if (!addLibraryElementMessage.ContainsKey(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY) ||
                addLibraryElementMessage.GetString(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY).Equals(""))
            {
                //default to private
                addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY] = NusysConstants.AccessType.Private.ToString();
            }
            var success = ContentController.Instance.SqlConnector.AddLibraryElement(addLibraryElementMessage);

            //insert all the metadata
            if (message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_METADATA_KEY))
            {
                var metadataEntries =
                    message.GetDict<string, MetadataEntry>(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_METADATA_KEY).Values;
                var metadataMessagesList = new List<Message>();
                foreach (var metadataEntry in metadataEntries)
                {
                    Message metadataMessage = new Message();
                    metadataMessage[NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY] = libraryId;
                    metadataMessage[NusysConstants.METADATA_KEY_COLUMN_KEY] = metadataEntry.Key;
                    metadataMessage[NusysConstants.METADATA_MUTABILITY_COLUMN_KEY] = metadataEntry.Mutability.ToString();
                    metadataMessage[NusysConstants.METADATA_VALUE_COLUMN_KEY] = JsonConvert.SerializeObject(metadataEntry.Values);
                    metadataMessagesList.Add(message);
                }
                if (metadataMessagesList.Any())
                {
                    SQLInsertQuery insertQuery = new SQLInsertQuery(Constants.SQLTableType.Metadata, metadataMessagesList);
                    var metadataSuccess = insertQuery.ExecuteCommand();
                    Debug.Assert(metadataSuccess);
                    addLibraryElementMessage[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_METADATA_KEY] =
                        JsonConvert.SerializeObject(metadataEntries);

                }
            }

            //create a libraryElementModel as requested and serialize it
            var model = LibraryElementModelFactory.CreateFromMessage(addLibraryElementMessage);
            var modelJson = JsonConvert.SerializeObject(model);

            //if the library element doesn't have the access Type of private,
            if(addLibraryElementMessage.GetEnum<NusysConstants.AccessType>(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY) != NusysConstants.AccessType.Private) { 
                //forward the message to everyone else, and just add the new model json
                ForwardMessage(new Message(message) { { NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY, modelJson }},senderHandler);
            }

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