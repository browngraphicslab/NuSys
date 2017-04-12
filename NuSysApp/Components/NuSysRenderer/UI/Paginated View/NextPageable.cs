using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    interface NextPageable<T>
    {
        List<T> getNextPage();
        List<T> getPreviousPage();
    }
}
