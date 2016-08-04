﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class SQLSelectQuery
    {
        private List<NusysConstants.SqlColumns> _cleanedSelectedColumns;
        private ITableRepresentable _fromTable;
        private SqlSelectQueryConditional _conditionals;

        /// <summary>
        /// Creates a new select query based on parameters.
        /// </summary>
        /// <param name="selectedColumns"> The columns you wish to recieve</param>
        /// <param name="fromTable">The tables from which you want to select </param>
        /// <param name="conditionals">Optional. Checks for conditional</param>
        public SQLSelectQuery(List<NusysConstants.SqlColumns> selectedColumns, ITableRepresentable fromTable, SqlSelectQueryConditional conditionals = null)
        {
            _fromTable = fromTable;
            _conditionals = CleanConditional(conditionals);
            _cleanedSelectedColumns = CleanColumns(selectedColumns);
        }

        /// <summary>
        /// If any of the properties in the conditional to be cleaned does not exist in 
        /// the tables where this query is selecting from, return a null conditional. This means that 
        /// when the select query is executed, the conditional will be ignored. 
        /// </summary>
        /// <param name="conditionalToBeCleaned"></param>
        /// <returns></returns>
        private SqlSelectQueryConditional CleanConditional(SqlSelectQueryConditional conditionalToBeCleaned)
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

        /// <summary>
        /// Checks whether columns passed in exist in the any of the tables involved in the sql query. Returns,
        /// cleaned columns.
        /// </summary>
        /// <param name="columnsToClean"></param>
        /// <returns></returns>
        private List<NusysConstants.SqlColumns> CleanColumns(List<NusysConstants.SqlColumns> columnsToClean)
        {
            var columns = new List<NusysConstants.SqlColumns>();
            var acceptedColumns = new HashSet<string>();
            foreach (var table in _fromTable.GetSqlTableNames())
            {
                acceptedColumns.UnionWith(Constants.GetAcceptedKeys(table));
            }
            foreach (var column in columnsToClean)
            {
                if (acceptedColumns.Contains(column.ToString()))
                {
                    columns.Add(column);
                }
            }
            return columns;
        }

        /// <summary>
        /// Creates sql command and executes. Returns bool on success.
        /// </summary>
        /// <returns></returns>
        public bool ExecuteCommand()
        {
            string commandString = "";
            if (_conditionals != null)
            {
                commandString = "SELECT " + string.Join(", ", _cleanedSelectedColumns) + " FROM " +
                                _fromTable.GetSqlQueryRepresentation() + " WHERE " + _conditionals.GetQueryString();
            }
            else
            {
                commandString = "SELECT " + string.Join(", ", _cleanedSelectedColumns) + " FROM " +
                                _fromTable.GetSqlQueryRepresentation();
            }
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(commandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }

    }
}