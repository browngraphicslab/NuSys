using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class MainModel : AtomModel
    {
        private Dictionary<string, WorkSpaceModel> _workspaceIdDict;
        public MainModel() : base("MAIN_ID")
        {
            _workspaceIdDict = new Dictionary<string, WorkSpaceModel>();
        }

        public Dictionary<string, WorkSpaceModel> IDToWorkSpaceDict
        {
            get { return _workspaceIdDict; }
        }

        public string ID { get { return "MAIN_ID"; } }
        public override void Delete() { }
    }
}
