using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{

    /// <summary>
    /// This is the interface for any select query. It has a string that represents the query string
    /// and a list of the properties (table columns) to check against.
    /// </summary>
    public interface SqlQueryConditional
    {
        string GetQueryString();
        IEnumerable<string> GetPropertyKeys();
    }

    /// <summary>
    /// Creates a select query by performing operations (AND/OR) on two different conditionals
    /// </summary>
    public class SqlQueryOperator : SqlQueryConditional
    {
        public Constants.Operator Operator { get; private set; }

        private List<SqlQueryConditional> _conditionalsList;

        private string _queryString;
        /// <summary>
        /// Creates a select query by performing operations (AND/OR) on two different conditionals.
        /// </summary>
        /// <param name="firstConditional"></param>
        /// <param name="secondConditional"></param>
        /// <param name="conditionalOperator"></param>
        public SqlQueryOperator(SqlQueryConditional firstConditional, SqlQueryConditional secondConditional, Constants.Operator conditionalOperator)
        {
            if (firstConditional == null || secondConditional == null)
            {
                throw new Exception("cannot create a Sql Query Operator with null conditionals");
            }
            Operator = conditionalOperator;
            _conditionalsList = new List<SqlQueryConditional>();
            _conditionalsList.Add(firstConditional);
            _conditionalsList.Add(secondConditional);
            _queryString = " (" + firstConditional.GetQueryString() + " " + Operator.ToString() + " " + secondConditional.GetQueryString() + ") ";
        }

        /// <summary>
        /// This constructor creates a new query joining the list of conditionals with the conditional Operator. 
        /// </summary>
        /// <param name="listOfConditionals"></param>
        /// <param name="conditionalOperator"></param>
        public SqlQueryOperator(List<SqlQueryConditional> listOfConditionals, Constants.Operator conditionalOperator)
        {
            if (listOfConditionals.Count <= 1)
            {
                throw new Exception("cannot create a Sql Query Operator with less than two conditinals");
            }
            _conditionalsList = listOfConditionals;
            Operator = conditionalOperator;
            _queryString = "(" + string.Join(" " + Operator.ToString() + " ", listOfConditionals.Select(q => q.GetQueryString())) + ")";
        }

        /// <summary>
        /// Returns the query string with the operation in between the two different conditionals
        /// </summary>
        /// <returns></returns>
        public string GetQueryString()
        {
            return _queryString;
        }

        /// <summary>
        /// Returns the list of properties (table columns) that are checked
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetPropertyKeys()
        {
            List<string> propertyKeys = new List<string>();
            foreach (var conditional in _conditionalsList)
            {
                propertyKeys.AddRange(conditional.GetPropertyKeys());
            }
            return propertyKeys;
        }
    }

    /// <summary>
    /// Creates a select query conditional that checks if the property = the required value
    /// </summary>
    public class SqlQueryEquals : SqlQueryConditional
    {
        /// <summary>
        /// the FULL-COLUMN TITLE property that has to have the specified value
        /// </summary>
        public string Property { get; private set; }
        

        public string QueryString { get; private set; }

        /// <summary>
        /// Creates a select query conditional that checks if the property = the required value; 
        /// The property column title does not need to be the full title.  
        /// In other words, "content_id" will work, while "library_elements.content_id" will not.
        /// </summary>
        public SqlQueryEquals(Constants.SQLTableType tableType, string property, string requiredValue)
        {
            if (property == null || requiredValue == null)
            {
                throw new Exception("cannot create a Sql Query Equals conditional with null conditionals");
            }
            Property = Constants.GetFullColumnTitle(tableType,property).First();
            QueryString = Property + " ='" + NusysConstants.CheckString(requiredValue) + "' ";
        }

        /// <summary>
        /// This is used to create a select query conditional that checks if the property = the value selected from the selectQuery
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="property"></param>
        /// <param name="requiredValue"></param>
        public SqlQueryEquals(Constants.SQLTableType tableType, string property, SQLSelectQuery selectQuery)
        {
            if (property == null || selectQuery == null)
            {
                throw new Exception("cannot create a Sql Query Equals conditional with null conditionals");
            }
            Property = Constants.GetFullColumnTitle(tableType, property).First();
            QueryString = Property + " = (" + selectQuery.CommandString + ") ";
        }

        /// <summary>
        /// Returns the query string that checks if the property = the required value
        /// </summary>
        public string GetQueryString()
        {
            return QueryString;
        }

        /// <summary>
        /// Returns the list of properties (table columns) that are checked
        /// </summary>
        public IEnumerable<string> GetPropertyKeys()
        {
            return new List<string>() { Property};
        }
    }

    /// <summary>
    /// Creates a select query conditional that checks if the property is equal to any of the possible values passed in.
    /// </summary>
    public class SqlQueryContains : SqlQueryConditional
    {
        public string Property { get; private set; }
        public IEnumerable<string> PossibleValues { get; private set; }

        /// <summary>
        /// Creates a select query conditional that checks if the property is equal to any of the possible values passed in.
        /// </summary>
        public SqlQueryContains(Constants.SQLTableType tableType, string property, IEnumerable<string> possibleValues)
        {
            if (property == null || possibleValues == null || !possibleValues.Any())
            {
                throw new Exception("cannot create a Sql Query contains conditional with null conditionals or no possible values");
            }
            Property = Constants.GetTableName(tableType) + "." + property;
            PossibleValues = possibleValues.Select(q => NusysConstants.CheckString(q));
        }

        /// <summary>
        /// Returns the query string that checks if the property = any of the possible values
        /// </summary>
        public string GetQueryString()
        {
            return Property + " IN('" + string.Join("','",PossibleValues) + "') ";
        }

        /// <summary>
        /// Returns the list of properties (table columns) that are checked
        /// </summary>
        public IEnumerable<string> GetPropertyKeys()
        {
            return new List<string>() { Property };
        }
    }


    /// <summary>
    /// Creates a select query condtional that checks if the any of the possible values passed in is a substring of the property's value
    /// </summary>
    public class SqlQueryContainsSubstring : SqlQueryConditional
    {
        /// <summary>
        /// the FULL-COLUMN TITLE property that has to have the specified value
        /// </summary>
        public string Property { get; private set; }

        /// <summary>
        /// the specified values that the must be a substring of property
        /// </summary>
        public IEnumerable<string> PossibleValues { get; private set; }

        /// <summary>
        /// Creates a select query condtional that checks if the any of the possible values passed in is a substring of the property's value
        /// </summary>
        public SqlQueryContainsSubstring(Constants.SQLTableType tableType, string property, IEnumerable<string> possibleValues)
        {
            if (property == null || possibleValues == null || !possibleValues.Any())
            {
                throw new Exception("cannot create a Sql Query contains conditional with null conditionals or no possible values");
            }
            Property = Constants.GetTableName(tableType) + "." + property;
            PossibleValues = possibleValues;
        }

        /// <summary>
        /// Returns the query string that checks if any of the possible values are substrings of the properties value
        /// </summary>
        public string GetQueryString()
        {
            string queryString = "";
            return string.Join("OR ", PossibleValues.Select(q => Property + " LIKE '%" + q + "%' "));
            
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return new List<string>() { Property };
        }
    }

    /// <summary>
    /// Condtional that checks if the required value is a substring of the property
    /// </summary>
    public class SqlQueryIsSubstring: SqlQueryConditional
    {
        /// <summary>
        /// the FULL-COLUMN TITLE property that has to have the specified value
        /// </summary>
        public string Property { get; private set; }

        /// <summary>
        /// the specified value that the proeprty must have as a substring to satisfy the equals condition
        /// </summary>
        public string RequiredValue { get; private set; }

        /// <summary>
        /// Creates a select query conditional that checks if the requred value is a substring of the property
        /// </summary>
        public SqlQueryIsSubstring(Constants.SQLTableType tableType, string property, string requiredValue)
        {
            if (property == null || requiredValue == null)
            {
                throw new Exception("cannot create a Sql Query Equals conditional with null conditionals");
            }
            Property = Constants.GetFullColumnTitle(tableType, property).First();
            RequiredValue = requiredValue;
        }

        /// <summary>
        /// Returns the query string that checks if the requred value is a substring of the property
        /// </summary>
        public string GetQueryString()
        {
            return Property + " LIKE '%" + RequiredValue + "%' ";
        }

        /// <summary>
        /// Returns the list of properties (table columns) that are checked
        /// </summary>
        public IEnumerable<string> GetPropertyKeys()
        {
            return new List<string>() { Property };
        }
    }
}