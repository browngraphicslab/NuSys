using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysServer
{
    public class BiDictionary<TK, TV> : Dictionary<TK, TV>
    {
        private readonly Dictionary<TV, TK> _valueToKey = new Dictionary<TV, TK>();

        public TK GetKeyByValue(TV val)
        {
            return _valueToKey[val];
        }

        public bool ContainsValue(TV value)
        {
            return _valueToKey.ContainsKey(value);
        }

        public TV this[TK i]
        {
            get { return base[i]; }
            set
            {
                base[i] = (TV)value;
                _valueToKey[(TV)value] = i;
            }
        }

    }
}
