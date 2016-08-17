using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;

namespace NusysServer
{
    public class SQLConnector
    {
        /// <summary>
        /// this string is used to connect to the azure SQL database
        /// </summary>
        private const string SQLSTRING = "Server=tcp:nureposql.database.windows.net,1433;Database=NuRepo_SQL;User ID=nusys@nureposql;Password=browngfx1!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=True;";

        /// <summary>
        /// the private database we will use when making SQL commands for any of the four tables
        /// </summary>
        private SqlConnection _db;

        /// <summary>
        /// settings for json serialization
        /// </summary>
        private static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
        };

        /// <summary>
        /// consrtuctor just opens the sql database
        /// </summary>
        /// <param name="databaseString"></param>
        public SQLConnector(string databaseString = SQLSTRING)
        {
            _db = new SqlConnection(databaseString);
            _db.Open(); //open database

            ResetTables(true);
            SetUpTables();

            TestFunc();
        }

        public void TestFunc()
        {
        }

        /// <summary>
        /// will set up all four tables with the initial settings
        /// </summary>
        private void SetUpTables()
        {
            var usersTable = MakeCommand("CREATE TABLE " + Constants.GetTableName(Constants.SQLTableType.Users) + " (" +
                NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY + " varchar(128) NOT NULL PRIMARY KEY, " +
                NusysConstants.USERS_TABLE_HASHED_PASSWORD_KEY + " varchar(128), " +
                NusysConstants.USERS_TABLE_SALT_KEY + " varchar(128), " +
                NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY + " varchar(4096), " +
                NusysConstants.USERS_TABLE_LAST_TEN_COLLECTIONS_USED_KEY + " varchar(4096));");

            var analysisModelsTable = MakeCommand("CREATE TABLE " + Constants.GetTableName(Constants.SQLTableType.AnalysisModels) + " (" +
                NusysConstants.ANALYIS_MODELS_TABLE_CONTENT_ID_KEY + " varchar(128) NOT NULL PRIMARY KEY, " +
                NusysConstants.ANALYSIS_MODELS_TABLE_ANALYSIS_JSON_KEY + " varchar(MAX));");

            var presentationLinksTable = MakeCommand("CREATE TABLE " + Constants.GetTableName(Constants.SQLTableType.PresentationLink) + " (" +
                NusysConstants.PRESENTATION_LINKS_TABLE_LINK_ID_KEY + " varchar(128) NOT NULL PRIMARY KEY, " +
                NusysConstants.PRESENTATION_LINKS_TABLE_IN_ELEMENT_ID_KEY + " varchar(128), " +
                NusysConstants.PRESENTATION_LINKS_TABLE_OUT_ELEMENT_ID_KEY + " varchar(128), " +
                NusysConstants.PRESENTATION_LINKS_TABLE_PARENT_COLLECTION_LIBRARY_ID_KEY + " varchar(128), " +
                NusysConstants.PRESENTATION_LINKS_TABLE_ANNOTATION_TEXT_KEY + " varchar(4096));");

            var contentTable = MakeCommand("CREATE TABLE " + Constants.GetTableName(Constants.SQLTableType.Content) + " (" +
                NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY + " varchar(128) NOT NULL PRIMARY KEY, " +
                NusysConstants.CONTENT_TABLE_TYPE_KEY + " varchar(128), " +
                NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY + " varchar(MAX));");

            var libraryElementTable = MakeCommand("CREATE TABLE " + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + " (" +
                NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY + " varchar(128) NOT NULL PRIMARY KEY, " +
                NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY + " varchar(128), " +
                NusysConstants.LIBRARY_ELEMENT_TYPE_KEY + " varchar(128), " +
                NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY + " varchar(1024), " +
                NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY + " varchar(1024), " +
                NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY + " varchar(4096), " +
                NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY + " varchar(32), " +
                NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY + " varchar(4096), " +
                NusysConstants.LIBRARY_ELEMENT_TITLE_KEY + " varchar(4096), " +
                NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY + " varchar(1024), " +
                NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY + " varchar(1024), " +
                NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY + " varchar(32), " +
                NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY + " varchar(1024));");

            var aliasTable = MakeCommand("CREATE TABLE "+ Constants.GetTableName(Constants.SQLTableType.Alias)+" ("+
                NusysConstants.ALIAS_ID_KEY + " varchar(128) NOT NULL PRIMARY KEY, " +
                NusysConstants.ALIAS_LIBRARY_ID_KEY + " varchar(128), " +
                NusysConstants.ALIAS_LOCATION_X_KEY + " float, " +
                NusysConstants.ALIAS_LOCATION_Y_KEY + " float, " +
                NusysConstants.ALIAS_SIZE_WIDTH_KEY + " float, " +
                NusysConstants.ALIAS_SIZE_HEIGHT_KEY + " float, " +
                NusysConstants.ALIAS_CREATOR_ID_KEY + " varchar(128), " +
                 NusysConstants.ALIAS_ACCESS_KEY + " varchar(128), " +
                NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " varchar(128));");

            var metadataTable = MakeCommand("CREATE TABLE "+ Constants.GetTableName(Constants.SQLTableType.Metadata)+" ("+
                NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY + " varchar(128)," +
                NusysConstants.METADATA_KEY_COLUMN_KEY + " varchar(1028)," +
                NusysConstants.METADATA_MUTABILITY_COLUMN_KEY + " varchar(256)," +
                NusysConstants.METADATA_VALUE_COLUMN_KEY + " varchar(4096));");

            var propertiesTable = MakeCommand("CREATE TABLE " + Constants.GetTableName(Constants.SQLTableType.Properties) + " (" +
                NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY + " varchar(128), " +
                NusysConstants.PROPERTIES_KEY_COLUMN_KEY + " varchar(1028), " +
                NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY + " datetime, " +
                NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY + " float, " +
                NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY + " varchar(4096));");

            usersTable.ExecuteNonQuery();
            analysisModelsTable.ExecuteNonQuery();
            presentationLinksTable.ExecuteNonQuery();
            libraryElementTable.ExecuteNonQuery();
            aliasTable.ExecuteNonQuery();
            metadataTable.ExecuteNonQuery();
            propertiesTable.ExecuteNonQuery();
            contentTable.ExecuteNonQuery();
        }

