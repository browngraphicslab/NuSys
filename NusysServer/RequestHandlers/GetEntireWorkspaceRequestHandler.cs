using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class GetEntireWorkspaceRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetEntireWorkspaceRequest);

            var message = GetRequestMessage(request);

            Debug.Assert(message.ContainsKey(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY));

            var workspaceId = message.GetString(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY);

            //todo actually get the info

            //store instance of sql connector
            var sql = ContentController.Instance.SqlConnector;


            var args = new SqlSelectQueryArgs();
            args.ColumnsToGet = Constants.GetFullColumnTitles(Constants.SQLTableType.Alias,NusysConstants.ALIAS_ACCEPTED_KEYS.Keys).
                Concat(Constants.GetFullColumnTitles(Constants.SQLTableType.Content,NusysConstants.ACCEPTED_CONTENT_TABLE_KEYS));

            args.Condition = new LeftJoinWhereCondition(
                new SqlTableRepresentation(
                    new InnerJoinWhereCondition(
                        new SqlTableRepresentation(Constants.SQLTableType.Alias), 
                        new SqlTableRepresentation(Constants.SQLTableType.LibrayElement), 
                        new SqlSelectQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_LIBRARY_ID_KEY,Constants.GetFullColumnTitle(Constants.SQLTableType.LibrayElement, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).FirstOrDefault()))),
                new SqlTableRepresentation(Constants.SQLTableType.Content),new SqlSelectQueryEquals(Constants.SQLTableType.Content,NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY, Constants.GetFullColumnTitle(Constants.SQLTableType.LibrayElement, NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY).FirstOrDefault()));






            //create arguments for selecting all libreary elements
            var aliasArgs = new SqlSelectQueryArgs();
            aliasArgs.ColumnsToGet = NusysConstants.ALIAS_ACCEPTED_KEYS.Keys;
            aliasArgs.TableType = Constants.SQLTableType.Alias;
            //aliasArgs.Condition = new SqlSelectQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_LIBRARY_ID_KEY,workspaceId);
            var aliasCmdArgs = sql.GetSelectCommand(aliasArgs);
            var elementMessages = sql.ExecuteSelectQueryAsMessages(aliasCmdArgs);

            //after query execution, map all the messages to the libraryId for that message
            var elementMap = new Dictionary<string, Message>();

            //also create a hashset for all the needed contentIds

            foreach (var elementMessage in elementMessages)
            {
                if (elementMessage.ContainsKey(NusysConstants.ALIAS_LIBRARY_ID_KEY))
                {
                    elementMap[elementMessage[NusysConstants.ALIAS_LIBRARY_ID_KEY].ToString()] = elementMessage;
                }
            }

            //create select query for getting the properties of the gotten aliases
            var propertiesArgs = new SqlSelectQueryArgs();
            propertiesArgs.ColumnsToGet = NusysConstants.ACCEPTED_PROPERTIES_TABLE_KEYS;
            propertiesArgs.TableType = Constants.SQLTableType.Properties;
            //propertiesArgs.Condition = new SqlSelectQueryContains(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY, new List<string>(elementMap.Keys));
            var propertiesCommand = sql.GetSelectCommand(propertiesArgs);

            //after execution, map messages back to original mapping
            var propertiesMessages = sql.ExecuteSelectQueryAsMessages(propertiesCommand, false);
            foreach (var property in propertiesMessages)
            {
                var key = property[NusysConstants.PROPERTIES_KEY_COLUMN_KEY].ToString();
                property.Remove(NusysConstants.PROPERTIES_KEY_COLUMN_KEY);

                var libraryId = property[NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY].ToString();
                property.Remove(NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY);

                if (property.Keys.Any(k => property[k] != null && property[k].ToString() != ""))
                {
                    elementMap[libraryId].Add(key, property[property.Keys.First(k => property[k] != null && property[k].ToString() != "")]);
                }
                else if (property.Keys.Any())//if there is a non-null value left
                {
                    elementMap[libraryId].Add(key, property[property.Keys.First()]);
                }
            }


            //time to get all the needed contents


            //create new args to return
            var returnArgs = new GetEntireWorkspaceRequestArgs();

            returnArgs.AliasStrings = elementMap.Values.Select(value => value.GetSerialized());
            returnArgs.ContentMessages = null; //TODO fill this in

            var returnMessage = new Message();
            returnMessage[NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY] = returnArgs;
            return returnMessage;
        }
    }
}