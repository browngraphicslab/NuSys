using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface ILockable
    {
        bool IsLocked { get; set; }
        string Id { get; }
        void Lock(string userId);
        void UnLock();
    }
}
