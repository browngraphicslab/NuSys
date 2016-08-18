using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Web;
using MuPDFLib;
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

            //If the type is a link, check to see if a link already exists, if it does, return that the request failed.
            if (message.GetEnum<NusysConstants.ElementType>(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY) ==
                NusysConstants.ElementType.Link)
            {
                var inLinkId = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_IN_KEY);
                var outLinkId = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_OUT_KEY);
                
                SqlQueryEquals inIDKeyConditional = new SqlQueryEquals(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_KEY_COLUMN_KEY, NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY);
                SqlQueryEquals outIDKeyConditional = new SqlQueryEquals(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_KEY_COLUMN_KEY, NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY);
                SqlQueryOperator propertyKeyConditional = new SqlQueryOperator(inIDKeyConditional, outIDKeyConditional, Constants.Operator.Or);

                SqlQueryEquals inIdValueConditional = new SqlQueryEquals(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY, inLinkId);
                SqlQueryEquals outIdValueConditional = new SqlQueryEquals(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY, outLinkId);
                SqlQueryOperator propertyValueConditional = new SqlQueryOperator(inIdValueConditional, outIdValueConditional, Constants.Operator.Or);

                var propertyKeyValueConditional = new SqlQueryOperator(propertyKeyConditional, propertyValueConditional, Constants.Operator.And);

                SQLSelectQuery CheckIfLinkExistsQuery = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.Properties, Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY)), propertyKeyValueConditional);

                var fullStringQuery = CheckIfLinkExistsQuery.CommandString + " GROUP BY " + Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First() + " HAVING( COUNT("+ Constants.GetFullColumnTitle(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First() + ") = 2)";
                var cmd = ContentController.Instance.SqlConnector.MakeCommand(fullStringQuery);
                var returnedRows = ContentController.Instance.SqlConnector.ExecuteSelectQueryAsMessages(cmd, false);
                if (returnedRows.Any())
                {
                    var failedReturnMessage = new Message();
                    failedReturnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = false;
                    return failedReturnMessage;
                }
            }


            //create message of database keys from request keys
            var addLibraryElementMessage = RequestToSqlKeyMappings.LibraryElementRequestKeysToDatabaseKeys(message);

            addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY] = NusysClient.IDtoUsers[senderHandler]?.UserID;

            // If an existing URL for the icons was passed in, we use that (this would happen if a library element was being copied.
            // Else, for new library elements, create thumbnails and add the paths to the sql database
            string smallIconPath;
            if (message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_SMALL_ICON_URL))
            {
                smallIconPath = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_SMALL_ICON_URL);
            }
            else
            {
                smallIconPath = FileHelper.CreateThumbnailFile(libraryId, NusysConstants.ThumbnailSize.Small, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SMALL_ICON_BYTE_STRING_KEY));
            }
            string mediumIconPath;
            if (message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_MEDIUM_ICON_URL))
            {
                mediumIconPath = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_MEDIUM_ICON_URL);
            }
            else
            {
                mediumIconPath = FileHelper.CreateThumbnailFile(libraryId, NusysConstants.ThumbnailSize.Medium, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_MEDIUM_ICON_BYTE_STRING_KEY));
            }
            string largeIconPath;
            if (message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_LARGE_ICON_URL))
            {
                largeIconPath = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_LARGE_ICON_URL);
            }
            else
            {
                largeIconPath = FileHelper.CreateThumbnailFile(libraryId, NusysConstants.ThumbnailSize.Large, message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LARGE_ICON_BYTE_STRING_KEY));
            }
            // The URLs are added to the message
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
                var metadataEntries = message.GetList<MetadataEntry>(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_METADATA_KEY);
                var metadataMessagesList = new List<Message>();
                foreach (var metadataEntry in metadataEntries)
                {
                    Message metadataMessage = new Message();
                    metadataMessage[NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY] = libraryId;
                    metadataMessage[NusysConstants.METADATA_KEY_COLUMN_KEY] = metadataEntry.Key;
                    metadataMessage[NusysConstants.METADATA_MUTABILITY_COLUMN_KEY] = metadataEntry.Mutability.ToString();
                    metadataMessage[NusysConstants.METADATA_VALUE_COLUMN_KEY] = JsonConvert.SerializeObject(metadataEntry.Values);
                    metadataMessagesList.Add(metadataMessage);
                }
                if (metadataMessagesList.Any())
                {
                    SQLInsertQuery insertQuery = new SQLInsertQuery(Constants.SQLTableType.Metadata, metadataMessagesList);
                    var metadataSuccess = insertQuery.ExecuteCommand();
                    Debug.Assert(metadataSuccess);
                    addLibraryElementMessage[NusysConstants.LIBRARY_ELEMENT_METADATA_KEY] =
                        JsonConvert.SerializeObject(metadataEntries.ToDictionary(entry => entry.Key, e => e));

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

        public List<byte[]> GetPdfPageBytes(string pdfFilePath)
        {
            int maxSideSize = 2500;

            //create the pdf object
            var pdf = new MuPDF("C:\\Users\\graphics_lab\\Downloads\\CS15_Lecture_19_Hashing_11_12_15.pdf", "");
            pdf.Initialize();

            var byteArrayList = new List<byte[]>(pdf.PageCount);

            var converter = new ImageConverter();//create an image converter
            for (int p = 1; p <= pdf.PageCount; p++)//for each 1-based page...
            {
                pdf.Page = p;//set the page

                var ratio = pdf.Width / pdf.Height;//get the size ratio
                int width;
                int height;
                if (ratio > 1) //depending on the bigger side, set the height and width
                {
                    width = Convert.ToInt32(maxSideSize);
                    height = Convert.ToInt32(maxSideSize / ratio);
                }
                else
                {
                    height = Convert.ToInt32(maxSideSize);
                    width = Convert.ToInt32(maxSideSize / ratio);
                }


                var img = pdf.GetBitmap(width, height, 1, 1, 0, RenderType.RGB, false, false, 50000); //get the image as a bitmap
                var bytes = converter.ConvertTo(img, typeof(byte[])) as byte[];
                byteArrayList.Add(bytes);
            }
            return byteArrayList;
        }
    }
}