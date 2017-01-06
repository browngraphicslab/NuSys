using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class DetailViewPageTabType : IComparable<DetailViewPageTabType>
    {


        public DetailViewPageType Type;


        public DetailViewPageTabType(DetailViewPageType type)
        {
            Type = type;
        }



        public int CompareTo(DetailViewPageTabType other)
        {
            if (Type == other.Type)
            {
                return 0;
            }
            return 1;
        }
    }
}
