using System.Collections.Generic;

namespace NuSysApp
{
    /// <summary>
    /// This class is just a logical (not graphical) container for the win2dlistviewitems in a certain row.
    /// </summary>
    public class ListViewRowContainer<T>
    {
        /// <summary>
        /// This is the source item for this particular row (e.g. a specific library element model)
        /// </summary>
        private T _source;

        //Each item in this list corresponds to a different column in this list
        //These items will be
        //private List<Win2dListViewItem>;
    }
}