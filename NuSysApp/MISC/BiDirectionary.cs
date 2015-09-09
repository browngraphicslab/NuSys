using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class BiDictionary<TK, TV> : Dictionary<TK,TV>
    {
        private Dictionary<TV, TK> valueToKey = new Dictionary<TV, TK>();

        public TK GetKeyByValue(TV val)
        {
            return valueToKey[val];
        }

        public TV this[TK i]
        {
            get { return base[i]; }
            set { base[i] = (TV)value;
                valueToKey[(TV)value] = i; }
        }

    }
}
