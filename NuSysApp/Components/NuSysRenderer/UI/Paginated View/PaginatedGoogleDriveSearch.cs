using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class PaginatedGoogleDriveSearch : NextPageable<GoogleDriveFileResult>
    {
        //The list to keep track of all our pages
        List<Page<GoogleDriveFileResult>> pages;
        int index = -1;

        /// <summary>
        /// Creates a new paginated google drive search class
        /// </summary>
        public PaginatedGoogleDriveSearch()
        {
            pages = new List<Page<GoogleDriveFileResult>>();
        }

        /// <summary>
        /// Returns the next page by either looking in the list of pages (if we are not at the end of the list), or makes a
        /// call to google drive communicator which then fetches the next page from the API. If there is no next page, this returns null.
        /// </summary>
        /// <returns></returns>
        public async Task<List<GoogleDriveFileResult>> getNextPage()
        {
            //If no pages have been loaded return an empty string
            if (pages.Count == 0)
            {
                return new List<GoogleDriveFileResult>();
            }
            //If you are not viewing the last page we have, just get the next page from the list
            if (pages.Count > index + 1)
            {
                index++;
                List<GoogleDriveFileResult> items = pages.ElementAt(index).Items;
                return items;

            }else
            {
                //If you are viewing the last page we have and the pages list is not empty, get the next page from the google api
                if (pages.Count != 0)
                {
                    var currPage = pages.Last();
                    if (currPage.HasNextPage)
                    {
                        var newPage = await GoogleDriveCommunicator.GetFileSearchResult(pages.Last().NextPageUrl);
                        pages.Add(newPage);
                        index++;
                        return newPage.Items;
                    }
                    //If there is no more pages (because google said so)
                    return null;
                }else
                {
                    return new List<GoogleDriveFileResult>();
                }
            }
        }

        /// <summary>
        /// If you are not at the start of the list, this just returns the previous page in the list and adjusts the index.
        /// </summary>
        /// <returns></returns>
        public async Task<List<GoogleDriveFileResult>> getPreviousPage()
        {
            if(index > 0)
            {
                index--;
                return pages.ElementAt(index).Items;
            }
            return null;
        }

        /// <summary>
        /// Makes a request to the Google Api to get the first page, and adds that page to the page list.
        /// </summary>
        /// <param name="searchString"></param>
        public async void MakeSearchRequest(string searchString)
        {
            pages.Clear();
            index = -1;
            var firstPage = await GoogleDriveCommunicator.SearchDrive(searchString, 5);
            if (firstPage != null)
            {
                pages.Add(firstPage);
            }
        }
    }
}
