using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;
using NusysServer.Util;

namespace NusysServer
{
    public class CreateSnapshotOfCollectionRequestHandler : RequestHandler
    {
        private Dictionary<string, string> _oldIdToNewId;
        /// <summary>
        /// Duplicate collection library element including properties and metadata
        /// Duplicate all aliases and properties giving each alias a new id and a new parent collection id
        /// Duplicate all Presentation links
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            var message = GetRequestMessage(request);
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateSnapshotOfCollectionRequest);
            Debug.Assert(message.ContainsKey(NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_COLLECTION_ID));
            var workspaceId = message.GetString(NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_COLLECTION_ID);
            var newWorkspaceId = NusysConstants.GenerateId();
            var model = DuplicateCollectionLibraryElement(workspaceId, newWorkspaceId);
            var modelJson = JsonConvert.SerializeObject(model);
            var aliasSuccess = DuplicateAliasesAndProperties(workspaceId, newWorkspaceId);
            var presentaionlinksSuccess = DuplicatePresentationLinks(workspaceId, newWorkspaceId);
            if (model.AccessType != NusysConstants.AccessType.Private)
            {
                //forward the message to everyone else, and just add the new model json
                ForwardMessage(new Message(message) { { NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY, modelJson } }, senderHandler);
            }

            var returnMessage = new Message();
            returnMessage[NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_RETURNED_COLLECTION_LIBRARY_ELEMENT_MODEL] = modelJson;
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = model != null && aliasSuccess && presentaionlinksSuccess;
            return returnMessage;

        }

        /// <summary>
        /// This duplicates the presentation links giving the links the new ids of the duplicated aliases and the new workspace id. Returns whether the duplication was succesful or not
        /// </summary>
        /// <returns></returns>
        private bool DuplicatePresentationLinks(string workspaceId, string newWorkspaceId)
        {
            SQLSelectQuery SelectPresentationLinks = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.PresentationLink), new SqlQueryEquals(Constants.SQLTableType.PresentationLink, NusysConstants.PRESENTATION_LINKS_TABLE_PARENT_COLLECTION_LIBRARY_ID_KEY, workspaceId));
            var listOfPresentationLinkMessages = SelectPresentationLinks.ExecuteCommand();
            foreach (var presentationLinkMessage in listOfPresentationLinkMessages)
            {
                presentationLinkMessage[NusysConstants.PRESENTATION_LINKS_TABLE_LINK_ID_KEY] =
                    NusysConstants.GenerateId();
                presentationLinkMessage[NusysConstants.PRESENTATION_LINKS_TABLE_PARENT_COLLECTION_LIBRARY_ID_KEY] =
                    newWorkspaceId;
                presentationLinkMessage[NusysConstants.PRESENTATION_LINKS_TABLE_IN_ELEMENT_ID_KEY] =
                    _oldIdToNewId[
                        presentationLinkMessage.GetString(NusysConstants.PRESENTATION_LINKS_TABLE_IN_ELEMENT_ID_KEY)];
                presentationLinkMessage[NusysConstants.PRESENTATION_LINKS_TABLE_OUT_ELEMENT_ID_KEY] =
                    _oldIdToNewId[
                        presentationLinkMessage.GetString(NusysConstants.PRESENTATION_LINKS_TABLE_OUT_ELEMENT_ID_KEY)];
            }
            if (listOfPresentationLinkMessages.Any())
            {
                SQLInsertQuery insertPresentationLinksQuery = new SQLInsertQuery(Constants.SQLTableType.PresentationLink, new List<Message>(listOfPresentationLinkMessages));
                return insertPresentationLinksQuery.ExecuteCommand();
            }
            return true;
        }

        /// <summary>
        /// This duplicates all the aliases that belong in the collection. It also duplicates all the properties of the aliases.
        /// </summary>
        /// <param name="oldWorkspaceId"></param>
        /// <param name="newWorkspaceId"></param>
        /// <returns></returns>
        private bool DuplicateAliasesAndProperties(string oldWorkspaceId, string newWorkspaceId)
        {
            SqlJoinOperationArgs aliasJoinPropertiesArgs = new SqlJoinOperationArgs();
            aliasJoinPropertiesArgs.LeftTable = new SingleTable(Constants.SQLTableType.Alias);
            aliasJoinPropertiesArgs.RightTable = new SingleTable(Constants.SQLTableType.Properties);
            aliasJoinPropertiesArgs.JoinOperator = Constants.JoinedType.LeftJoin;
            aliasJoinPropertiesArgs.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.Alias,
                NusysConstants.ALIAS_ID_KEY).First();
            aliasJoinPropertiesArgs.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Properties,
                NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First();
            JoinedTable aliasJoinedProperties = new JoinedTable(aliasJoinPropertiesArgs);

            SqlQueryEquals parentCollectionEqualsCollectionId = new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY, oldWorkspaceId);
            SQLSelectQuery SelectAliases = new SQLSelectQuery(aliasJoinedProperties, parentCollectionEqualsCollectionId);
            var parser = new PropertiesParser();
            var listOfAliasWithPropertiesMessages = parser.ConcatMessageProperties(SelectAliases.ExecuteCommand());
            _oldIdToNewId = new Dictionary<string, string>();
            List<Message> aliasesToInsert = new List<Message>();
            List<Message> propertiesToInsert = new List<Message>();
            foreach (var aliasWithPropertiesMessage in listOfAliasWithPropertiesMessages)
            {
                var newId = NusysConstants.GenerateId();
                _oldIdToNewId.Add(aliasWithPropertiesMessage.GetString(NusysConstants.ALIAS_ID_KEY), newId);
                aliasWithPropertiesMessage[NusysConstants.ALIAS_ID_KEY] = newId;
                aliasWithPropertiesMessage[NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY] = newWorkspaceId;

                var propertiesToAdd = new List<Message>();
                var aliasToAdd = new Message();
                foreach (var kvp in aliasWithPropertiesMessage)
                {
                    if (!NusysConstants.ALIAS_ACCEPTED_KEYS.Keys.Contains(kvp.Key))
                    {
                        //if the custom property is known to not be allowed, ignore it
                        if (NusysConstants.ILLEGAL_PROPERTIES_TABLE_KEY_NAMES.Contains(kvp.Key))
                        {
                            continue;
                        }
                        //if we reach here then the key has passed the bar of allowed to be a custom property
                        Message property = new Message();
                        property[NusysConstants.PROPERTIES_KEY_COLUMN_KEY] = kvp.Key;
                        property[NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY] = kvp.Value.ToString();
                        property[NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY] = newId;
                        propertiesToAdd.Add(property);
                    }
                    else
                    {
                        aliasToAdd.Add(kvp.Key, kvp.Value);
                    }
                }
                aliasesToInsert.Add(aliasToAdd);
                propertiesToInsert.AddRange(propertiesToAdd);
            }
            var insertAliasSuccess = true;
            var insertPropertiesSuccess = true;
            if (aliasesToInsert.Any())
            {
                SQLInsertQuery insertNewAliases = new SQLInsertQuery(Constants.SQLTableType.Alias, aliasesToInsert);
                 insertAliasSuccess = insertNewAliases.ExecuteCommand();
            }
            if (propertiesToInsert.Any())
            {

                SQLInsertQuery insertNewProperties = new SQLInsertQuery(Constants.SQLTableType.Properties, propertiesToInsert);
                insertPropertiesSuccess = insertNewProperties.ExecuteCommand();
            }
            return insertPropertiesSuccess && insertAliasSuccess;
        }

        /// <summary>
        /// This method should duplicate the collection library element giving the duplicate a new id. It should also
        /// duplicate all the properties and metadata that belong to the workspace. It will return a collectionlibraryelementmodel for the new collection if 
        /// the duplication was successful and will return null if it failed.
        /// </summary>
        /// <param name="oldWorkspaceId"></param>
        /// <param name="newWorkspaceId"></param>
        /// <returns></returns>
        private CollectionLibraryElementModel DuplicateCollectionLibraryElement(string oldWorkspaceId, string newWorkspaceId)
        {
            SqlJoinOperationArgs libraryElementJoinProperties = new SqlJoinOperationArgs();
            libraryElementJoinProperties.LeftTable = new SingleTable(Constants.SQLTableType.LibraryElement);
            libraryElementJoinProperties.RightTable = new SingleTable(Constants.SQLTableType.Properties);
            libraryElementJoinProperties.JoinOperator = Constants.JoinedType.LeftJoin;
            libraryElementJoinProperties.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
                NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).First();
            libraryElementJoinProperties.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Properties,
                NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First();
            JoinedTable propertiesJoinLibraryElement = new JoinedTable(libraryElementJoinProperties);

            SqlJoinOperationArgs libraryElementJoinPropertiesJoinMetadata = new SqlJoinOperationArgs();
            libraryElementJoinPropertiesJoinMetadata.LeftTable = propertiesJoinLibraryElement;
            libraryElementJoinPropertiesJoinMetadata.RightTable = new SingleTable(Constants.SQLTableType.Metadata);
            libraryElementJoinPropertiesJoinMetadata.JoinOperator = Constants.JoinedType.LeftJoin;
            libraryElementJoinPropertiesJoinMetadata.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
                NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).First();
            libraryElementJoinPropertiesJoinMetadata.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Metadata,
                NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY).First();
            JoinedTable propertiesJoinLibraryElementJoinMetadata = new JoinedTable(libraryElementJoinPropertiesJoinMetadata);

            SQLSelectQuery SelectCollectionInformationQuery = new SQLSelectQuery(propertiesJoinLibraryElementJoinMetadata, new SqlQueryEquals(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY, oldWorkspaceId));
            PropertiesAndMetadataParser parser = new PropertiesAndMetadataParser();
            var duplicateCollectionMessage = parser.ConcatPropertiesAndMetadata(new List<Message>(SelectCollectionInformationQuery.ExecuteCommand())).First();
            duplicateCollectionMessage[NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY] = newWorkspaceId;
            duplicateCollectionMessage[NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY] =
                DateTime.UtcNow.ToString();
            duplicateCollectionMessage[NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY] =
                DateTime.UtcNow.ToString();
            duplicateCollectionMessage[NusysConstants.LIBRARY_ELEMENT_TITLE_KEY] =
                duplicateCollectionMessage.GetString(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY) + " Snapshot";
            var newLibraryElementSuccess = ContentController.Instance.SqlConnector.AddLibraryElement(duplicateCollectionMessage);
            var metadataList = duplicateCollectionMessage.GetDict<string, MetadataEntry>(NusysConstants.LIBRARY_ELEMENT_METADATA_KEY).Values;
            var metadataMessagesList = new List<Message>();
            foreach (var metadataEntry in metadataList)
            {
                Message message = new Message();
                message[NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY] = newWorkspaceId;
                message[NusysConstants.METADATA_KEY_COLUMN_KEY] = metadataEntry.Key;
                message[NusysConstants.METADATA_MUTABILITY_COLUMN_KEY] = metadataEntry.Mutability.ToString();
                message[NusysConstants.METADATA_VALUE_COLUMN_KEY] = JsonConvert.SerializeObject(metadataEntry.Values);
                metadataMessagesList.Add(message);
            }
            if (metadataMessagesList.Any())
            {
                SQLInsertQuery insertQuery = new SQLInsertQuery(Constants.SQLTableType.Metadata, metadataMessagesList);
                var metadataSuccess = insertQuery.ExecuteCommand();
            }
            if (newLibraryElementSuccess)
            {
                CollectionLibraryElementModel model = new CollectionLibraryElementModel(newWorkspaceId);
                model.UnPackFromDatabaseKeys(duplicateCollectionMessage);
                return model;
            }
            return null;

        }
    }
}