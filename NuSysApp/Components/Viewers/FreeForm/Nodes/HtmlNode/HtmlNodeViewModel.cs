using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class HtmlNodeViewModel : ImageElementViewModel
    {
        public HtmlNodeViewModel(HtmlElementController controller) : base(controller)
        {
            Debug.Assert(controller != null);
        }
    }
}
