using System;
using System.Collections.Generic;
using System.Linq;

namespace NusysServer
{
    public class SingleTable: ITableRepresentable
    {
        private string _sqlQueryString;
        private Constants.SQLTableType _tableType;
        private IEnumerable<string> _columnsToSelect;

        /// <summary>
        /// Creates a new single table based on the table type passed in
        /// </summary>
        /// <param name="tableType"></param>
        public SingleTable(Constants.SQLTableType tableType, IEnumerable<string> columnsToSelect = null)
        {
            _tableType = tableType;
            _sqlQueryString = Constants.GetTableName(tableType);
            if(columnsToSelect == null)
            {
                _columnsToSelect = new List<string>() { Constants.GetTableName(tableType) + ".*" };
            }
            else
            {
                _columnsToSelect = CleanColumns(columnsToSelect);
            }
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

        public IEnumerable<string> GetSQLColumnsToSelect()
        {
            return _columnsToSelect;
        }

        public IEnumerable<string> CleanColumns(IEnumerable<string> columnsToClean)
        {
            IEnumerable<string> acceptedKeys = Constants.GetAcceptedKeys(_tableType, true);
            return columnsToClean.Intersect(acceptedKeys);
        }
    }
}