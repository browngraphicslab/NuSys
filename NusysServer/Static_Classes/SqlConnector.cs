using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace NuRepo
{
    public class SQLConnector
    {
        public enum SQLTableType
        {
            Alias,
            LibrayElement,
            Metadata,
            Properties
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
            var libraryElementTable = MakeCommand("CREATE TABLE " + GetTableName(SQLTableType.LibrayElement) + " (" +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_TYPE_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY + " boolean, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY + " text, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_TITLE_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY + " varchar);");

            var aliasTable = MakeCommand("CREATE TABLE "+GetTableName(SQLTableType.Alias)+" ("+
                NusysIntermediate.NusysConstants.ALIAS_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.ALIAS_LIBRARY_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.ALIAS_LOCATION_X_KEY + " double, " +
                NusysIntermediate.NusysConstants.ALIAS_LOCATION_Y_KEY + " double, " +
                NusysIntermediate.NusysConstants.ALIAS_SIZE_WIDTH_KEY + " double, " +
                NusysIntermediate.NusysConstants.ALIAS_SIZE_HEIGHT_KEY + " double, " +
                NusysIntermediate.NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY + " char(32));");

            var metadataTable = MakeCommand("CREATE TABLE "+GetTableName(SQLTableType.Metadata)+" ("+
                NusysIntermediate.NusysConstants.METADATA_LIBRARY_ELEMENT_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.METADATA_KEY_COLUMN_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.METADATA_VALUE_COLUMN_KEY + " varchar);");

            var propertiesTable = MakeCommand("CREATE TABLE " + GetTableName(SQLTableType.Properties) + " (" +
                NusysIntermediate.NusysConstants.PROPERTIES_LIBRARY_ID_KEY + " char(32), " +
                NusysIntermediate.NusysConstants.PROPERTIES_KEY_COLUMN_KEY + " varchar, " +
                NusysIntermediate.NusysConstants.PROPERTIES_DATE_VALUE_COLUMN_KEY + " datetime, " +
                NusysIntermediate.NusysConstants.PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY + " double, " +
                NusysIntermediate.NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY + " varchar);");

            libraryElementTable.ExecuteNonQuery();
            aliasTable.ExecuteNonQuery();
            metadataTable.ExecuteNonQuery();
            propertiesTable.ExecuteNonQuery();
        }

        /// <summary>
        /// be very carefull
        /// THIS RESETS THE ENTIRE SERVER
        /// DONT BE STUPID
        /// </summary>
        private void ResetTables()
        {
            var clearAliases = MakeCommand("TRUNCATE TABLE "+GetTableName(SQLTableType.Alias));
            var clearLibraryElements = MakeCommand("TRUNCATE TABLE " + GetTableName(SQLTableType.LibrayElement));
            var clearProperties = MakeCommand("TRUNCATE TABLE " + GetTableName(SQLTableType.Properties));
            var clearMetadata = MakeCommand("TRUNCATE TABLE " + GetTableName(SQLTableType.Metadata));

            clearAliases.ExecuteNonQuery();
            clearLibraryElements.ExecuteNonQuery();
            clearProperties.ExecuteNonQuery();
            clearMetadata.ExecuteNonQuery();
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
            }
            return null;
        }
    }
}