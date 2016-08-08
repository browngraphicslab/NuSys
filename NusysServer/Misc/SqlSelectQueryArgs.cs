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