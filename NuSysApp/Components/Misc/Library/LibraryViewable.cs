using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface LibraryViewable
    {
        void Sort(string s);
        void Search(string s);
        void SetItems(ICollection<LibraryElement> elements);
    }
}
