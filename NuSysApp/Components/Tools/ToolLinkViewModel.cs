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
        public delegate void DisposedEventHandler();
        public event DisposedEventHandler Disposed;
        public ToolLinkable InTool;
        public ToolLinkable OutTool;

        public ToolLinkViewModel(ToolLinkable inTool, ToolLinkable outTool)
        {
            SetUpInTool(inTool);
            SetUpOutTool(outTool);
        }

        /// <summary>
        /// This function is called when you want to replace the outtool object with a new tool.
        /// (e.g when you switch from filter chooser to an actual tool, or from all metadata to basic or vice versa) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newOutTool"></param>
        private void OutTool_ReplacedToolLinkAnchorPoint(object sender, ToolLinkable newOutTool)
        {
            RemoveOutToolListeners();
            SetUpOutTool(newOutTool);
            RaisePropertyChanged("Anchor");
        }

        /// <summary>
        /// This function is called when you want to replace the intool object with a new tool.
        /// (e.g when you switch from filter chooser to an actual tool, or from all metadata to basic or vice versa) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newInTool"></param>
        private void InTool_ReplacedToolLinkAnchorPoint(object sender, ToolLinkable newInTool)
        {
            RemoveInToolListeners();
            SetUpInTool(newInTool);
            RaisePropertyChanged("Anchor");
        }

        /// <summary>
        /// When either tool the link is connected to is deleted, remove the link, and remove and listeners.
        /// </summary>
        public void Tool_Disposed(object sender, string id)
        {
            Dispose();
        }

        /// <summary>
        /// When either tool linkable anchors change, let the view know.
        /// </summary>
        private void ToolToolAnchorChanged(object sender, Point2d e)
        {
            RaisePropertyChanged("Anchor");
        }

        /// <summary>
        /// Removes listeners from in tool and out tool. Also fires the disposed event so that the tool link view can dispose of itself visually
        /// </summary>
        public void Dispose()
        {
            RemoveInToolListeners();
            RemoveOutToolListeners();
            Disposed?.Invoke();
        }

        /// <summary>
        /// Sets the passed in ToolLinkable as the new intool and sets up the listeners
        /// </summary>
        /// <param name="inTool"></param>
        public void SetUpInTool(ToolLinkable inTool)
        {
            InTool = inTool;
            inTool.ToolAnchorChanged += ToolToolAnchorChanged;
            inTool.ReplacedToolLinkAnchorPoint += InTool_ReplacedToolLinkAnchorPoint;
            inTool.Disposed += Tool_Disposed;
        }

        /// <summary>
        /// Sets the passed in ToolLinkable as the new outTool and sets up the listeners
        /// </summary>
        public void SetUpOutTool(ToolLinkable outTool)
        {
            OutTool = outTool;
            outTool.ToolAnchorChanged += ToolToolAnchorChanged;
            outTool.Disposed += Tool_Disposed;
            outTool.ReplacedToolLinkAnchorPoint += OutTool_ReplacedToolLinkAnchorPoint;
        }

        /// <summary>
        /// Removes listeners for current inTool
        /// </summary>
        public void RemoveInToolListeners()
        {
            InTool.Disposed -= Tool_Disposed;
            InTool.ToolAnchorChanged -= ToolToolAnchorChanged;
            InTool.ReplacedToolLinkAnchorPoint -= InTool_ReplacedToolLinkAnchorPoint;
        }

        /// <summary>
        /// Removes listeners for current outTool
        /// </summary>
        public void RemoveOutToolListeners()
        {
            OutTool.Disposed -= Tool_Disposed;
            OutTool.ToolAnchorChanged -= ToolToolAnchorChanged;
            OutTool.ReplacedToolLinkAnchorPoint -= OutTool_ReplacedToolLinkAnchorPoint;
        }
    }
}