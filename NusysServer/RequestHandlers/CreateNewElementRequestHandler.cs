using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class CreateNewElementRequestHandler: RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.NewElementRequest);
            var message = GetRequestMessage(request);

            //TODO not make these debug.asserts
            //Some should throw errors, others aren't necessarily REQUIRED
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY));

            //set the creator of the element as the sender
            message[NusysConstants.NEW_ELEMENT_REQUEST_CREATOR_ID_KEY] = NusysClient.IDtoUsers[senderHandler].UserID;

            //TESTING STUFF DELETE AFTER FINISHED TESTING***************************
            //ContentController.Instance.SqlConnector.AddStringProperty(
            //    message.GetString(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY), "test key 1",
            //    "test value 1");
            //ContentController.Instance.SqlConnector.AddStringProperty(
            //    message.GetString(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY), "test key 2",
            //    "test value 2");
            //ContentController.Instance.SqlConnector.AddStringProperty(
            //    message.GetString(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY), "test key 3",
            //    "test value 3");
            //****************************************************************    


            //query the library elements to get the type
            var typeQuery = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.LibraryElement),
                new SqlQueryEquals(Constants.SQLTableType.LibraryElement,
                    NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY,
                    message.GetString(NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY)));

            var results = typeQuery.ExecuteCommand();
            if (!results.Any())//if there was no library elements, return false
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY , false} });
            }

            //map the request keys tothe database keys
            var addAliasMessage = RequestToSqlKeyMappings.ElementRequestKeysToDatabaseKeys(message);

            //take the first result and set the alias elementType as that returned type
            addAliasMessage[NusysConstants.LIBRARY_ELEMENT_TYPE_KEY] = results.First().GetString(NusysConstants.LIBRARY_ELEMENT_TYPE_KEY);

            //if the message does not contain an access type, default to private
            if (string.IsNullOrEmpty(addAliasMessage.GetString(NusysConstants.ALIAS_ACCESS_KEY)))
            {
                addAliasMessage[NusysConstants.ALIAS_ACCESS_KEY] = NusysConstants.AccessType.Public.ToString();
            }

            var success = ContentController.Instance.SqlConnector.AddAlias(addAliasMessage);

            if (!success)
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false } });
            }
            var model = JsonConvert.SerializeObject(ElementModelFactory.CreateFromMessage(addAliasMessage));

            //if the new element isn't a private alias
            if (addAliasMessage.GetEnum<NusysConstants.AccessType>(NusysConstants.ALIAS_ACCESS_KEY) != NusysConstants.AccessType.Private)
            {
                //forward the element message with the json to other clients
                ForwardMessage(new Message(message) {{NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY, model}}, senderHandler);
            }


            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            returnMessage[NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY] = model;

            return returnMessage;
        }
    }
}