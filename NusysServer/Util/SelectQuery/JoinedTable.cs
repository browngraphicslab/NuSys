using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysServer.Misc;

namespace NusysServer
{
    public class JoinedTable : ITableRepresentable
    {
        private SqlJoinOperationArgs _args;
        private string _queryString;

        /// <summary>
        /// Creates a joined table based on the arguments passed in.
        /// </summary>
        /// <param name="args"></param>
        public JoinedTable(SqlJoinOperationArgs args)
        {
            _args = args;
            string joinString ="";
            switch (args.JoinOperator)
            {
                case Constants.JoinedType.InnerJoin:
                    joinString = " INNER JOIN ";
                    break;
                case Constants.JoinedType.LeftJoin:
                    joinString = " LEFT JOIN ";
                    break;
                case Constants.JoinedType.RightJoin:
                    joinString = " RIGHT JOIN ";
                    break;
            }
            _queryString = args.LeftTable.GetSqlQueryRepresentation() + joinString +
                           args.RightTable.GetSqlQueryRepresentation() + " ON " + args.Column1TableSource.ToString() +
                           "." + args.Column1 + " = " + args.Column2TableSource.ToString() + "." + args.Column2;
        }
        /// <summary>
        /// Returns the string representation for the query string. Goes after SELECT ____ FROM...
        /// </summary>
        /// <returns></returns>
        public string GetSqlQueryRepresentation()
        {
            return _queryString;
        }

        /// <summary>
        /// Recursively gets all the table types that may be nested in the table.
        /// </summary>
        /// <returns></returns>
        public List<Constants.SQLTableType> GetSqlTableNames()
        {
            List<Constants.SQLTableType> tableNames = new List<Constants.SQLTableType>();
            tableNames.AddRange(_args.LeftTable.GetSqlTableNames());
            tableNames.AddRange(_args.RightTable.GetSqlTableNames());
            return tableNames;
        }
    }
}