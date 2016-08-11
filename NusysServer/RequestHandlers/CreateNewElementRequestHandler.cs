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
            message[NusysConstants.NEW_ELEMENT_REQUEST_CREATOR_ID_KEY] = ActiveClient.ActiveClients[senderHandler].Client.ID;

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


            var addAliasMessage = new Message();
            addAliasMessage[NusysConstants.ALIAS_ID_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY];
            addAliasMessage[NusysConstants.ALIAS_LIBRARY_ID_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY];
            addAliasMessage[NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY];
            addAliasMessage[NusysConstants.ALIAS_LOCATION_X_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY];
            addAliasMessage[NusysConstants.ALIAS_LOCATION_Y_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY];
            addAliasMessage[NusysConstants.ALIAS_SIZE_HEIGHT_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY];
            addAliasMessage[NusysConstants.ALIAS_SIZE_WIDTH_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY];
            addAliasMessage[NusysConstants.ALIAS_CREATOR_ID_KEY] = message[NusysConstants.NEW_ELEMENT_REQUEST_CREATOR_ID_KEY];

            //take the first result and set the alias elementType as that returned type
            addAliasMessage[NusysConstants.LIBRARY_ELEMENT_TYPE_KEY] = results.First().GetString(NusysConstants.LIBRARY_ELEMENT_TYPE_KEY);
            if (!addAliasMessage.ContainsKey(NusysConstants.ALIAS_ACCESS_KEY) ||
                addAliasMessage.GetString(NusysConstants.ALIAS_ACCESS_KEY).Equals(""))
            {
                addAliasMessage[NusysConstants.ALIAS_ACCESS_KEY] =
                    NusysConstants.AccessType.Private.ToString();
            }
            var success = ContentController.Instance.SqlConnector.AddAlias(addAliasMessage);

            if (!success)
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false } });
            }
            var model = JsonConvert.SerializeObject(ElementModelFactory.CreateFromMessage(addAliasMessage));

            var forwardMessage = new Message(message);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            forwardMessage[NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY] = model;
            NuWebSocketHandler.BroadcastToSubset(forwardMessage, new HashSet<NuWebSocketHandler>() { senderHandler });

            var returnMessage = new Message();
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            returnMessage[NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY] = model;

            return returnMessage;
        }
    }
}