using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace NuSysApp
{
    public interface Sendable
    {
        Task<Dictionary<string, string>> Pack();
        Task UnPack(Dictionary<string, string> props);
        string ID { get;}
        Atom.EditStatus CanEdit { set; get; }
    }
}
