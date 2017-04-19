using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class PaginatedGoogleDriveSearch : NextPageable<String>
    {
        List<Page<String>> pages;
        int index = -1;

        public PaginatedGoogleDriveSearch()
        {
            pages = new List<Page<String>>();
        }

        public async Task<List<string>> getNextPage()
        {
            //If the list of pages has not been populated return empty list
            if(index == -1)
            {
                return new List<String>();
            }
            //If you are not viewing the last page we have, just get the next page from the list
            if(pages.Count > index)
            {
                List < String > items = pages.ElementAt(index).Items;
                index++;
                return items;

            }else
            {
                //If you are viewing the last page we have and the pages list is not empty, get the next page from the google api
                if (pages.Count != 0)
                {
                    var newPage = await GoogleDriveCommunicator.GetNextSearchPage(pages.Last().NextPageUrl);
                    pages.Add(newPage);
                    index++;
                    return newPage.Items;
                }else
                {
                    return new List<String>();
                }
            }
        }

        public async Task<List<string>> getPreviousPage()
        {
            var demo = new List<String>();
            demo.Add("Title 1");
            demo.Add("Title 2");
            demo.Add("Title 3");
            return demo;
        }

        public async void MakeSearchRequest(string searchString)
        {
            pages.Clear();
            var firstPage = await GoogleDriveCommunicator.SearchDrive(searchString);
            if (firstPage != null)
            {
                pages.Add(firstPage);
                index = 0;
            }
        }
    }
}
