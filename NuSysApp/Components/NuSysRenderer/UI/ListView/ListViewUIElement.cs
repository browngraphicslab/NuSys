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
        public ListViewUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _itemsSource = new List<T>();
            _listColumns = new List<ListColumn<T>>();
        }

        /// <summary>
        /// This method will populate the list view using the functions of the columns and the item source.
        /// </summary>
        public void PopulateListView()
        {
            
        }


    }
}
