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
        public SqlSelectQueryEquals(string property, string requiredValue)
        {
            if (property == null || requiredValue == null)
            {
                throw new Exception("cannot create a Sql Query Equals conditional with null conditionals");
            }
            Property = property;
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
        public SqlSelectQueryContains(string property, IEnumerable<string> possibleValues)
        {
            if (property == null || possibleValues == null || !possibleValues.Any())
            {
                throw new Exception("cannot create a Sql Query contains conditional with null conditionals or no possible values");
            }
            Property = property;
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