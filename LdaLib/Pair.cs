using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LdaLibrary
{
    public class Pair
    {
        public object first;
        public IComparable second; // replaced comparable with icomparable
        public static bool naturalOrder = false;

        public Pair(Object k, IComparable v)
        {
            first = k;
            second = v;
        }

        public Pair(Object k, IComparable v, bool naturalOrder)
        {
            first = k;
            second = v;
            Pair.naturalOrder = naturalOrder;
        }

        public int compareTo(Pair p)
        {
            if (naturalOrder)
                return this.second.CompareTo(p.second); //switched compareTo with CompareTo
            else return -this.second.CompareTo(p.second);
        }
    }
}
