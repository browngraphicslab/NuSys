using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.EventArgs
{
    public class DeleteInqLineEventArgs : System.EventArgs
    {
        public DeleteInqLineEventArgs(InqLineModel lineModel)
        {
            LineModelToDelete = lineModel;
        }

        public InqLineModel LineModelToDelete { get; }
    }
}
