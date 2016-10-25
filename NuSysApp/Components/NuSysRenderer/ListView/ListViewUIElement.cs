using System;
using System.Collections.Generic;
using NuSysApp;

namespace NuSysAp
{
    public class ListView<T>
    {
        /// <summary>
        /// This is the list of source items used in the list. For example, it could be the list of all 
        /// library element model ids.
        /// </summary>
        private List<T> _sourceItems;

        /// <summary>
        /// This is the list of
        /// </summary>
        private List<Win2dListViewItem> _listViewItems;
        public ListView()
        {
            
        }

        /// <summary>
        /// This function adds all the items from itemsToAdd in to the listview.  
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddItems(List<T> itemsToAdd)
        {
            
        }

        /// <summary>
        /// This adds a new column to the list where the value of the item in that specific row and column 
        /// can be created using the function passed in.
        /// </summary>
        /// <param name="convertFromTToListViewItem"></param>
        public void AddColumn(Func<T, Win2dListViewItem> convertFromTToListViewItem)
        {
            //For each of the items in 
        }


    }
}