        /// <summary>
        /// Delete boolean for reseting via deleting the tables
        /// be very careful.  
        /// THIS RESETS THE ENTIRE SERVER
        /// DONT BE STUPID
        /// </summary>
        private void ResetTables(bool delete = false)
        {
            if (delete)
            {
                var dropPresentationLinks = MakeCommand("IF OBJECT_ID('dbo."+ Constants.GetTableName(Constants.SQLTableType.PresentationLink) + "', 'U') IS NOT NULL DROP TABLE " + Constants.GetTableName(Constants.SQLTableType.PresentationLink));
                var dropAliases = MakeCommand("IF OBJECT_ID('dbo." + Constants.GetTableName(Constants.SQLTableType.Alias) + "', 'U') IS NOT NULL DROP TABLE " + Constants.GetTableName(Constants.SQLTableType.Alias));
                var dropLibraryElements = MakeCommand("IF OBJECT_ID('dbo." + Constants.GetTableName(Constants.SQLTableType.LibraryElement) + "', 'U') IS NOT NULL DROP TABLE " + Constants.GetTableName(Constants.SQLTableType.LibraryElement));
                var dropProperties = MakeCommand("IF OBJECT_ID('dbo." + Constants.GetTableName(Constants.SQLTableType.Properties) + "', 'U') IS NOT NULL DROP TABLE " + Constants.GetTableName(Constants.SQLTableType.Properties));
                var dropMetadata = MakeCommand("IF OBJECT_ID('dbo." + Constants.GetTableName(Constants.SQLTableType.Metadata) + "', 'U') IS NOT NULL DROP TABLE " + Constants.GetTableName(Constants.SQLTableType.Metadata));
                var dropContent = MakeCommand("IF OBJECT_ID('dbo." + Constants.GetTableName(Constants.SQLTableType.Content) + "', 'U') IS NOT NULL DROP TABLE " + Constants.GetTableName(Constants.SQLTableType.Content));
                var dropAnalysisModels = MakeCommand("IF OBJECT_ID('dbo." + Constants.GetTableName(Constants.SQLTableType.AnalysisModels) + "', 'U') IS NOT NULL DROP TABLE " + Constants.GetTableName(Constants.SQLTableType.AnalysisModels));
                var dropUsers = MakeCommand("IF OBJECT_ID('dbo." + Constants.GetTableName(Constants.SQLTableType.Users) + "', 'U') IS NOT NULL DROP TABLE " + Constants.GetTableName(Constants.SQLTableType.Users));

                dropPresentationLinks.ExecuteNonQuery();
                dropAliases.ExecuteNonQuery();
                dropLibraryElements.ExecuteNonQuery();
                dropProperties.ExecuteNonQuery();
                dropMetadata.ExecuteNonQuery();
                dropContent.ExecuteNonQuery();
                dropAnalysisModels.ExecuteNonQuery();
                dropUsers.ExecuteNonQuery();
            }
            else
            {
                var clearPresentationLinks = MakeCommand("TRUNCATE TABLE " + Constants.GetTableName(Constants.SQLTableType.PresentationLink));
                var clearAliases = MakeCommand("TRUNCATE TABLE " + Constants.GetTableName(Constants.SQLTableType.Alias));
                var clearLibraryElements = MakeCommand("TRUNCATE TABLE " + Constants.GetTableName(Constants.SQLTableType.LibraryElement));
                var clearProperties = MakeCommand("TRUNCATE TABLE " + Constants.GetTableName(Constants.SQLTableType.Properties));
                var clearMetadata = MakeCommand("TRUNCATE TABLE " + Constants.GetTableName(Constants.SQLTableType.Metadata));
                var clearContent = MakeCommand("TRUNCATE TABLE " + Constants.GetTableName(Constants.SQLTableType.Content));
                var clearAnalysisModels = MakeCommand("TRUNCATE TABLE " + Constants.GetTableName(Constants.SQLTableType.AnalysisModels));
                var clearUsers = MakeCommand("TRUNCATE TABLE " + Constants.GetTableName(Constants.SQLTableType.Users));

                clearPresentationLinks.ExecuteNonQuery();
                clearAliases.ExecuteNonQuery();
                clearLibraryElements.ExecuteNonQuery();
                clearProperties.ExecuteNonQuery();
                clearMetadata.ExecuteNonQuery();
                clearContent.ExecuteNonQuery();
                clearAnalysisModels.ExecuteNonQuery();
                clearUsers.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// takes in a string and makes a command from our database using that command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public SqlCommand MakeCommand(string command)
        {
            var cmd = _db.CreateCommand();
            cmd.CommandText = command;
            return cmd;
        }

        /// <summary>
        /// To add a library element to the server.  Returns true if successful, false otherwise
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool AddLibraryElement(Message message)
        {
            if (!message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY))
            {
                throw new Exception("cannot add library element to database without a library element Id");
            } 
            var libraryId = message.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY);

            //a dictionary to keep track of the present keys
            //that we can add directly into the library element database
            var acceptedKeysDictionary = new Message();
            List<SQLUpdatePropertiesArgs> propertiesToAdd = new List<SQLUpdatePropertiesArgs>();

            foreach (var kvp in message)
            {
                if (!NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS.Keys.Contains(kvp.Key))
                {
                    //if the custom property is known to not be allowed, ignore it
                    if (NusysConstants.ILLEGAL_PROPERTIES_TABLE_KEY_NAMES.Contains(kvp.Key))
                    {
                        continue;
                    }
                    //if we reach here then the key has passed the bar of allowed to be a custom property
                    SQLUpdatePropertiesArgs property = new SQLUpdatePropertiesArgs();
                    property.PropertyKey = kvp.Key;
                    property.PropertyValue = kvp.Value.ToString();
                    property.LibraryOrAliasId = libraryId;
                    propertiesToAdd.Add(property);
                }
                else
                {
                    Type type = NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS[kvp.Key];
                    acceptedKeysDictionary.Add(kvp.Key, kvp.Value);
                }
            }
            if (propertiesToAdd.Any())
            {
                SQLUpdateOrInsertPropertyQuery updateOrInsertPropertiesQuery =
                        new SQLUpdateOrInsertPropertyQuery(propertiesToAdd);
                if (!updateOrInsertPropertiesQuery.ExecuteCommand())
                {
                    throw new Exception("Could not update or insert the properties from the sql query" + updateOrInsertPropertiesQuery.CommandString);
                }
            }
            var cmd = new SQLInsertQuery(Constants.SQLTableType.LibraryElement, acceptedKeysDictionary);
            return cmd.ExecuteCommand();

        }

