using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class JoinedTable : ITableRepresentable
    {
        private SqlJoinOperationArgs _args;
        private string _queryString;
        private IEnumerable<string> _columnsToSelect;

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
                           args.RightTable.GetSqlQueryRepresentation() + " ON " + args.Column1 + " = " + args.Column2;
            _columnsToSelect = args.LeftTable.GetSQLColumnsToSelect().Concat(args.RightTable.GetSQLColumnsToSelect());

        }



        /// <summary>
        /// returns the cleaned list of columns to select from this table
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSQLColumnsToSelect()
        {
            return _columnsToSelect;
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