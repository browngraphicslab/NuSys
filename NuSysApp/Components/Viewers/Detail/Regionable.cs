using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface Regionable <T>
    {

        void AddRegion();

        void RemoveRegion(T displayedRegion);

        void DisplayRegion(Region region);
    }
}
