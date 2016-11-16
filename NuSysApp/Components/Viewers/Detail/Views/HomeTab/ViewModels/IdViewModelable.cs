using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface IdViewModelable
    {
        string Id { get;}
        double Width { get; }
        double Height { get; }
        double X { get; }
        double Y { get; }
        bool IsSelected { get; }
    }
}
