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
            var commandString = "DELETE FROM " + Constants.GetTableName(tableType) + " WHERE ";
            int i = 0;
            foreach (var kvp in cleanedMessage)
            {
                if (i == 0)
                {
                    commandString = commandString + kvp.Key + " = '" + kvp.Value + "' ";
                }
                else
                {
                    commandString = commandString + deleteOperatorString + " " + kvp.Key + " = '" + kvp.Value + "' ";
                }
                i++;
            }
            CommandString = commandString + ";";
        }

        /// <summary>
        /// executes the delete command and returns true if a row was deleted;
        /// </summary>
        /// <returns></returns>
        public bool ExecuteCommand()
        {
            if (CommandString == null || CommandString.Equals(""))
            {
                throw new Exception("trying to execute delete query but the command string is empty");
            }
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}