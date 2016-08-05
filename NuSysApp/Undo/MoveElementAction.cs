using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class MoveElementAction : IUndoable
    {
        public IUndoable GetInverse()
        {
            throw new NotImplementedException();
        }

        public Request ToRequest()
        {
            throw new NotImplementedException();
        }
    }
}
