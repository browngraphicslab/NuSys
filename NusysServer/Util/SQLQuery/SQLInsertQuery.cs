using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class SQLInsertQuery
    {
        /// <summary>
        /// The string used to create a sql command
        /// </summary>
        public string CommandString{get; private set;}
        /// <summary>
        /// creates an insert command for the table specified. The passed in column value messages are the key value pairs for the column name and the value you want to insert.
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

        /// <summary>
        /// This executes the insert command and returns whether it was successful or not
        /// </summary>
        /// <returns></returns>
        public bool ExecuteCommand()
        {
            if (CommandString == null || CommandString.Equals(""))
            {
                throw new Exception("trying to execute insertquery but the command string is empty");
            }
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}