using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class IDMap<T,V>
    {
        private ConcurrentDictionary<T, V> _dict = new ConcurrentDictionary<T, V>();

        /// <summary>
        /// returns the value or null if not exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public V Get(T key)
        {
            Debug.Assert(key != null);
            Debug.Assert(_dict.ContainsKey(key));
            return _dict[key];
        }

        /// <summary>
        /// just means "ContainsKey"  
        /// Shorter and easier to read
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Has(T key)
        {
            Debug.Assert(key != null);
            return _dict.ContainsKey(key);
        }

        /// <summary>
        /// Adds or overrides the value for that key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(T key, V value)
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);
            _dict[key] = value;
        }


    }
}
