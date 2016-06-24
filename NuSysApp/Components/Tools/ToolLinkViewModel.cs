using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ToolLinkViewModel
    {

        private ToolController InToolController;
        private ToolController OutToolController;

        private ToolView InTool;
        private ToolView OutTool;

        //public LinkModel LinkModel { get; }
        //private SolidColorBrush _defaultColor;
        public ToolLinkViewModel(ToolView inTool, ToolView outTool)
        {
            //InToolController
            InTool = inTool;
            OutTool = outTool;

        }
    }
}
