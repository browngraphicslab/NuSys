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

            var selectQuery = CreateGetEntireWorkspaceSqlQuery(workspaceId);

            var keys =
                Constants.GetFullColumnTitles(Constants.SQLTableType.Content, NusysConstants.ACCEPTED_CONTENT_TABLE_KEYS)
                    .Concat(Constants.GetFullColumnTitles(Constants.SQLTableType.Alias,
                        NusysConstants.ALIAS_ACCEPTED_KEYS.Keys)).Concat(new List<string>() {NusysConstants.LIBRARY_ELEMENT_TYPE_KEY});

            var command = "SELECT "+string.Join(",",keys)+" FROM alias LEFT JOIN library_elements ON alias.alias_library_id = library_elements.library_id LEFT JOIN contents ON library_elements.library_element_content_id = contents.content_id WHERE alias.parent_collection_id = '" + workspaceId+"'";

            var args = new SelectCommandReturnArgs(ContentController.Instance.SqlConnector.MakeCommand(command), keys);
            var returnedMessages = ContentController.Instance.SqlConnector.ExecuteSelectQueryAsMessages(args);
            
            var contentDataModels = returnedMessages.Select(m => ContentDataModelFactory.CreateFromMessage(Constants.StripTableNames(m)));
            var aliases = returnedMessages.Select(m => ElementModelFactory.CreateFromMessage(Constants.StripTableNames(m)));

            //create new args to return
            var returnArgs = new GetEntireWorkspaceRequestArgs();

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
        /// <returns></returns>
        private SQLSelectQuery CreateGetEntireWorkspaceSqlQuery(string workspaceId)
        {
            //Joins alias and library element tables where alias.libraryelementid = libraryelement.libraryelementid
            SqlJoinOperationArgs aliasJoinLibraryElementArgs = new SqlJoinOperationArgs();
            aliasJoinLibraryElementArgs.LeftTable = new SingleTable(Constants.SQLTableType.Alias);
            aliasJoinLibraryElementArgs.RightTable = new SingleTable(Constants.SQLTableType.LibraryElement);
            aliasJoinLibraryElementArgs.JoinOperator = Constants.JoinedType.InnerJoin;
            aliasJoinLibraryElementArgs.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.Alias,
                NusysConstants.ALIAS_LIBRARY_ID_KEY).First();
            aliasJoinLibraryElementArgs.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
                NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).First();
            JoinedTable aliasJoinLibraryElement = new JoinedTable(aliasJoinLibraryElementArgs);



            //creates joined table from previous joined table and the content table where the libraryelement.contentid = content.contentid
            SqlJoinOperationArgs aliasJoinLibraryJoinContentArgs = new SqlJoinOperationArgs();
            aliasJoinLibraryJoinContentArgs.LeftTable = aliasJoinLibraryElement;
            aliasJoinLibraryJoinContentArgs.RightTable = new SingleTable(Constants.SQLTableType.Content);
            aliasJoinLibraryJoinContentArgs.JoinOperator = Constants.JoinedType.InnerJoin;
            aliasJoinLibraryJoinContentArgs.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
                NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY).First();
            aliasJoinLibraryJoinContentArgs.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Content,
                NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY).First();
            JoinedTable aliasJoinLibraryJoinContent = new JoinedTable(aliasJoinLibraryJoinContentArgs);

            //creates joined table from previous joined table and properties table where alias.aliasid = properties.aliasorlibraryid
            SqlJoinOperationArgs aliasJoinLibraryJoinContentJoinPropertiesArgs = new SqlJoinOperationArgs();
            aliasJoinLibraryJoinContentJoinPropertiesArgs.LeftTable = aliasJoinLibraryJoinContent;
            aliasJoinLibraryJoinContentJoinPropertiesArgs.RightTable = new SingleTable(Constants.SQLTableType.Properties);
            aliasJoinLibraryJoinContentJoinPropertiesArgs.JoinOperator = Constants.JoinedType.LeftJoin;
            aliasJoinLibraryJoinContentJoinPropertiesArgs.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.Alias,
                NusysConstants.ALIAS_LIBRARY_ID_KEY).First();
            aliasJoinLibraryJoinContentJoinPropertiesArgs.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Properties,
                NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First();
            JoinedTable aliasJoinLibraryJoinContentJoinProperties = new JoinedTable(aliasJoinLibraryJoinContentJoinPropertiesArgs);

            //creates a where query where the alias parent collection is equal the one requested
            var whereQuery = new SqlSelectQueryEquals(Constants.SQLTableType.Alias,
                Constants.GetFullColumnTitle(Constants.SQLTableType.Alias,
                    NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY).First(), workspaceId);

            //creates a list of all columns from alias, content, and properties tables
            var columnsToGet =
                new List<string>(
                    Constants.GetFullColumnTitles(Constants.SQLTableType.Alias, Constants.GetAcceptedKeys(Constants.SQLTableType.Alias))
                        .Concat(Constants.GetFullColumnTitles(Constants.SQLTableType.Content,Constants.GetAcceptedKeys(Constants.SQLTableType.Content)))
                        .Concat(Constants.GetFullColumnTitles(Constants.SQLTableType.Properties, Constants.GetAcceptedKeys(Constants.SQLTableType.Properties))));


            return new SQLSelectQuery(columnsToGet, aliasJoinLibraryJoinContentJoinProperties, whereQuery);
        }
    }
}