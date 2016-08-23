using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
{

    /// <summary>
    /// A dictionary that internally stores another dictionary with inverse Key-Value relationships, such that
    /// Keys can be retrieved by passing in a value to the GetKeyByValue() method.
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class BiDictionary<TK, TV> : Dictionary<TK,TV>
    {
        private Dictionary<TV, TK> valueToKey = new Dictionary<TV, TK>();

        public TK GetKeyByValue(TV val)
        {
            return valueToKey[val];
        }

        public bool ContainsValue(TV value)
        {
            return valueToKey.ContainsKey(value);
        }

        public TV this[TK i]
        {
            get { return base[i]; }
            set { base[i] = (TV)value;
                valueToKey[(TV)value] = i; }
        }

    }
}
