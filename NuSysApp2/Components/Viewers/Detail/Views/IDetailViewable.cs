using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface IDetailViewable
    {
        string TabId();

        void SetTitle(string title);

        event EventHandler<string> TitleChanged;
    }
}
