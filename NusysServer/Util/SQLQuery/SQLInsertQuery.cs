﻿using System;
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
        /// creates an insert command for the table specified. The passed in column value messages are the key value pairs for the column name and the value you want to insert. This inserts a single row
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
        /// creates an insert command for the table specified. The passed in column value messages are the key value pairs for the column name and the value you want to insert. This inserts multiple rows. Each of the
        /// messages in columnValueMessages must have THE SAME KEY. 
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="columnValueMessage"></param>
        public SQLInsertQuery(Constants.SQLTableType tableType, List<Message> columnValueMessages)
        {
            var cleanedMessage = columnValueMessages.Select(q => Constants.GetCleanedMessageForDatabase(q, tableType));
            if (!cleanedMessage.First().Any())
            {
                throw new Exception("didn't find any valid " +
                                    " in requested sql insert command");
            }
            CommandString = "INSERT INTO " + Constants.GetTableName(tableType) + " ";
            var listOfColumns = cleanedMessage.First().GetKeys();
            var columnNames = "(" + string.Join(",", listOfColumns) + ")" ;
            var values = String.Join(",",
                cleanedMessage.Select(q => "(" + string.Join(",", q.GetValues().Select(w => "'" + w + "'")) + ")"));
            CommandString = CommandString + columnNames + " VALUES " + values + ";";
        }

        /// <summary>
        /// executes the insert command of this query. 
        /// Returns whether at least one row was populated
        /// This executes the insert command and returns whether it was successful or not
        /// </summary>
        /// <returns></returns>
        public bool ExecuteCommand()
        {
            if (CommandString == null || CommandString.Equals(""))
            {
                throw new Exception("trying to execute insert query but the command string is empty");
            }
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}