using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    interface NextPageable<T>
    {
        /// <summary>
        /// Gets the next page of the search request
        /// </summary>
        /// <returns></returns>
        Task<List<T>> getNextPage();

        /// <summary>
        /// Gets the previous page of the search request
        /// </summary>
        /// <returns></returns>
        Task<List<T>> getPreviousPage();

        /// <summary>
        /// Should get the new pages based on the search request
        /// </summary>
        /// <param name="searchString">
        /// The search string</param>
        void MakeSearchRequest(String searchString);
    }
}
