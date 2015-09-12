using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class AddPartialLineEventArgs : SuperEventArgs
    {
        public AddPartialLineEventArgs(string text, InqLineModel line):base(text)
        {
            AddedLineModel = line;
        }

        public InqLineModel AddedLineModel { get; }
    }
}
