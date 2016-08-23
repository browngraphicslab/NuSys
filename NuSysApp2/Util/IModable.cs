using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
{
    public interface IModable
    {
        void GoToCurrent();
        bool Next();
        void MoveToNext();
        bool Previous();
        void MoveToPrevious();
        void ExitMode();
        ModeType Mode { get; }
    }
}
