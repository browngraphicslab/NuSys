using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// This is an interface that allows wrappers (RectangleWrapper and AudioWrapper)
    /// to show/hide specific regions or all regions. 
    /// 
    /// In our current implementation, there are two ways to show/hide regions:
    /// 1) Show/Hide only regions that are descendants of this region (ie, were created by it in
    /// the region tab of the detail view). This does not include descendants of descendants.
    /// 
    /// 2) Show/Hide ALL regions
    /// </summary>
    public interface IRegionHideable
    {

        /// <summary>
        /// Shows all the regions made on the content
        /// </summary>
        void ShowAllRegions();
        /// <summary>
        /// Hides all the regions made on the content
        /// </summary>
        void HideAllRegions();
        /// <summary>
        /// Shows only descendant regions
        /// </summary>
        void ShowOnlyChildrenRegions();
    }
}
