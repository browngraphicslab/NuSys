using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public interface LibraryViewable
    {
        Task Sort(string s, bool reverse = false);
        Task Search(string s);
        void SetItems(ICollection<LibraryElementModel> elements);
    }
}
