using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;
using System.Diagnostics;
using Newtonsoft.Json;

namespace NusysServer
{
    public class CreateNewMetadataRequestHandler :RequestHandler
    {
        /// <summary>
        /// Tries to insert new metadata entry to the database and lets everyone know about the new info.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateNewMetadataRequest);
            var message = GetRequestMessage(request);

            //TODO not make these debug.asserts
            //Check to make sure message has everything we need
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY));
            
            //create new insert into metadata table query and execute
            var messageToPassIntoQuery = GetMessageToPassIntoQuery(message);
            SQLInsertQuery insertMetadataQuery = new SQLInsertQuery(Constants.SQLTableType.Metadata, messageToPassIntoQuery);
            var success = insertMetadataQuery.ExecuteCommand();

            //if inserting into the database did not work, return a new message that says we failed.
            if (!success)
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false } });
            }

            var entry = CreateMetadataEntry(messageToPassIntoQuery);
            var modelJson = JsonConvert.SerializeObject(entry);

            //Let everyone except for the original sender know a new metadata entry was created
            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            forwardMessage[NusysConstants.CREATE_NEW_METADATA_REQUEST_RETURNED_METADATA_ENTRY_KEY] = modelJson;
            NuWebSocketHandler.BroadcastToSubset(forwardMessage, new HashSet<NuWebSocketHandler>() { senderHandler });
            
            //Send message back to original request creator that says we were succesful. 
            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            returnMessage[NusysConstants.CREATE_NEW_METADATA_REQUEST_RETURNED_METADATA_ENTRY_KEY] = modelJson;


            return returnMessage;
        }
        
        /// <summary>
        /// Creates a new message that can be passed into the insert into metadata table query.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Message GetMessageToPassIntoQuery(Message message)
        {
            var messageToPassInQuery = new Message();
            var key = message.GetString(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY);
            var value = JsonConvert.SerializeObject(message.GetList<string>(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY));
            var libraryid = message.GetString(NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY);
            messageToPassInQuery[NusysConstants.METADATA_KEY_COLUMN_KEY] = key;
            messageToPassInQuery[NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY] = libraryid;
            messageToPassInQuery[NusysConstants.METADATA_VALUE_COLUMN_KEY] = value;
            string mutability = "";
            if (message.ContainsKey(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY))
            {
                mutability = message.GetString(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY);
            }
            else
            {
                mutability = MetadataMutability.MUTABLE.ToString();
            }
            messageToPassInQuery[NusysConstants.METADATA_MUTABILITY_COLUMN_KEY] = mutability;
            return messageToPassInQuery;
        }


        /// <summary>
        /// creates a new metadata entry instance from the passed in message. The passed in message should be the same message passed into the query.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private MetadataEntry CreateMetadataEntry(Message message)
        {
            var key = message.GetString(NusysConstants.METADATA_KEY_COLUMN_KEY);
            var value = JsonConvert.SerializeObject(message.GetList<string>(NusysConstants.METADATA_VALUE_COLUMN_KEY));
            var libraryid = message.GetString(NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY);
            var mutability = message.GetString(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY);
            MetadataEntry entry = new MetadataEntry(key, JsonConvert.DeserializeObject<List<string>>(value), (MetadataMutability)Enum.Parse(typeof(MetadataMutability), mutability));
            return entry;
        }
    }
}