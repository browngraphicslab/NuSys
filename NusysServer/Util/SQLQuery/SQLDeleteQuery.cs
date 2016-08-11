using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer.Util.SQLQuery
{
    public class SQLDeleteQuery
    {
        public string CommandString { get; private set; }

        /// <summary>
        /// This returns an sql delete command where either all or any (depending on the delete operator passed in) 
        /// of the key value pairs of columnValueMessage is is contained in the table.
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="columnValueMessage"></param>
        /// <param name="deleteOperator"></param>
        /// <returns></returns>
        public SQLDeleteQuery(Constants.SQLTableType tableType, Message columnValueMessage, Constants.Operator deleteOperator)
        {
            var cleanedMessage = Constants.GetCleanedMessageForDatabase(columnValueMessage, tableType);
            var deleteOperatorString = deleteOperator.ToString();
            var CommandString = "DELETE FROM " + Constants.GetTableName(tableType) + " WHERE ";
            int i = 0;
            foreach (var kvp in cleanedMessage)
            {
                if (i == 0)
                {
                    CommandString = CommandString + kvp.Key + " = '" + kvp.Value + "' ";
                }
                else
                {
                    CommandString = CommandString + deleteOperatorString + kvp.Key + " = '" + kvp.Value + "' ";
                }
                i++;
            }
            CommandString = CommandString + ";";
        }
        public bool ExecuteCommand()
        {
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}