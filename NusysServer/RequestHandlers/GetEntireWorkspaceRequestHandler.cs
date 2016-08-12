using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
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
            var userId = ActiveClient.ActiveClients[senderHandler]?.Client?.ID;
            if (userId == null)
            {
                throw new Exception("The client has no id");
            }

            //var selectQuery = CreateGetEntireWorkspaceSqlQuery(workspaceId);

            var keys =
                Constants.GetFullColumnTitles(Constants.SQLTableType.Content, NusysConstants.ACCEPTED_CONTENT_TABLE_KEYS)
                    .Concat(Constants.GetFullColumnTitles(Constants.SQLTableType.Alias,
                        NusysConstants.ALIAS_ACCEPTED_KEYS.Keys)).Concat(new List<string>() {NusysConstants.LIBRARY_ELEMENT_TYPE_KEY});

            //var command = "SELECT "+string.Join(",",keys)+" FROM "+Constants.GetTableName(Constants.SQLTableType.Alias)+ " LEFT JOIN " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + " ON " + Constants.GetTableName(Constants.SQLTableType.Alias) + ".library_id = " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + ".library_id LEFT JOIN " + Constants.GetTableName(Constants.SQLTableType.Content) + " ON " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + ".content_id = " + Constants.GetTableName(Constants.SQLTableType.Content) + ".content_id WHERE " + Constants.GetTableName(Constants.SQLTableType.Alias) + ".parent_collection_id = '" + workspaceId+"'";
            //var command = "SELECT "+string.Join(",",keys)+" FROM alias LEFT JOIN library_elements ON alias.library_id = library_elements.library_id LEFT JOIN contents ON library_elements.content_id = contents.content_id WHERE alias.parent_collection_id = '" + workspaceId+"'";
            
            //var command = "WITH q AS (SELECT * FROM " + Constants.GetTableName(Constants.SQLTableType.Alias) +
            //    " WHERE " + Constants.GetTableName(Constants.SQLTableType.Alias) + "." + NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = '" + workspaceId + "' " + "UNION ALL SELECT m.* FROM " +
            //    Constants.GetTableName(Constants.SQLTableType.Alias) + " m JOIN q ON m." + NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = q." +
            //    NusysConstants.ALIAS_LIBRARY_ID_KEY + ") SELECT q.*, " + Constants.GetTableName(Constants.SQLTableType.Properties) +
            //    ".*, " + Constants.GetTableName(Constants.SQLTableType.Content) + ".*, " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + ".type FROM q LEFT JOIN " +
            //    Constants.GetTableName(Constants.SQLTableType.LibraryElement) + " ON q." + NusysConstants.ALIAS_LIBRARY_ID_KEY + " = "
            //    + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + "." + NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY + " LEFT JOIN " +
            //    Constants.GetTableName(Constants.SQLTableType.Content) + " ON " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + "." + NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY + " = "
            //    + Constants.GetTableName(Constants.SQLTableType.Content) + "." + NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY +
            //    " LEFT JOIN " + Constants.GetTableName(Constants.SQLTableType.Properties) + " on " +
            //    Constants.GetTableName(Constants.SQLTableType.Properties) + "." + NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY + " = q." + NusysConstants.ALIAS_ID_KEY + " WHERE q." + NusysConstants.ALIAS_ACCESS_KEY + " = '" + NusysConstants.AccessType.Public + "' OR q." + NusysConstants.ALIAS_ACCESS_KEY + " = '" + NusysConstants.AccessType.ReadOnly + "' OR q." + NusysConstants.ALIAS_CREATOR_ID_KEY + " = '" + userId + "'";


            var returnedMessages = ContentController.Instance.SqlConnector.ExecuteSelectQueryAsMessages(CreateGetEntireWorkspaceSqlQuery(workspaceId, userId, 2));
            

            PropertiesParser propertiesParser = new PropertiesParser();
            var concatPropertiesReturnedMessages = propertiesParser.ConcatMessageProperties(returnedMessages);

            var stripped = concatPropertiesReturnedMessages.Select(m => Constants.StripTableNames(m));

            //really, just dont ask.  all you need to know is that it converts the url to the correct data string for the content data model
            var cleaned = stripped.Select(strippedMessage => new Message(strippedMessage.Concat(new List<KeyValuePair<string, object>>() {new KeyValuePair<string, object>(
                NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY, FileHelper.GetDataFromContentURL(
                         strippedMessage.GetString(NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY),
                         strippedMessage.GetEnum<NusysConstants.ContentType>(NusysConstants.CONTENT_TABLE_TYPE_KEY)))}).ToDictionary(x => x.Key, y => y.Value)));

            var contentDataModels = cleaned.Select(m => ContentDataModelFactory.CreateFromMessage(m));
            var aliases = cleaned.Select(m => ElementModelFactory.CreateFromMessage(m));

            //create new args to return
            var returnArgs = new GetEntireWorkspaceRequestReturnArgs();

            returnArgs.ContentMessages = contentDataModels.Select(m => JsonConvert.SerializeObject(m));
            returnArgs.AliasStrings = aliases.Select(m => JsonConvert.SerializeObject(m));

            var returnMessage = new Message();
            returnMessage[NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY] = returnArgs;
            return returnMessage;
        }

        /// <summary>
        /// Creates a select query for getting all information for the get entire workspace query for the specified workspace id.
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="levelsDeep">Either 0, 1, or 2. Represents how many levels down collections we want to go</param>
        /// <returns></returns>
        private SqlCommand CreateGetEntireWorkspaceSqlQuery(string workspaceId, string userId, int levelsDeep)
        {
            var command = "";
            switch (levelsDeep)
            {
                case 0:
                    command = "WITH q AS (SELECT * FROM " +Constants.GetTableName(Constants.SQLTableType.Alias) + " where "+ NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = '" + workspaceId+ "')";
                    break;
                case 1:
                    command = "WITH p AS (SELECT * FROM " +Constants.GetTableName(Constants.SQLTableType.Alias)+ " where " + NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = '" + workspaceId+ "'), " +
                              "q AS(SELECT * FROM p UNION SELECT m.* from " + Constants.GetTableName(Constants.SQLTableType.Alias) + " m JOIN p ON m." + NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = p." + NusysConstants.ALIAS_LIBRARY_ID_KEY + ")";
                    break;
                case 2:
                    command = "WITH p AS (SELECT * FROM " + Constants.GetTableName(Constants.SQLTableType.Alias) + " where " + NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = '" + workspaceId + "'), w AS(SELECT * FROM p UNION SELECT m.* from " + Constants.GetTableName(Constants.SQLTableType.Alias) + " m JOIN p ON m." + NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = p." + NusysConstants.ALIAS_LIBRARY_ID_KEY + "), q AS(SELECT * FROM w UNION SELECT m.* from " + Constants.GetTableName(Constants.SQLTableType.Alias)+ " m JOIN w ON m." + NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = w." + NusysConstants.ALIAS_LIBRARY_ID_KEY + ")";
                    break;
            }
            command = command + " SELECT q.*, " + Constants.GetTableName(Constants.SQLTableType.Properties) +
            ".*, " + Constants.GetTableName(Constants.SQLTableType.Content) + ".*, " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + ".type FROM q LEFT JOIN " +
            Constants.GetTableName(Constants.SQLTableType.LibraryElement) + " ON q." + NusysConstants.ALIAS_LIBRARY_ID_KEY + " = "
            + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + "." + NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY + " LEFT JOIN " +
            Constants.GetTableName(Constants.SQLTableType.Content) + " ON " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + "." + NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY + " = "
            + Constants.GetTableName(Constants.SQLTableType.Content) + "." + NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY +
            " LEFT JOIN " + Constants.GetTableName(Constants.SQLTableType.Properties) + " on " +
            Constants.GetTableName(Constants.SQLTableType.Properties) + "." + NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY + " = q." + NusysConstants.ALIAS_ID_KEY + " WHERE q." + NusysConstants.ALIAS_ACCESS_KEY + " = '" + NusysConstants.AccessType.Public + "' OR q." + NusysConstants.ALIAS_ACCESS_KEY + " = '" + NusysConstants.AccessType.ReadOnly + "' OR q." + NusysConstants.ALIAS_CREATOR_ID_KEY + " = '" + userId + "'";
            return ContentController.Instance.SqlConnector.MakeCommand(command);

        }
    }
}