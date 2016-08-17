using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;

namespace NusysServer
{
    public class SQLUpdateOrInsertPropertyQuery
    {
        public string CommandString { get; private set; }
        /// <summary>
        /// should only ever really be used for the properties table. This has the ability to either add or update multiple rows 
        /// in the properties table.
        /// </summary>
        /// <param name="tableToUpdate"></param>
        /// <param name="propertiesAndValuesToUpdate"></param>
        /// <param name="conditional"></param>
        public SQLUpdateOrInsertPropertyQuery(List<SQLUpdatePropertiesArgs> args)
        {
            CommandString = "";
            foreach (var propertyToAdd in args)
            {
                //Sets the value column as the column to update
                List<SqlQueryEquals> propertiesToUpdate = new List<SqlQueryEquals>()
                {
                    new SqlQueryEquals(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY, propertyToAdd.PropertyValue)
                };
                //TODO: use the update row and insert query generators instead of this hardcoded string!
                CommandString = CommandString +
                         "UPDATE "+Constants.GetTableName(Constants.SQLTableType.Properties)+" SET "+NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY+" = '"+propertyToAdd.PropertyValue+"' WHERE "+NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY+" = '"+propertyToAdd.LibraryOrAliasId+"' AND "+NusysConstants.PROPERTIES_KEY_COLUMN_KEY+" = '"+propertyToAdd.PropertyKey+ "' IF @@ROWCOUNT = 0 INSERT INTO " + Constants.GetTableName(Constants.SQLTableType.Properties) + " (" + NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY + ", " + NusysConstants.PROPERTIES_KEY_COLUMN_KEY + ", " + NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY + ") VALUES ('" + propertyToAdd.LibraryOrAliasId + "', '" + propertyToAdd.PropertyKey + "', '" + propertyToAdd.PropertyValue + "');";
            }
        }

        /// <summary>
        /// Executes the query command and returns a bool on whether the command altered any rows.
        /// </summary>
        /// <returns></returns>
        public bool ExecuteCommand()
        {
            if (CommandString == null || CommandString.Equals(""))
            {
                throw new Exception("trying to execute updateorinsertpropertyquery but the command string is empty");
            }
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}