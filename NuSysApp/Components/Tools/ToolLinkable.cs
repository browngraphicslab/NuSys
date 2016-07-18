using System;

namespace NuSysApp.Tools
{
    public interface ToolLinkable
    {
        Point2d Anchor { get; }

        event EventHandler<Point2d> AnchorChanged;

    }
}