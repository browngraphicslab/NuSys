using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using NuSysApp.Tools;

namespace NuSysApp
{
    public class ToolLinkViewModel : BaseINPC
    {
        public ToolLinkable InTool;
        public ToolLinkable OutTool;

        public ToolLinkViewModel(ToolLinkable inTool, ToolLinkable outTool)
        {
            InTool = inTool;
            OutTool = outTool;
            
            InTool.ToolAnchorChanged += ToolToolAnchorChanged;
            OutTool.ToolAnchorChanged += ToolToolAnchorChanged;
        }

        /// <summary>
        /// When either tool linkable anchors change, let the view know.
        /// </summary>
        private void ToolToolAnchorChanged(object sender, Point2d e)
        {
            RaisePropertyChanged("Anchor");
        }

        public void Dispose()
        {
            InTool.ToolAnchorChanged -= ToolToolAnchorChanged;
            OutTool.ToolAnchorChanged -= ToolToolAnchorChanged; 
        }
    }
}