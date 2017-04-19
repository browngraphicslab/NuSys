using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class Page<T>
    {
        public List<T> Items
        {
            get;
            private set;
        }

        public String NextPageUrl {
            get;private set;
        }


        public Page(List<T> items, String nextPageUrl)
        {
            Items = items;
            NextPageUrl = nextPageUrl;
        }
    }
}
