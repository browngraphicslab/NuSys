using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// generic column class for the list view. specific column classes should extend from this and return more specific UIElements
    /// takes in a function in its constructor so it can get a value from a list item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListViewColumn<T>
    {
        /// <summary>
        /// function that will take in a generic class and return a rectangular ui element
        /// </summary>
        public Func<T, string> _func;
        public string _title;

        public ListViewColumn(Func<T, string> func, string title)
        {
            _func = func;
            _title = title;
        }
    }
}
