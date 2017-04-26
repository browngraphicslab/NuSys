using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// This is the class that represents an Google Drive Page.
    /// It contains the list of items to be displayed on that page.
    /// Also it contains whether or not their exists a next page, and if there is, it also has the nextpage url that can be used
    /// to query the google drive API.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Page<T>
    {
        /// <summary>
        /// Whether or not their is a next page
        /// </summary>
        public bool HasNextPage
        {
            get;
            private set;
        }
        
        /// <summary>
        /// The list of items on that page
        /// </summary>
        public List<T> Items
        {
            get;
            private set;
        }

        /// <summary>
        /// The url of the next page
        /// </summary>
        public String NextPageUrl {
            get;private set;
        }

        /// <summary>
        /// Create a new page that has a next page.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="nextPageUrl"></param>
        public Page(List<T> items, String nextPageUrl)
        {
            Items = items;
            NextPageUrl = nextPageUrl;
            HasNextPage = true;
        }

        /// <summary>
        /// Create a new page that doesnt have a next page.
        /// </summary>
        /// <param name="items"></param>
        public Page(List<T> items)
        {
            Items = items;
            HasNextPage = false;
        }
    }
}
