using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using MyToolkit.Collections;

namespace NuSysApp
{
    public class SearchViewModel : BaseINPC
    {
        public Visibility NoResultsFound;
        public ObservableCollection<SearchResult> PageElements { get; set; }
        private string _searchString = string.Empty;
        //private List<LibraryElementModel> _orgList;          

        public SearchViewModel()
        {
            PageElements = new ObservableCollection<SearchResult>();
            NoResultsFound = Visibility.Collapsed;
        }

        public async void AdvancedSearch(Query searchQuery)
        {
            PageElements.Clear();        

            var searchResults = await SessionController.Instance.NuSysNetworkSession.AdvancedSearchOverLibraryElements(searchQuery);
            if (searchResults == null)
            {
                if (NoResultsFound == Visibility.Collapsed)
                {
                    NoResultsFound = Visibility.Visible;
                }
                return;
            }
            else
            {
                foreach (var result in searchResults)
                {
                    //var id = result.ContentID;
                    //if (id != null)
                    if(!string.IsNullOrEmpty(result))
                    {
                        var model = SessionController.Instance.ContentController.Get(result);
                        
                        var searchResult = new SearchResult(result,model.Type,"extra info example");
                        searchResult.Title = model.Title;
                        searchResult.TimeStamp = model.Timestamp;                  
                        PageElements.Add(searchResult);
                    }

                }
            }
        }
    }


}
