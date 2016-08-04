using System;

namespace NuSysApp.Tools
{
    public interface ToolLinkable
    {
        /// <summary>
        ///The anchor that the tool link uses
        /// </summary>
        Point2d ToolAnchor { get; }

        /// <summary>
        ///The event that fires when the node has been repositioned or resized
        /// </summary>
        event EventHandler<Point2d> ToolAnchorChanged;
        event EventHandler<string> Disposed;
        event EventHandler<ToolLinkable> ReplacedToolLinkAnchorPoint;


        /// <summary>
        ///Should return the tool startable. If there is none, returns null
        /// </summary>
        ToolStartable GetToolStartable();

    }
}