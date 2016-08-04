using System.Collections.Generic;

namespace NusysServer
{
    public class SingleTable: ITableRepresentable
    {
        private string _sqlQueryString;
        private Constants.SQLTableType _tableType;

        /// <summary>
        /// Creates a new single table based on the table type passed in
        /// </summary>
        /// <param name="tableType"></param>
        public SingleTable(Constants.SQLTableType tableType)
        {
            _tableType = tableType;
            _sqlQueryString = SQLConnector.GetTableName(tableType);
        }

        /// <summary>
        /// Returns the single table name associated with this itablerepresentable
        /// </summary>
        /// <returns></returns>
        public List<Constants.SQLTableType> GetSqlTableNames()
        {
            return new List<Constants.SQLTableType>() {_tableType};
        }

        /// <summary>
        /// Returns the query string to go after SELECT___ FROM...
        /// </summary>
        /// <returns></returns>
        public string GetSqlQueryRepresentation()
        {
            return _sqlQueryString;
        }
    }
}