using System;
using System.Collections.Generic;
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

            //var selectQuery = CreateGetEntireWorkspaceSqlQuery(workspaceId);

            var keys =
                Constants.GetFullColumnTitles(Constants.SQLTableType.Content, NusysConstants.ACCEPTED_CONTENT_TABLE_KEYS)
                    .Concat(Constants.GetFullColumnTitles(Constants.SQLTableType.Alias,
                        NusysConstants.ALIAS_ACCEPTED_KEYS.Keys)).Concat(new List<string>() {NusysConstants.LIBRARY_ELEMENT_TYPE_KEY});

            //var command = "SELECT "+string.Join(",",keys)+" FROM "+Constants.GetTableName(Constants.SQLTableType.Alias)+ " LEFT JOIN " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + " ON " + Constants.GetTableName(Constants.SQLTableType.Alias) + ".library_id = " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + ".library_id LEFT JOIN " + Constants.GetTableName(Constants.SQLTableType.Content) + " ON " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + ".content_id = " + Constants.GetTableName(Constants.SQLTableType.Content) + ".content_id WHERE " + Constants.GetTableName(Constants.SQLTableType.Alias) + ".parent_collection_id = '" + workspaceId+"'";
            //var command = "SELECT "+string.Join(",",keys)+" FROM alias LEFT JOIN library_elements ON alias.library_id = library_elements.library_id LEFT JOIN contents ON library_elements.content_id = contents.content_id WHERE alias.parent_collection_id = '" + workspaceId+"'";
            var command = "WITH q AS (SELECT * FROM " + Constants.GetTableName(Constants.SQLTableType.Alias) + 
                " WHERE " + Constants.GetTableName(Constants.SQLTableType.Alias) + "." + NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " = '"  + workspaceId + "' "+ "UNION ALL SELECT m.* FROM " + 
                Constants.GetTableName(Constants.SQLTableType.Alias) + " m JOIN q ON m."+NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY+" = q." +
                NusysConstants.ALIAS_LIBRARY_ID_KEY+") SELECT q.*, " + Constants.GetTableName(Constants.SQLTableType.Properties) + 
                ".*, " + Constants.GetTableName(Constants.SQLTableType.Content) + ".*, " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + ".type FROM q LEFT JOIN " + 
                Constants.GetTableName(Constants.SQLTableType.LibraryElement) + " ON q."+NusysConstants.ALIAS_LIBRARY_ID_KEY+" = " 
                + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + "."+NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY+" LEFT JOIN " + 
                Constants.GetTableName(Constants.SQLTableType.Content) + " ON " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + "."+NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY+" = " 
                + Constants.GetTableName(Constants.SQLTableType.Content) + "."+NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY+
                " LEFT JOIN " + Constants.GetTableName(Constants.SQLTableType.Properties) + " on " +
                Constants.GetTableName(Constants.SQLTableType.Properties) + "."+NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY+" = q."+NusysConstants.ALIAS_ID_KEY;

            var cmd = ContentController.Instance.SqlConnector.MakeCommand(command);
            var returnedMessages = ContentController.Instance.SqlConnector.ExecuteSelectQueryAsMessages(cmd);
            

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

        

        ///// <summary>
        ///// Creates a select query for getting all information for the get entire workspace query for the specified workspace id.
        ///// </summary>
        ///// <param name="workspaceId"></param>
        ///// <returns></returns>
        //private SQLSelectQuery CreateGetEntireWorkspaceSqlQuery(string workspaceId)
        //{
        //    //Joins alias and library element tables where alias.libraryelementid = libraryelement.libraryelementid
        //    SqlJoinOperationArgs aliasJoinLibraryElementArgs = new SqlJoinOperationArgs();
        //    aliasJoinLibraryElementArgs.LeftTable = new SingleTable(Constants.SQLTableType.Alias);
        //    aliasJoinLibraryElementArgs.RightTable = new SingleTable(Constants.SQLTableType.LibraryElement);
        //    aliasJoinLibraryElementArgs.JoinOperator = Constants.JoinedType.LeftJoin;
        //    aliasJoinLibraryElementArgs.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.Alias,
        //        NusysConstants.ALIAS_LIBRARY_ID_KEY).First();
        //    aliasJoinLibraryElementArgs.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
        //        NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).First();
        //    JoinedTable aliasJoinLibraryElement = new JoinedTable(aliasJoinLibraryElementArgs);



        //    //creates joined table from previous joined table and the content table where the libraryelement.contentid = content.contentid
        //    SqlJoinOperationArgs aliasJoinLibraryJoinContentArgs = new SqlJoinOperationArgs();
        //    aliasJoinLibraryJoinContentArgs.LeftTable = aliasJoinLibraryElement;
        //    aliasJoinLibraryJoinContentArgs.RightTable = new SingleTable(Constants.SQLTableType.Content);
        //    aliasJoinLibraryJoinContentArgs.JoinOperator = Constants.JoinedType.LeftJoin;
        //    aliasJoinLibraryJoinContentArgs.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
        //        NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY).First();
        //    aliasJoinLibraryJoinContentArgs.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Content,
        //        NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY).First();
        //    JoinedTable aliasJoinLibraryJoinContent = new JoinedTable(aliasJoinLibraryJoinContentArgs);

        //    //creates joined table from previous joined table and properties table where alias.aliasid = properties.aliasorlibraryid
        //    SqlJoinOperationArgs aliasJoinLibraryJoinContentJoinPropertiesArgs = new SqlJoinOperationArgs();
        //    aliasJoinLibraryJoinContentJoinPropertiesArgs.LeftTable = aliasJoinLibraryJoinContent;
        //    aliasJoinLibraryJoinContentJoinPropertiesArgs.RightTable = new SingleTable(Constants.SQLTableType.Properties);
        //    aliasJoinLibraryJoinContentJoinPropertiesArgs.JoinOperator = Constants.JoinedType.LeftJoin;
        //    aliasJoinLibraryJoinContentJoinPropertiesArgs.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.Alias,
        //        NusysConstants.ALIAS_ID_KEY).First();
        //    aliasJoinLibraryJoinContentJoinPropertiesArgs.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Properties,
        //        NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First();
        //    JoinedTable aliasJoinLibraryJoinContentJoinProperties = new JoinedTable(aliasJoinLibraryJoinContentJoinPropertiesArgs);

        //    //creates a where query where the alias parent collection is equal the one requested
        //    var whereQuery = new SqlSelectQueryEquals(Constants.SQLTableType.Alias,
        //            NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY, workspaceId);

        //    //creates a list of all columns from alias, content, and properties tables
        //    var columnsToGet =
        //        new List<string>((
        //            Constants.GetAcceptedKeys(Constants.SQLTableType.Alias))
        //                .Concat(Constants.GetAcceptedKeys(Constants.SQLTableType.Content))
        //                .Concat(Constants.GetAcceptedKeys(Constants.SQLTableType.Properties)));
        //    var keys =
        //        Constants.GetFullColumnTitles(Constants.SQLTableType.Content, NusysConstants.ACCEPTED_CONTENT_TABLE_KEYS)
        //            .Concat(Constants.GetFullColumnTitles(Constants.SQLTableType.Alias,
        //                NusysConstants.ALIAS_ACCEPTED_KEYS.Keys)).Concat(Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_TYPE_KEY)).Concat(Constants.GetAcceptedKeys(Constants.SQLTableType.Properties));

        //    return new SQLSelectQuery(keys, aliasJoinLibraryJoinContentJoinProperties, whereQuery);
        //}
    }
}