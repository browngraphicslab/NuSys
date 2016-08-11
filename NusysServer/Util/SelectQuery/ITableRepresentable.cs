using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public interface ITableRepresentable
    {
        /// <summary>
        /// Returns all the table names involved
        /// </summary>
        /// <returns></returns>
        List<Constants.SQLTableType> GetSqlTableNames();
        

        /// <summary>
        /// Returns all the columns you wish to select from the table.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSQLColumnsToSelect();

        /// <summary>
        /// Returns the fomateed string which can be pluugd into the Select query.
        /// Comes after FROM
        /// </summary>
        /// <returns></returns>
        string GetSqlQueryRepresentation();
    }
}