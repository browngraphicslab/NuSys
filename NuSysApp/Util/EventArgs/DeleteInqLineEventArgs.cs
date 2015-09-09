using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.EventArgs
{
    public class DeleteInqLineEventArgs : System.EventArgs
    {
        public DeleteInqLineEventArgs(InqLine line)
        {
            LineToDelete = line;
        }

        public InqLine LineToDelete { get; }
    }
}
