using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface ILinkable : INuSysDisposable
    {
        Point2d Anchor { get; }

        event EventHandler<Point2d> AnchorChanged;

        string Id { get; }

        string ContentId { get; }

        void UpdateCircleLinks();

        string GetParentCollectionId();
    }
}
