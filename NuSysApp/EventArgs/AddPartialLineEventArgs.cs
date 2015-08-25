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
        public AddPartialLineEventArgs(string text, Line line):base(text)
        {
            AddedLines = line;
        }

        public Line AddedLines { get; }
    }
}
