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
        public AddPartialLineEventArgs(string text, IList<Line> lines):base(text)
        {
            AddedLines = lines;
        }

        public IList<Line> AddedLines { get; }
    }
}
