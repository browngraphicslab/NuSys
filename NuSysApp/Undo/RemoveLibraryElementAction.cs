using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class RemoveLibraryElementAction : IUndoable
    {

        public RemoveLibraryElementAction(Message m)
        {
            
        }

        public IUndoable GetInverse()
        {
            throw new NotImplementedException();
        }

        public void ExecuteAction()
        {
            throw new NotImplementedException();
        }
    }
}
