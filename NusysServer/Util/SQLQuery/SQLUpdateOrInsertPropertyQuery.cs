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
            //TODO: use the update row and insert query generators
            CommandString = "";
            foreach (var propertyToAdd in args)
            {
                //Sets the value column as the column to update
                List<SqlQueryEquals> propertiesToUpdate = new List<SqlQueryEquals>()
                {
                    new SqlQueryEquals(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY, propertyToAdd.PropertyValue)
                };
                //Sets conditional as where the libraryoraliasid is equal to the 
                //SqlQueryConditional conditionalID = new SqlQueryEquals(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY, propertyToAdd.LibraryOrAliasId);
                //SqlQueryConditional conditionalKey = new SqlQueryEquals(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_KEY_COLUMN_KEY, propertyToAdd.PropertyKey);
                //var condtionalIdAndKey = new SqlQueryOperator(conditionalID, conditionalKey, Constants.Operator.And);
                //SQLUpdateRowQuery updateQuery = new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.Properties), propertiesToUpdate, condtionalIdAndKey);
                //var insertQuery = new SQLInsertQuery(Constants.SQLTableType.Properties, );
                ////TODO: use the update row and insert query generators!!!!! FINISH THIS!!!!

                CommandString = CommandString +
                         "UPDATE "+Constants.GetTableName(Constants.SQLTableType.Properties)+" SET "+NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY+" = '"+propertyToAdd.PropertyValue+"' WHERE "+NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY+" = '"+propertyToAdd.LibraryOrAliasId+"' AND "+NusysConstants.PROPERTIES_KEY_COLUMN_KEY+" = '"+propertyToAdd.PropertyKey+ "' IF @@ROWCOUNT = 0 INSERT INTO " + Constants.GetTableName(Constants.SQLTableType.Properties) + " (" + NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY + ", " + NusysConstants.PROPERTIES_KEY_COLUMN_KEY + ", " + NusysConstants.PROPERTIES_STRING_VALUE_COLUMN_KEY + ") VALUES ('" + propertyToAdd.LibraryOrAliasId + "', '" + propertyToAdd.PropertyKey + "', '" + propertyToAdd.PropertyValue + "');";
            }
        }

        /// <summary>
        /// Executes the query command and returns a bool on whether the command altered any rows
        /// </summary>
        /// <returns></returns>
        public bool ExecuteCommand()
        {
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}