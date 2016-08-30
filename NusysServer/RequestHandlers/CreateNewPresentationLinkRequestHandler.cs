using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class CreateNewPresentationLinkRequestHandler:RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateNewPresentationLinkRequest);
            var message = GetRequestMessage(request);

            //TODO not make these debug.asserts
            //Check to make sure message has everything we need
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY));

            //create new insert into metadata table query and execute
            var messageToPassIntoQuery = GetMessageToPassIntoQuery(message);
            SQLInsertQuery insertPresentationQuery = new SQLInsertQuery(Constants.SQLTableType.PresentationLink, messageToPassIntoQuery);
            var success = insertPresentationQuery.ExecuteCommand();

            //if inserting into the database did not work, return a new message that says we failed.
            if (!success)
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false } });
            }

            //Update the collection's last edited time stamp
            UpdateLibraryElementLastEditedTimeStamp(message.GetString(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY));

            var presentationLink = CreatePresentationLinkModel(messageToPassIntoQuery);
            var modelJson = JsonConvert.SerializeObject(presentationLink);

            //Let everyone except for the original sender know a new metadata entry was created
            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            forwardMessage[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_RETURNED_PRESENTATION_LINK_MODEL_KEY] = modelJson;
            NuWebSocketHandler.BroadcastToSubset(forwardMessage, new HashSet<NuWebSocketHandler>() { senderHandler });

            //Send message back to original request creator that says we were succesful. 
            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            returnMessage[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_RETURNED_PRESENTATION_LINK_MODEL_KEY] = modelJson;

            return returnMessage;
        }

        /// <summary>
        /// Creates a new message that can be passed into the insert into presentation link table query.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Message GetMessageToPassIntoQuery(Message message)
        {
            var messageToPassInQuery = new Message();
            messageToPassInQuery[NusysConstants.PRESENTATION_LINKS_TABLE_IN_ELEMENT_ID_KEY] = message.GetString(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY);
            messageToPassInQuery[NusysConstants.PRESENTATION_LINKS_TABLE_OUT_ELEMENT_ID_KEY] = message.GetString(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY);
            messageToPassInQuery[NusysConstants.PRESENTATION_LINKS_TABLE_LINK_ID_KEY] = message.GetString(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_ID_KEY);
            messageToPassInQuery[NusysConstants.PRESENTATION_LINKS_TABLE_PARENT_COLLECTION_LIBRARY_ID_KEY] = message.GetString(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY);
            messageToPassInQuery[NusysConstants.PRESENTATION_LINKS_TABLE_ANNOTATION_TEXT_KEY] = message.GetString(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY, "");
            return messageToPassInQuery;
        }

        /// <summary>
        /// creates a new presentation link model from the passed in message. The passed in message should be the same message passed into the query.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private PresentationLinkModel CreatePresentationLinkModel(Message message)
        {
            PresentationLinkModel link = new PresentationLinkModel();
            link.LinkId = message.GetString(NusysConstants.PRESENTATION_LINKS_TABLE_LINK_ID_KEY);
            link.InElementId = message.GetString(NusysConstants.PRESENTATION_LINKS_TABLE_IN_ELEMENT_ID_KEY);
            link.OutElementId = message.GetString(NusysConstants.PRESENTATION_LINKS_TABLE_OUT_ELEMENT_ID_KEY);
            link.ParentCollectionId = message.GetString(NusysConstants.PRESENTATION_LINKS_TABLE_PARENT_COLLECTION_LIBRARY_ID_KEY);
            link.AnnotationText = message.GetString(NusysConstants.PRESENTATION_LINKS_TABLE_ANNOTATION_TEXT_KEY);
            return link;
        }
    }
}