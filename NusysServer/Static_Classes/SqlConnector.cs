using System;
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
        public enum SQLTableType
        {
            Alias,
            LibrayElement,
            Metadata,
            Properties,
            Content
        }

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
        }

        /// <summary>
        /// will set up all four tables with the initial settings
        /// </summary>
        private void SetUpTables()
        {

            var contentTable = MakeCommand("CREATE TABLE " + GetTableName(SQLTableType.Content) + " (" +
                NusysIntermediate.NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.CONTENT_TABLE_TYPE_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.CONTENT_TABLE_CONTENT_URL + " varchar);");

            var libraryElementTable = MakeCommand("CREATE TABLE " + GetTableName(SQLTableType.LibrayElement) + " (" +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_TYPE_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY + " char(5), " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_TITLE_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY + " varchar);");

            var aliasTable = MakeCommand("CREATE TABLE "+GetTableName(SQLTableType.Alias)+" ("+
                NusysIntermediate.NusysConstants.ALIAS_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.ALIAS_LIBRARY_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.ALIAS_LOCATION_X_KEY + " float, " +
                NusysIntermediate.NusysConstants.ALIAS_LOCATION_Y_KEY + " float, " +
                NusysIntermediate.NusysConstants.ALIAS_SIZE_WIDTH_KEY + " float, " +
                NusysIntermediate.NusysConstants.ALIAS_SIZE_HEIGHT_KEY + " float, " + 
                NusysIntermediate.NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " char(32));");

            var metadataTable = MakeCommand("CREATE TABLE "+GetTableName(SQLTableType.Metadata)+" ("+
                NusysIntermediate.NusysConstants.METADATA_LIBRARY_ELEMENT_ID_KEY + " char(32)," +
                NusysIntermediate.NusysConstants.METADATA_KEY_COLUMN_KEY + " varchar," +
                NusysIntermediate.NusysConstants.METADATA_VALUE_COLUMN_KEY + " varchar,);");

            var propertiesTable = MakeCommand("CREATE TABLE " + GetTableName(SQLTableType.Properties) + " (" +
                NusysIntermediate.NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.PROPERTIES_KEY_COLUMN_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY + " datetime, " +
                NusysIntermediate.NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY + " float, " +
                NusysIntermediate.NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY + " varchar);");

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

                var dropAliases = MakeCommand("DROP TABLE " + GetTableName(SQLTableType.Alias));
                var dropLibraryElements = MakeCommand("DROP TABLE " + GetTableName(SQLTableType.LibrayElement));
                var dropProperties = MakeCommand("DROP TABLE " + GetTableName(SQLTableType.Properties));
                var dropMetadata = MakeCommand("DROP TABLE " + GetTableName(SQLTableType.Metadata));
                var dropContent = MakeCommand("DROP TABLE " + GetTableName(SQLTableType.Content));

                dropAliases.ExecuteNonQuery();
                dropLibraryElements.ExecuteNonQuery();
                dropProperties.ExecuteNonQuery();
                dropMetadata.ExecuteNonQuery();
                dropContent.ExecuteNonQuery();
            }
            else
            {
                var clearAliases = MakeCommand("TRUNCATE TABLE " + GetTableName(SQLTableType.Alias));
                var clearLibraryElements = MakeCommand("TRUNCATE TABLE " + GetTableName(SQLTableType.LibrayElement));
                var clearProperties = MakeCommand("TRUNCATE TABLE " + GetTableName(SQLTableType.Properties));
                var clearMetadata = MakeCommand("TRUNCATE TABLE " + GetTableName(SQLTableType.Metadata));
                var clearContent = MakeCommand("TRUNCATE TABLE " + GetTableName(SQLTableType.Content));

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
        private static string GetTableName(SQLTableType type)
        {
            switch (type)
            {
                case SQLTableType.Alias:
                    return NusysIntermediate.NusysConstants.ALIASES_SQL_TABLE_NAME;
                case SQLTableType.LibrayElement:
                    return NusysIntermediate.NusysConstants.LIBRARY_ELEMENTS_SQL_TABLE_NAME;
                case SQLTableType.Metadata:
                    return NusysIntermediate.NusysConstants.METADATA_SQL_TABLE_NAME;
                case SQLTableType.Properties:
                    return NusysIntermediate.NusysConstants.PROPERTIES_SQL_TABLE_NAME;
                case SQLTableType.Content:
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
                return false;
            } 
            var libraryId = message.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY);

            //a dictionary to keep track of the present keys
            //that we can add directly into the library element database
            var acceptedKeysDictionary = new Message();
            foreach (var kvp in message)
            {
                if (!NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS.Keys.Contains(kvp.Key))
                {
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
            var cmd = GetInsertCommand(SQLTableType.LibrayElement, acceptedKeysDictionary);
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

            //makes sure that the message used to get the insert sql command only contains keys that exist in the 
            //content sql table. This is to make sure that there are no errors when executing the command.
            var safeInsertMessage = new Message();
            foreach (var acceptedKey in NusysConstants.ACCEPTED_CONTENT_TABLE_KEYS)
            {
                if (message.ContainsKey(acceptedKey))
                {
                    safeInsertMessage[acceptedKey] = message[acceptedKey];
                }
            }

            var cmd = GetInsertCommand(SQLTableType.Content, safeInsertMessage);
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
            safeInsertMessage[NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY] =
                message[NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY];
            var cmd = GetDeleteCommand(SQLTableType.LibrayElement, safeInsertMessage, NusysConstants.Operator.And);
            var successInt = cmd.ExecuteNonQuery();
            return successInt > 0;
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
            var cmd = GetInsertCommand(SQLTableType.Properties, new Message(new Dictionary<string, object>() {{propertyKey, value}}));
            var successInt = cmd.ExecuteNonQuery();
            return successInt > 0;
        }



        /// <summary>
        /// creates an insert command for the table t
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="columnValueMessage"></param>
        /// <returns></returns>
        private SqlCommand GetInsertCommand (SQLTableType tableType, Message columnValueMessage)
        {
            var cleanedMessage = NusysConstants.CleanForDatabaseInsertion(columnValueMessage);
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
        private SqlCommand GetDeleteCommand(SQLTableType tableType, Message columnValueMessage, NusysConstants.Operator deleteOperator )
        {
            var deleteOperatorString = deleteOperator.ToString();
            var deleteStringCmd = "DELETE FROM " + GetTableName(tableType) + " WHERE ";
            int i = 0;
            foreach (var kvp in columnValueMessage)
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

    }
}