using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace NusysServer.Util.SQLQuery
{
    public class SQLUpdateRowQuery
    {
        public string CommandString { get; private set; }
        private ITableRepresentable _tableToUpdate;
        /// <summary>
        /// This updates a single row in the table.
        /// </summary>
        /// <param name="tableToUpdate"></param>
        /// <param name="propertiesToUpdate"></param>
        /// <param name="conditional"></param>
        public SQLUpdateRowQuery(ITableRepresentable tableToUpdate, List<SqlQueryEquals> propertiesToUpdate, SqlQueryConditional conditional)
        {
            CommandString = "UPDATE " + tableToUpdate.GetSqlQueryRepresentation() + " SET " +
                             string.Join(",", propertiesToUpdate.Select(q => q.GetQueryString())) + " WHERE "+
                             conditional.GetQueryString();
        }

        /// <summary>
        /// Removes any of the properties to update that dont exist in the table to update.
        /// </summary>
        /// <param name="propertiesToClean"></param>
        /// <returns></returns>
        public IEnumerable<SqlQueryEquals> CleanPropertiesToUpdate(List<SqlQueryEquals> propertiesToClean)
        {
            if (propertiesToClean == null)
            {
                return null;
            }
            var acceptedColumns = new HashSet<string>();
            foreach (var table in _tableToUpdate.GetSqlTableNames())
            {
                acceptedColumns.UnionWith(Constants.GetAcceptedKeys(table));
            }
            foreach (var column in propertiesToClean)
            {
                if (!acceptedColumns.Contains(column.Property))
                {
                    propertiesToClean.Remove(column);
                }
            }
            return propertiesToClean;
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
            foreach (var table in _tableToUpdate.GetSqlTableNames())
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
        /// Executes the sql command and returns a bool on whether atleast 1 row was edited.
        /// </summary>
        /// <returns></returns>
        public bool ExecuteCommand()
        {
            if (CommandString == null || CommandString.Equals(""))
            {
                throw new Exception("trying to execute update row but the command string is empty");
            }
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(CommandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}