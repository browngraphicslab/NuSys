using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface LibraryViewable
    {
        Task Sort(string s);
        Task Search(string s);
        void SetItems(ICollection<LibraryElement> elements);
    }
}
