﻿using System;
using System.Diagnostics;
using NusysIntermediate;

namespace NusysServer
{
    public class CreateNewContentRequestHandler : RequestHandler
    {
        /// <summary>
        /// should broadcast the new library element to all clients, shoudl return to the sender with the library element model as well
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateNewContentRequest);
            var message = GetRequestMessage(request);
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES));
            var returnMessage = new Message();

            Message addContentToDatabaseMessage = CreateAddContentToDatabaseMessage(message);

            //try to add new content to the sql database
            var createNewContentsuccess = ContentController.Instance.SqlConnector.AddContent(addContentToDatabaseMessage);

            //if could not add content sql database, return a message that the request failed
            if (createNewContentsuccess == false)
            {
                returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = false;
                return returnMessage;
            }

            //if content has been successfully added remove the content part of the message and add a new library element model
            message.Remove(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY);
            message.Remove(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY);
            message.Remove(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES);

            var createNewLibraryRequest = new Request(NusysConstants.RequestType.CreateNewLibraryElementRequest, message);
            var createNewLibraryElementRequestHandler = new CreateNewLibraryElementRequestHandler();
            
            //return a message saying whether content and library element model were successfully created
            var libraryElementRequestMessage = createNewLibraryElementRequestHandler.HandleRequest(createNewLibraryRequest, senderHandler);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = libraryElementRequestMessage.GetBool(NusysConstants.REQUEST_SUCCESS_BOOL_KEY);

            //if the library element request was successful.
            if (libraryElementRequestMessage.GetBool(NusysConstants.REQUEST_SUCCESS_BOOL_KEY))
            {
                //add the json-libraryElementModel from the libraryElementModel request to our returned message
                returnMessage[NusysConstants.NEW_CONTENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY] =
                    libraryElementRequestMessage[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY];
            }

            if (Constants.user == "junsu" && 
                message.ContainsKey("type") &&
                message.GetEnum<NusysConstants.ContentType>("type") == NusysConstants.ContentType.PDF)//obviously only for testing
            {
                var tup = new Tuple<string, string>(FileHelper.GetDataFromContentURL(addContentToDatabaseMessage.GetString(NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY), NusysConstants.ContentType.PDF), message.GetString(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY));
                ContentController.Instance.ComparisonController.AddDocument(tup);
                ContentController.Instance.ComparisonController.CompareRandonDoc();
            }

            return returnMessage;
        }

        /// <summary>
        /// based on the content type being uploaded, returns a suitable message to be passed into the sql connector to create a new row in the content table
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="originalMessage"></param>
        /// <returns></returns>
        private Message CreateAddContentToDatabaseMessage(Message originalMessage)
        {
            //try to get the type of content being added
            var contentType = (NusysConstants.ContentType)Enum.Parse(typeof(NusysConstants.ContentType),originalMessage.GetString(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY), true);
            Message addContentToDatabaseMessage = new Message();

            //depending on type of content, create new URL, or create new file in the server

            addContentToDatabaseMessage[NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY] = originalMessage[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY];
            addContentToDatabaseMessage[NusysConstants.CONTENT_TABLE_TYPE_KEY] = originalMessage[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY];

            addContentToDatabaseMessage[NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY] = FileHelper.CreateDataFile(originalMessage.Get(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY), contentType, originalMessage.GetString(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES), originalMessage.Get(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_FILE_EXTENTION));
            return addContentToDatabaseMessage;
        }

        
    }
}