using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class BasicToolModel : ToolModel
    {
        

        public ToolFilterTypeTitle Filter { get; private set; }
        public  string Selection { get; protected set; }

        public void SetSelection(string selection)
        {
            Selection = selection;
        }
        public void SetFilter(ToolFilterTypeTitle filter)
        {
            Filter = filter;
        }
    }
}
