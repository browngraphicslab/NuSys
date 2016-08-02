using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public interface SqlSelectQueryConditional
    {
        string GetQueryString();
        IEnumerable<string> GetPropertyKeys();
    }

    public class SqlSelectQueryOperator : SqlSelectQueryConditional
    {
        public Constants.Operator Operator { get; private set; }
        public SqlSelectQueryConditional FirstConditional { get; private set; }
        public SqlSelectQueryConditional SecondConditional { get; private set; }
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

        public string GetQueryString()
        {
            return " ("+FirstConditional.GetQueryString() + " "+Operator.ToString() +" "+SecondConditional.GetQueryString() +") ";
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return FirstConditional.GetPropertyKeys().Concat(SecondConditional.GetPropertyKeys());
        }
    }

    public class SqlSelectQueryEquals : SqlSelectQueryConditional
    {
        public string Property { get; private set; }
        public string RequiredValue { get;private set; }

        public SqlSelectQueryEquals(string property, string requiredValue)
        {
            if (property == null || requiredValue == null)
            {
                throw new Exception("cannot create a Sql Query Equals conditional with null conditionals");
            }
            Property = property;
            RequiredValue = requiredValue;
        }

        public string GetQueryString()
        {
            return Property + " ='" + RequiredValue + "' ";
        }
        public IEnumerable<string> GetPropertyKeys()
        {
            return new List<string>() { Property};
        }
    }

    public class SqlSelectQueryContains : SqlSelectQueryConditional
    {
        public string Property { get; private set; }
        public IEnumerable<string> PossibleValues { get; private set; }

        public SqlSelectQueryContains(string property, IEnumerable<string> possibleValues)
        {
            if (property == null || possibleValues == null || !possibleValues.Any())
            {
                throw new Exception("cannot create a Sql Query contains conditional with null conditionals or no possible values");
            }
            Property = property;
            PossibleValues = possibleValues;
        }
        public string GetQueryString()
        {
            return Property + " IN('" + string.Join("','",PossibleValues) + "') ";
        }
        public IEnumerable<string> GetPropertyKeys()
        {
            return new List<string>() { Property };
        }
    }
}