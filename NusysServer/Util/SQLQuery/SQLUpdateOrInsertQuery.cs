using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class SQLUpdateOrInsertQuery
    {
        private string _commandString;
        /// <summary>
        /// should only ever really be used for the properties table
        /// </summary>
        /// <param name="tableToUpdate"></param>
        /// <param name="propertiesAndValuesToUpdate"></param>
        /// <param name="conditional"></param>
        public SQLUpdateOrInsertQuery(SingleTable tableToUpdate, IEnumerable<SqlSelectQueryEquals> propertiesAndValuesToUpdate, SqlSelectQueryConditional conditional)
        {
            foreach(var propertyToAdd)
            _commandString = "begin tran" +
                                 "Update " + tableToUpdate.GetSqlTableNames().First() + " with (serializable)set " + string.Join(",", propertiesAndValuesToUpdate.Select(k => k.GetQueryString()) + " " +
                                 "where " + conditional.GetQueryString() + " " +
                                 "if @@rowcount = 0 " +
                                 "begin " +
                                      "insert into " + tableToUpdate.GetSqlTableNames().First() + " (" + string.Join(",", propertiesAndValuesToUpdate.Select(k => k.Property) + "," + string.Join(",", conditional.GetPropertyKeys()) + ") values(" + string.Join(",", propertiesAndValuesToUpdate.Select(k => k.RequiredValue) + ") " +
                                 "end " +
                             "commit tran;";

        }

        public bool ExecuteCommand()
        {
            var cmd = ContentController.Instance.SqlConnector.MakeCommand(_commandString);
            var success = cmd.ExecuteNonQuery();
            return success > 0;
        }
    }
}