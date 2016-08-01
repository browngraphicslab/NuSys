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
            _db = new SqlConnection(SQLSTRING);
            _db.Open(); //open database
            var m = new Message();
            ResetTables(true);
            SetUpTables();
            m[NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY] = "test.jpg";
            m[NusysConstants.CONTENT_TABLE_TYPE_KEY] = NusysConstants.ContentType.Image;
            m[NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY] = "test_id";
            var s = AddContent(m);
        }

        /// <summary>
        /// will set up all four tables with the initial settings
        /// </summary>
        private void SetUpTables()
        {

            var contentTable = MakeCommand("CREATE TABLE " + GetTableName(Constants.SQLTableType.Content) + " (" +
                NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY + " varchar(128) NOT NULL PRIMARY KEY, " +
                NusysConstants.CONTENT_TABLE_TYPE_KEY + " varchar(128), " +
                NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY + " varchar(1024));");

            var libraryElementTable = MakeCommand("CREATE TABLE " + GetTableName(Constants.SQLTableType.LibrayElement) + " (" +
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
                NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY + " varchar(1024));");

            var aliasTable = MakeCommand("CREATE TABLE "+GetTableName(Constants.SQLTableType.Alias)+" ("+
                NusysConstants.ALIAS_ID_KEY + " varchar(128) NOT NULL PRIMARY KEY, " +
                NusysConstants.ALIAS_LIBRARY_ID_KEY + " varchar(128), " +
                NusysConstants.ALIAS_LOCATION_X_KEY + " float, " +
                NusysConstants.ALIAS_LOCATION_Y_KEY + " float, " +
                NusysConstants.ALIAS_SIZE_WIDTH_KEY + " float, " +
                NusysConstants.ALIAS_SIZE_HEIGHT_KEY + " float, " + 
                NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " varchar(128));");

            var metadataTable = MakeCommand("CREATE TABLE "+GetTableName(Constants.SQLTableType.Metadata)+" ("+
                NusysConstants.METADATA_LIBRARY_ELEMENT_ID_KEY + " varchar(128)," +
                NusysConstants.METADATA_KEY_COLUMN_KEY + " varchar(1028)," +
                NusysConstants.METADATA_VALUE_COLUMN_KEY + " varchar(4096));");

            var propertiesTable = MakeCommand("CREATE TABLE " + GetTableName(Constants.SQLTableType.Properties) + " (" +
                NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY + " varchar(128), " +
                NusysConstants.PROPERTIES_KEY_COLUMN_KEY + " varchar(1028), " +
                NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY + " datetime, " +
                NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY + " float, " +
                NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY + " varchar(4096));");

            libraryElementTable.ExecuteNonQuery();
            aliasTable.ExecuteNonQuery();
            metadataTable.ExecuteNonQuery();
            propertiesTable.ExecuteNonQuery();
            contentTable.ExecuteNonQuery();
        }

        /// <summary>
        /// be very carefull
        /// THIS RESETS THE ENTIRE SERVER
        /// DONT BE STUPID
        /// </summary>
        private void ResetTables(bool delete = false)
        {
            if (delete)
            {

                var dropAliases = MakeCommand("DROP TABLE " + GetTableName(Constants.SQLTableType.Alias));
                var dropLibraryElements = MakeCommand("DROP TABLE " + GetTableName(Constants.SQLTableType.LibrayElement));
                var dropProperties = MakeCommand("DROP TABLE " + GetTableName(Constants.SQLTableType.Properties));
                var dropMetadata = MakeCommand("DROP TABLE " + GetTableName(Constants.SQLTableType.Metadata));
                var dropContent = MakeCommand("DROP TABLE " + GetTableName(Constants.SQLTableType.Content));

                dropAliases.ExecuteNonQuery();
                dropLibraryElements.ExecuteNonQuery();
                dropProperties.ExecuteNonQuery();
                dropMetadata.ExecuteNonQuery();
                dropContent.ExecuteNonQuery();
            }
            else
            {
                var clearAliases = MakeCommand("TRUNCATE TABLE " + GetTableName(Constants.SQLTableType.Alias));
                var clearLibraryElements = MakeCommand("TRUNCATE TABLE " + GetTableName(Constants.SQLTableType.LibrayElement));
                var clearProperties = MakeCommand("TRUNCATE TABLE " + GetTableName(Constants.SQLTableType.Properties));
                var clearMetadata = MakeCommand("TRUNCATE TABLE " + GetTableName(Constants.SQLTableType.Metadata));
                var clearContent = MakeCommand("TRUNCATE TABLE " + GetTableName(Constants.SQLTableType.Content));

                clearAliases.ExecuteNonQuery();
                clearLibraryElements.ExecuteNonQuery();
                clearProperties.ExecuteNonQuery();
                clearMetadata.ExecuteNonQuery();
                clearContent.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// takes in a string and makes a command from our database using that command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private SqlCommand MakeCommand(string command)
        {
            var cmd = _db.CreateCommand();
            cmd.CommandText = command;
            return cmd;
        }

        /// <summary>
        /// method to return the 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTableName(Constants.SQLTableType type)
        {
            switch (type)
            {
                case Constants.SQLTableType.Alias:
                    return NusysIntermediate.NusysConstants.ALIASES_SQL_TABLE_NAME;
                case Constants.SQLTableType.LibrayElement:
                    return NusysIntermediate.NusysConstants.LIBRARY_ELEMENTS_SQL_TABLE_NAME;
                case Constants.SQLTableType.Metadata:
                    return NusysIntermediate.NusysConstants.METADATA_SQL_TABLE_NAME;
                case Constants.SQLTableType.Properties:
                    return NusysIntermediate.NusysConstants.PROPERTIES_SQL_TABLE_NAME;
                case Constants.SQLTableType.Content:
                    return NusysIntermediate.NusysConstants.CONTENTS_SQL_TABLE_NAME;
            }
            return null;
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
                    if (!AddStringProperty(libraryId, kvp.Key, kvp.Value.ToString()))
                    {
                        //TODO remove all the already-added properties
                        throw new Exception("The library element could not be added with key: " + kvp.Key +
                                            "  and value: " + kvp.Value);
                    }
                }
                else
                {
                    Type type = NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS[kvp.Key];
                    acceptedKeysDictionary.Add(kvp.Key, kvp.Value);
                }
            }
            var cmd = GetInsertCommand(Constants.SQLTableType.LibrayElement, acceptedKeysDictionary);
            var successInt = cmd.ExecuteNonQuery();
            return successInt > 0;
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

            var cmd = GetInsertCommand(Constants.SQLTableType.Content, message);
            var successInt = cmd.ExecuteNonQuery();
            return successInt > 0;
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

            var cmd = GetInsertCommand(Constants.SQLTableType.Alias, message);
            var successInt = cmd.ExecuteNonQuery();
            return successInt > 0;
        }

        /// <summary>
        /// To remove a single library element from the server, the passed in message should contain the LIBRARY_ELEMENT_LIBRARY_ID_KEY.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool DeleteLibraryElement(Message message)
        {
            if (!message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY))
            {
                return false;
            }
            var safeInsertMessage = new Message();
            safeInsertMessage[NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY] = message[NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY];
            var cmd = GetDeleteCommand(Constants.SQLTableType.LibrayElement, safeInsertMessage, Constants.Operator.And);
            var successInt = cmd.ExecuteNonQuery();
            return successInt > 0;
        }

        public IEnumerable<ElementModel> GetAliasesOfCollection(string collectionLibraryId)
        {
            return null;
        }

        /// <summary>
        /// Adds a string property to the properties table using the given key and library or alias Id
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="propertyKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool AddStringProperty(string objectId, string propertyKey, string value)
        {
            if (NusysConstants.ILLEGAL_PROPERTIES_TABLE_KEY_NAMES.Contains(propertyKey))
            {
                throw new Exception("Tried to add ilegal key to the properties table");
            }
            var cmd = GetInsertCommand(Constants.SQLTableType.Properties, new Message(new Dictionary<string, object>() {{propertyKey, value}}));
            var successInt = cmd.ExecuteNonQuery();
            return successInt > 0;
        }

        /// <summary>
        /// returns the contentDataModel, if any, of the specified contentDataModelId.  
        /// Will throw errors if there isn't one
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public ContentDataModel GetContentDataModel(string contentDataModelId)
        {
            //create new query args class
            var queryArgs = new SqlSelectQueryArgs();
            queryArgs.SelectProperties = new Message(new Dictionary<string, object>()
            {
                {NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY, contentDataModelId}
            });
            queryArgs.TableType = Constants.SQLTableType.Content;
            queryArgs.ColumnsToGet = Constants.GetAcceptedKeys(Constants.SQLTableType.Content);

            //get SQl Command from query args
            var returnArgs = GetSelectCommand(queryArgs);

            //execute query command
            using (var reader = returnArgs.Command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var m = new Message();
                        var i = 0;
                        foreach (var columnName in returnArgs.Columns)
                        {
                            m[columnName] = reader[i];
                            i++;
                        }
                        var dataModel = Constants.ParseContentDataModelFromDatabaseMessage(m);
                        return dataModel;
                    }
                }
            }
            throw new Exception("the requested contentDataModel wasn't found on the database");
        }

        /// <summary>
        /// Creates a SIMPLE select command for a specified table.  
        /// Returns a private class with the select command and a list of queried columns.  
        /// Will AND or OR the message key-value pairs together.  
        /// 
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="selectionParameterMessage"></param>
        /// <returns></returns>
        private SelectCommandReturnArgs GetSelectCommand(SqlSelectQueryArgs queryArgs)
        {
            var cleanedMessage = Constants.GetCleanedMessageForDatabase(queryArgs.SelectProperties, queryArgs.TableType);
            var cleanedColumnsToGet = new List<string>(queryArgs.ColumnsToGet.Intersect(Constants.GetAcceptedKeys(queryArgs.TableType)));
            var sqlStatement = "SELECT "+String.Join(",",cleanedColumnsToGet)+" FROM " + GetTableName(queryArgs.TableType) + " WHERE ";
            if (cleanedMessage.Any())
            {
                var first = cleanedMessage.First();
                sqlStatement += first.Key + "='" + first.Value + "' ";
                cleanedMessage.Remove(first.Key);
            }
            foreach (var kvp in cleanedMessage)
            {
                sqlStatement += queryArgs.GroupOperator.ToString()+" "+kvp.Key + "='" + kvp.Value + "' "; 
            }
            sqlStatement += ";";
            return new SelectCommandReturnArgs(MakeCommand(sqlStatement), cleanedColumnsToGet);
        }

        /// <summary>
        /// creates an insert command for the table specified
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="columnValueMessage"></param>
        /// <returns></returns>
        private SqlCommand GetInsertCommand (Constants.SQLTableType tableType, Message columnValueMessage)
        {
            var cleanedMessage = Constants.GetCleanedMessageForDatabase(columnValueMessage,tableType);
            var sqlStatement = "INSERT INTO " + GetTableName(tableType) + " ";
            var columnNames = "(";
            var values = "(";
            int i = 0;
            foreach (var kvp in cleanedMessage)
            {
                if (i == 0)
                {
                    columnNames = columnNames + kvp.Key;
                    values = values + "'"+kvp.Value+"'";
                }
                else
                {
                    columnNames = columnNames + ", " + kvp.Key;
                    values = values + ", " + "'" + kvp.Value + "'";
                }
                i++;
            }
            columnNames = columnNames + ")";
            values = values + ")";
            sqlStatement = sqlStatement + columnNames + " VALUES " + values + ";";
            return MakeCommand(sqlStatement);
        }
        
        /// <summary>
        /// This returns an sql delete command where either all or any (depending on the delete operator passed in) 
        /// of the key value pairs of columnValueMessage is is contained in the table.
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="columnValueMessage"></param>
        /// <param name="deleteOperator"></param>
        /// <returns></returns>
        private SqlCommand GetDeleteCommand(Constants.SQLTableType tableType, Message columnValueMessage, Constants.Operator deleteOperator )
        {
            var cleanedMessage = Constants.GetCleanedMessageForDatabase(columnValueMessage, tableType);
            var deleteOperatorString = deleteOperator.ToString();
            var deleteStringCmd = "DELETE FROM " + GetTableName(tableType) + " WHERE ";
            int i = 0;
            foreach (var kvp in cleanedMessage)
            {
                if (i == 0)
                {
                    deleteStringCmd = deleteStringCmd + kvp.Key + " = '" + kvp.Value + "' ";
                }
                else
                {
                    deleteStringCmd = deleteStringCmd + deleteOperatorString + kvp.Key + " = '" + kvp.Value + "' ";
                }
                i++;
            }
            deleteStringCmd = deleteStringCmd + ";";
            return MakeCommand(deleteStringCmd);
        }

        /// <summary>
        /// class for when you make a SQL select command.  This class should be returned.
        /// The columns will tell you exactly which columns and in what order they have been queried for on the database.
        /// The command itself is the select command to execute
        /// </summary>
        private class SelectCommandReturnArgs
        {
            /// <summary>
            /// the columns that the command actually queries from the database
            /// </summary>
            public IEnumerable<string> Columns { get; private set; }

            /// <summary>
            /// the command to execute to actually query the database
            /// </summary>
            public SqlCommand Command { get; private set; }


            /// <summary>
            /// simple constructor that takes in the command and the columns
            /// </summary>
            /// <param name="command"></param>
            /// <param name="columns"></param>
            public SelectCommandReturnArgs(SqlCommand command, IEnumerable<string> columns)
            {
                Columns = columns;
                Command = command;
            }
        }
    }

}