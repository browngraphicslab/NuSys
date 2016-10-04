using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
namespace NuSysApp
{
    public class ListViewUIElement<T> : RectangleUIElement
    {
        private List<T> _itemsSource;
        private List<ListColumn<T>> _listColumns;
        private List<ListViewRowUIElement<T>> _listViewRowUIElements;
        private HashSet<ListViewRowUIElement<T>> _selectedElements;
        private bool _multipleSelections;
        public ListViewUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, List<T> itemsSource = null) : base(parent, resourceCreator)
        {
            _itemsSource = new List<T>();

            if (itemsSource == null)
            {
                AddItems(itemsSource);
            }
            _listColumns = new List<ListColumn<T>>();
        }

        /// <summary>
        /// This method will populate the list view using the functions of the columns and the item source.
        /// </summary>
        public void PopulateListView()
        {
            
        }

        /// <summary>
        /// Appends things to the _itemsSource list. Creates new ListViewRowUIElement for each of the items
        /// and also adds those to the list of RowUIElements.
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddItems(List<T> itemsToAdd)
        {
            
        }

        /// <summary>
        /// Removes things to the _itemsSource list. Removes the Row from the ListViewRowUIElements list.
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void RemoveItems(List<T> itemsToRemove)
        {

        }

        /// <summary>
        /// This should add the listColumn to the _listColumns.
        /// This should also update all the ListViewRowUIElements apprpriately by adding the proper cells.
        /// This method will not graphically reload the entire list.
        /// </summary>
        /// <param name="column"></param>
        public void AddColumn(ListColumn<T> listColumn)
        {
            
        }

        /// <summary>
        /// This should remove the column with the name from _listColumns.
        /// This should also update all the listviewRowUIElements appropriately by removing the proper cells.
        /// This method will not graphically reload the entire list
        /// </summary>
        /// <param name="listColumn"></param>
        public void RemoveColumn(string columnTitle)
        {
            
        }

        /// <summary>
        /// Scrolls down to the item
        /// </summary>
        /// <param name="item"></param>
        public void ScrollTo(T item)
        {
            
        }

        /// <summary>
        /// Returns the items (not the row element) selected.
        /// </summary>
        public IEnumerable<T> GetSelectedItems()
        {
            return _selectedElements.Select(row => row.Item);
        }

    }
}
