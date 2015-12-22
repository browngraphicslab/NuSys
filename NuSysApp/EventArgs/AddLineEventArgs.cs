using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class AddLineEventArgs : System.EventArgs
    {
        public AddLineEventArgs(InqLineModel line)
        {
            AddedLineModel = line;
        }

        public InqLineModel AddedLineModel { get; }
    }
}
