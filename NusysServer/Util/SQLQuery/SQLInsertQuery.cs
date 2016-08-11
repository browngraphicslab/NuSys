using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class SQLInsertQuery
    {
        public string CommandString{get; private set;}
        /// <summary>
        /// creates an insert command for the table specified
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="columnValueMessage"></param>
        /// <returns></returns>
        public SQLInsertQuery(Constants.SQLTableType tableType, Message columnValueMessage)
        {
            var cleanedMessage = Constants.GetCleanedMessageForDatabase(columnValueMessage, tableType);
            if (!cleanedMessage.Any())
            {
                throw new Exception("didn't find any valid keys in requested sql insert command");
            }
            CommandString = "INSERT INTO " + Constants.GetTableName(tableType) + " ";
            var columnNames = "(";
            var values = "(";
            int i = 0;
            foreach (var kvp in cleanedMessage)
            {
                if (i == 0)
                {
                    columnNames = columnNames + kvp.Key;
                    values = values + "'" + kvp.Value + "'";
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
            CommandString = CommandString + columnNames + " VALUES " + values + ";";
        }

        public bool ExecuteCommand()
        {
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}