using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// This is the class for displaying pages of data as a list
    /// </summary>
    /// <typeparam name="T">
    /// The type of data to be associated with the values in the list.
    /// </typeparam>
    class PaginatedListView<T> : RectangleUIElement
    {
        /// <summary>
        /// The list ui element to display the data
        /// </summary>
        private ListViewUIElementContainer<T> _list;

        /// <summary>
        /// The source element that is used to provide the data for the next page and the previous page
        /// </summary>
        private NextPageable<T> _source;

        /// <summary>
        /// Creates a new Paginated list view
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="source">
        ///  The source element that is used to provide the data for the next page and the previous page
        ///  </param>
        public PaginatedListView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, NextPageable<T> source) : base(parent, resourceCreator)
        {
            if(source == null)
            {
                throw new ArgumentNullException("Cannot have a null source when creating a paginated list view");
            }
            _list = new ListViewUIElementContainer<T>(parent, resourceCreator);
            AddChild(_list);
            _source = source;
        }

        /// <summary>
        /// This adds all the columns to _listColumns. If you are adding multiple columns use this instead of the AddColumn method
        /// so that the list only reloads once.
        /// </summary>
        /// <param name="listColumns"></param>
        public void AddColumns(IEnumerable<ListColumn<T>> listColumns)
        {
            _list.AddColumns(listColumns);
        }

        /// <summary>
        /// This should add the listColumn to the _listColumns.
        /// This should also update all the listviewRowUIElements appropriately by repopulating the row with cells with the proper widths.
        /// </summary>
        /// <param name="column"></param>
        public void AddColumn(ListColumn<T> listColumn)
        {
            _list.AddColumn(listColumn);
        }

    }
}
