using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.AtomPub;

namespace NuSysApp
{
    public class BasicToolModel : ToolModel
    {
        

        public ToolFilterTypeTitle Filter { get; private set; }
        public  HashSet<string> Selection { get; protected set; }

        public BasicToolModel()
        {
            Selection = new HashSet<string>();
        }

        public void SetSelection(HashSet<string> selection)
        {
            Selection = selection;
        }
        public void SetFilter(ToolFilterTypeTitle filter)
        {
            Filter = filter;
        }
    }
}
