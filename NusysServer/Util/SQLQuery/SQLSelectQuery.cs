using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;

namespace NusysServer
{
    public class SQLSelectQuery
    {
        //private IEnumerable<string> _cleanedSelectedColumns;
        private ITableRepresentable _fromTable;
        private SqlQueryConditional _conditionals;
        public string CommandString { get; private set; }

        /// <summary>
        /// Creates a new select query based on parameters.
        /// </summary>
        /// <param name="selectedColumns"> The columns you wish to recieve</param>
        /// <param name="fromTable">The tables from which you want to select </param>
        /// <param name="conditionals">Optional. Checks for conditional</param>
        public SQLSelectQuery(ITableRepresentable fromTable, SqlQueryConditional conditionals = null)
        {
            _fromTable = fromTable;
            _conditionals = CleanConditional(conditionals);
            //_cleanedSelectedColumns = CleanColumns(selectedColumns);
            if (_conditionals != null)
            {
                CommandString = "SELECT " + string.Join(",", _fromTable.GetSQLColumnsToSelect()) + " FROM " +
                                _fromTable.GetSqlQueryRepresentation() + " WHERE " + _conditionals.GetQueryString();
            }
            else
            {
                CommandString = "SELECT " + string.Join(", ", _fromTable.GetSQLColumnsToSelect()) + " FROM " +
                                _fromTable.GetSqlQueryRepresentation();
            }
        }
        

        /// <summary>
        /// If any of the properties in the conditional to be cleaned does not exist in 
        /// the tables where this query is selecting from, return a null conditional. This means that 
        /// when the select query is executed, the conditional will be ignored. 
        /// </summary>
        /// <param name="conditionalToBeCleaned"></param>
        /// <returns></returns>
        private SqlQueryConditional CleanConditional(SqlQueryConditional conditionalToBeCleaned)
        {
            if (conditionalToBeCleaned == null)
            {
                return null;
            }
            var acceptedColumns = new HashSet<string>();
            foreach (var table in _fromTable.GetSqlTableNames())
            {
                acceptedColumns.UnionWith(Constants.GetAcceptedKeys(table));
            }
            foreach (var column in conditionalToBeCleaned.GetPropertyKeys())
            {
                if (!acceptedColumns.Contains(column))
                {
                    return null;
                }
            }
            return conditionalToBeCleaned;
        }

        ///// <summary>
        ///// Checks whether columns passed in exist in the any of the tables involved in the sql query. Returns,
        ///// cleaned columns.
        ///// </summary>
        ///// <param name="columnsToClean"></param>
        ///// <returns></returns>
        //private IEnumerable<string> CleanColumns(IEnumerable<string> columnsToClean)
        //{
        //    IEnumerable<string> acceptedColumns = new HashSet<string>();
        //    foreach (var table in _fromTable.GetSqlTableNames())
        //    {
        //        acceptedColumns = acceptedColumns.Concat(Constants.GetAcceptedKeys(table));
        //    }
        //    return columnsToClean.Intersect(acceptedColumns);
        //}

        /// <summary>
        /// Creates sql command and executes. Returns IEnumerable<Message>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Message> ExecuteCommand()
        {
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            return ContentController.Instance.SqlConnector.ExecuteSelectQueryAsMessages(cmd, false);
        }
        

    }
}