using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// A class to hold either a table name or a joined table
    /// </summary>
    public class SqlTableRepresentation
    {
        //keeps track of the string that represents the table
        private string _table;

        /// <summary>
        /// this constructor allows you to input a table representation, such as a 
        /// </summary>
        /// <param name="table"></param>
        public SqlTableRepresentation(SqlSelectCondition table)
        {
            _table = table.GetQueryString();
        }

        public SqlTableRepresentation(Constants.SQLTableType type)
        {
            _table = SQLConnector.GetTableName(type);
        }

        public string GetTableString()
        {
            return _table;
        }
    }
    public interface SqlSelectCondition
    {
        string GetQueryString();
        IEnumerable<string> GetPropertyKeys();
    }

    public class SingleTableWhereCondition : SqlSelectCondition
    {
        public SqlSelectQueryConditional Conditional { get; private set; } 
        public SingleTableWhereCondition(SqlSelectQueryConditional conditional)
        {
            Conditional = conditional;
        }
        public string GetQueryString()
        {
            return " WHERE " + Conditional.GetQueryString();
        }
        public IEnumerable<string> GetPropertyKeys()
        {
            return Conditional.GetPropertyKeys();
        }
    }

    public class LeftJoinWhereCondition : SqlSelectCondition
    {
        public SqlSelectQueryConditional Conditional { get; private set; }
        public SqlTableRepresentation Left { get; private set; }
        public SqlTableRepresentation Right { get; private set; }
        public LeftJoinWhereCondition(SqlTableRepresentation left, SqlTableRepresentation right, SqlSelectQueryConditional conditional)
        {
            Conditional = conditional;
            Left = left;
            Right = right;
        }
        public string GetQueryString()
        {
            return "(" +Left.GetTableString() + " LEFT JOIN "+ Right.GetTableString() +" ON "+Conditional.GetQueryString()+")";
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return Conditional.GetPropertyKeys();
        }
    }

    public class InnerJoinWhereCondition : SqlSelectCondition
    {
        public SqlSelectQueryConditional Conditional { get; private set; }
        public SqlTableRepresentation Left { get; private set; }
        public SqlTableRepresentation Right { get; private set; }
        public InnerJoinWhereCondition(SqlTableRepresentation left, SqlTableRepresentation right, SqlSelectQueryConditional conditional)
        {
            Conditional = conditional;
            Left = left;
            Right = right;
        }
        public string GetQueryString()
        {
            return "(" + Left.GetTableString() + " INNER JOIN " + Right.GetTableString() + " ON " + Conditional.GetQueryString() + ")";
        }
        public IEnumerable<string> GetPropertyKeys()
        {
            return Conditional.GetPropertyKeys();
        }
    }

    /// <summary>
    /// This is the interface for any select query. It has a string that represents the query string
    /// and a list of the properties (table columns) to check against.
    /// </summary>
    public interface SqlSelectQueryConditional
    {
        string GetQueryString();
        IEnumerable<string> GetPropertyKeys();
    }

    /// <summary>
    /// Creates a select query by performing operations (AND/OR) on two different conditionals
    /// </summary>
    public class SqlSelectQueryOperator : SqlSelectQueryConditional
    {
        public Constants.Operator Operator { get; private set; }
        public SqlSelectQueryConditional FirstConditional { get; private set; }
        public SqlSelectQueryConditional SecondConditional { get; private set; }

        /// <summary>
        /// Creates a select query by performing operations (AND/OR) on two different conditionals
        /// </summary>
        /// <param name="firstConditional"></param>
        /// <param name="secondConditional"></param>
        /// <param name="conditionalOperator"></param>
        public SqlSelectQueryOperator(SqlSelectQueryConditional firstConditional, SqlSelectQueryConditional secondConditional, Constants.Operator conditionalOperator)
        {
            if (firstConditional == null || secondConditional == null)
            {
                throw new Exception("cannot create a Sql Query Operator with null conditionals");
            }
            Operator = conditionalOperator;
            FirstConditional = firstConditional;
            SecondConditional = secondConditional;
        }

        /// <summary>
        /// Returns the query string with the operation in between the two different conditionals
        /// </summary>
        /// <returns></returns>
        public string GetQueryString()
        {
            return " ("+FirstConditional.GetQueryString() + " "+Operator.ToString() +" "+SecondConditional.GetQueryString() +") ";
        }

        /// <summary>
        /// Returns the list of properties (table columns) that are checked
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetPropertyKeys()
        {
            return FirstConditional.GetPropertyKeys().Concat(SecondConditional.GetPropertyKeys());
        }
    }

    /// <summary>
    /// Creates a select query conditional that checks if the property = the required value
    /// </summary>
    public class SqlSelectQueryEquals : SqlSelectQueryConditional
    {
        public string Property { get; private set; }
        public string RequiredValue { get;private set; }
        /// <summary>
        /// Creates a select query conditional that checks if the property = the required value
        /// </summary>
        public SqlSelectQueryEquals(Constants.SQLTableType tableType, string property, string requiredValue)
        {
            if (property == null || requiredValue == null)
            {
                throw new Exception("cannot create a Sql Query Equals conditional with null conditionals");
            }
            Property = SQLConnector.GetTableName(tableType) +"."+ property;
            RequiredValue = requiredValue;
        }

        /// <summary>
        /// Returns the query string that checks if the property = the required value
        /// </summary>
        public string GetQueryString()
        {
            return Property + " ='" + RequiredValue + "' ";
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
    public class SqlSelectQueryContains : SqlSelectQueryConditional
    {
        public string Property { get; private set; }
        public IEnumerable<string> PossibleValues { get; private set; }

        /// <summary>
        /// Creates a select query conditional that checks if the property is equal to any of the possible values passed in.
        /// </summary>
        public SqlSelectQueryContains(Constants.SQLTableType tableType, string property, IEnumerable<string> possibleValues)
        {
            if (property == null || possibleValues == null || !possibleValues.Any())
            {
                throw new Exception("cannot create a Sql Query contains conditional with null conditionals or no possible values");
            }
            Property = SQLConnector.GetTableName(tableType) + "." + property;
            PossibleValues = possibleValues;
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
}