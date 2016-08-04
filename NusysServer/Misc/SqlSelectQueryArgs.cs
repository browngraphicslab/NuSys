using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Microsoft;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// class to create a SQL select query with
    /// </summary>
    public class SqlSelectQueryArgs
    {
        /// <summary>
        /// the table you're selecting from
        /// </summary>
        public Constants.SQLTableType TableType { set; get; }

        /// <summary>
        /// the conditional that must be satisfied for the row to be picked
        /// </summary>
        public SqlSelectCondition Condition;

        /// <summary>
        /// the ienumerable of desired columns to fetch from the table
        /// </summary>
        public IEnumerable<string> ColumnsToGet { get; set; }
    }


    /// <summary>
    /// class for when you make a SQL select command.  This class should be returned.
    /// The columns will tell you exactly which columns and in what order they have been queried for on the database.
    /// The command itself is the select command to execute
    /// </summary>
    public class SelectCommandReturnArgs
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