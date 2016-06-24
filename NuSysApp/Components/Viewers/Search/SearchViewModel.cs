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
        public ObservableCollection<SearchResultTemplate> PageElements { get; set; }
        private string _searchString = string.Empty;
        public double ResultWidth { get; set; }
        //private List<LibraryElementModel> _orgList;          

        public SearchViewModel()
        {
            PageElements = new ObservableCollection<SearchResultTemplate>();
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
                        
                        var searchResult = new SearchResult(result,model.Type,result);            
                        PageElements.Add(new SearchResultTemplate(searchResult));
                    }

                }
            }
        }
    }


}
