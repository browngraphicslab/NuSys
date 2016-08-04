using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysServer.Misc;

namespace NusysServer
{
    public class JoinedTable : ITableRepresentable
    {
        private string _queryString;
        public JoinedTable(SqlJoinOperationArgs args)
        {
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
        public string GetSqlQueryRepresentation()
        {
            return _queryString;
        }

        public List<Constants.SQLTableType> GetSqlTableNames()
        {
            throw new NotImplementedException();
        }
    }
}