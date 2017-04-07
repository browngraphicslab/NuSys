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
        

        public  HashSet<string> Selection { get; protected set; }

        public BasicToolModel(string id = null) : base(id ?? SessionController.Instance.GenerateId())
        {
            Selection = new HashSet<string>();
        }

        public void SetSelection(HashSet<string> selection)
        {
            Selection = selection;
        }
        
    }
}