        /// <summary>
        /// To add a content to the server.  Returns true if successful, false otherwise
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool AddContent(Message message)
        {
            if (!message.ContainsKey(NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY))
            {
                return false;
            }

            var cmd = new SQLInsertQuery(Constants.SQLTableType.Content, message);
            return cmd.ExecuteCommand();
        }

        

        /// <summary>
        /// To add an alias to the database.  Returns true if successful, false otherwise
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool AddAlias(Message message)
        {
            if (!message.ContainsKey(NusysConstants.ALIAS_ID_KEY))
            {
                return false;
            }

            var cmd = new SQLInsertQuery(Constants.SQLTableType.Alias, message);
            return cmd.ExecuteCommand();
        }

        /// <summary>
        /// To remove a single library element from the server, the passed in message should contain the LIBRARY_ELEMENT_LIBRARY_ID_KEY.
        /// This also takes care of deleting all the related metadata from the metadata table.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool DeleteLibraryElement(Message message)
        {
            if (!message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY))
            {
                return false;
            }
            var cmdToDeleteFromLibraryElementTable = new SQLDeleteQuery(Constants.SQLTableType.LibraryElement, message, Constants.Operator.And);

            //Since column names for the Metadata table differ, we need to create a new message which contains the library id.
            var metadataMessage = new Message();
            metadataMessage[NusysConstants.METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY] = message.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY);

            // The command below deletes all the related metadata from the metadata table.
            var cmdToDeleteRelatedMetadata = new SQLDeleteQuery(Constants.SQLTableType.Metadata, metadataMessage, Constants.Operator.And);
            return cmdToDeleteFromLibraryElementTable.ExecuteCommand() && cmdToDeleteRelatedMetadata.ExecuteCommand();
        }

        /// <summary>
        /// To remove a single alias from the server, the passed in message should contain the ALIAS_ID_KEY.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool DeleteAlias(Message message)
        {
            if (!message.ContainsKey(NusysConstants.ALIAS_ID_KEY))
            {
                return false;
            }
            var cmd = new SQLDeleteQuery(Constants.SQLTableType.Alias, message, Constants.Operator.And);
            return cmd.ExecuteCommand();
        }

        /// <summary>
        /// This removes the specified metadata entry from the library element whose id is passed in. Needs to have the key of the entry.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool DeleteMetadataEntry(Message message)
        {
            
            if (!message.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY) || !message.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY))
            {
                return false;
            }
            var cmd = new SQLDeleteQuery(Constants.SQLTableType.Metadata, message, Constants.Operator.And);
            return cmd.ExecuteCommand();
        }

        /// <summary>
        /// method to add a new analysisModelJson to the table. 
        ///  It takes in the id of the model and the josn for the analysis model and adds it to the table.
        /// It will return true if the json was added succesfully.
        /// </summary>
        /// <param name="analysisModelContentDataModelId"></param>
        /// <param name="analysisModelJson"></param>
        /// <returns></returns>
        public bool AddAnalysisModel(string analysisModelContentDataModelId, string analysisModelJson)
        {
            //make sure they arent null or empty strings
            if (string.IsNullOrEmpty(analysisModelContentDataModelId) || string.IsNullOrEmpty(analysisModelJson))
            {
                throw new Exception("tried to insert invalid json or content Data model Ids into the AnalysisModels table");
            }

            //create a message with the json and id for table insertion
            var insertMessage = new Message()
            {
                {NusysConstants.ANALYIS_MODELS_TABLE_CONTENT_ID_KEY, analysisModelContentDataModelId },
                {NusysConstants.ANALYSIS_MODELS_TABLE_ANALYSIS_JSON_KEY, analysisModelJson }
            };

            //create the insert command
            var insertCmd = new SQLInsertQuery(Constants.SQLTableType.AnalysisModels, insertMessage);

            return insertCmd.ExecuteCommand();
        }


        /// <summary>
        /// returns the contentDataModel, if any, of the specified contentDataModelId.  
        /// Will throw errors if there isn't one
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public ContentDataModel GetContentDataModel(string contentDataModelId)
        {
            //get SQl Command from query args
            var sqlQuery = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.Content), 
                new SqlQueryEquals(Constants.SQLTableType.Content, NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY,contentDataModelId));

            //execute query command
            var executeMessages = sqlQuery.ExecuteCommand();
            if (executeMessages.Any())
            {
                var strippedMessage = Constants.StripTableNames(executeMessages.FirstOrDefault());
                strippedMessage[NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY] = FileHelper.GetDataFromContentURL(
                        strippedMessage.GetString(NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY),
                        strippedMessage.GetEnum<NusysConstants.ContentType>(NusysConstants.CONTENT_TABLE_TYPE_KEY));
                var dataModel = ContentDataModelFactory.CreateFromMessage(strippedMessage);//TODO factor out to contentDataModel facotry
                return dataModel;
            }
            throw new Exception("the requested contentDataModel wasn't found on the database");
        }

        /// <summary>
        /// executes a select query and returns the selected objects as messages
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerable<Message> ExecuteSelectQueryAsMessages(SqlCommand command, bool includeNulls = true)
        {
            var messages = new List<Message>();
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var m = new Message();

                        for (var i= 0; i < reader.FieldCount; i++)
                        {
                            if (reader[i] != null || includeNulls)
                            {
                                m[reader.GetName(i)] = reader[i];
                                
                            }
                        }

                        //var i = 0;
                        //foreach (var columnName in args.Columns)
                        //{
                        //    var x = reader[i];
                        //    if (reader[i] != null|| includeNulls)
                        //    {
                        //        m[columnName] = reader[i];
                        //        i++;
                        //    }
                        //}
                        messages.Add(m);
                    }
                }
            }
            return messages;
        }

        ///// <summary>
        ///// creates an insert command for the table specified
        ///// </summary>
        ///// <param name="tableType"></param>
        ///// <param name="columnValueMessage"></param>
        ///// <returns></returns>
        //private SqlCommand GetInsertCommand (Constants.SQLTableType tableType, Message columnValueMessage)
        //{
        //    var cleanedMessage = Constants.GetCleanedMessageForDatabase(columnValueMessage,tableType);
        //    if (!cleanedMessage.Any())
        //    {
        //        throw new Exception("didn't find any valid keys in requested sql insert command");
        //    }
        //    var sqlStatement = "INSERT INTO " + Constants.GetTableName(tableType) + " ";
        //    var columnNames = "(";
        //    var values = "(";
        //    int i = 0;
        //    foreach (var kvp in cleanedMessage)
        //    {
        //        if (i == 0)
        //        {
        //            columnNames = columnNames + kvp.Key;
        //            values = values + "'"+kvp.Value+"'";
        //        }
        //        else
        //        {
        //            columnNames = columnNames + ", " + kvp.Key;
        //            values = values + ", " + "'" + kvp.Value + "'";
        //        }
        //        i++;
        //    }
        //    columnNames = columnNames + ")";
        //    values = values + ")";
        //    sqlStatement = sqlStatement + columnNames + " VALUES " + values + ";";
        //    return MakeCommand(sqlStatement);
        //}
        
        ///// <summary>
        ///// This returns an sql delete command where either all or any (depending on the delete operator passed in) 
        ///// of the key value pairs of columnValueMessage is is contained in the table.
        ///// </summary>
        ///// <param name="tableType"></param>
        ///// <param name="columnValueMessage"></param>
        ///// <param name="deleteOperator"></param>
        ///// <returns></returns>
        //private SqlCommand GetDeleteCommand(Constants.SQLTableType tableType, Message columnValueMessage, Constants.Operator deleteOperator )
        //{
        //    var cleanedMessage = Constants.GetCleanedMessageForDatabase(columnValueMessage, tableType);
        //    var deleteOperatorString = deleteOperator.ToString();
        //    var deleteStringCmd = "DELETE FROM " + Constants.GetTableName(tableType) + " WHERE ";
        //    int i = 0;
        //    foreach (var kvp in cleanedMessage)
        //    {
        //        if (i == 0)
        //        {
        //            deleteStringCmd = deleteStringCmd + kvp.Key + " = '" + kvp.Value + "' ";
        //        }
        //        else
        //        {
        //            deleteStringCmd = deleteStringCmd + deleteOperatorString + kvp.Key + " = '" + kvp.Value + "' ";
        //        }
        //        i++;
        //    }
        //    deleteStringCmd = deleteStringCmd + ";";
        //    return MakeCommand(deleteStringCmd);
        //}
    }

}