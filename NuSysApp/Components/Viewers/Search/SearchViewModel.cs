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
        private Visibility _noResultsFound;

        public Visibility NoResultsFound
        {
            get { return _noResultsFound; }
            set
            {
                _noResultsFound = value;
                RaisePropertyChanged("NoResultsFound");
            }
        }

        private Visibility _searchViewHelperTextVisibility;

        public Visibility SearchViewHelperTextVisibility
        {
            get { return _searchViewHelperTextVisibility; }
            set
            {
                _searchViewHelperTextVisibility = value;
                RaisePropertyChanged("SearchViewHelperTextVisibility");
            }
        }

        private Visibility _searchResultsListVisibility;

        public Visibility SearchResultsListVisibility
        {
            get { return _searchResultsListVisibility; }
            set
            {
                _searchResultsListVisibility = value;
                RaisePropertyChanged("SearchResultsListVisibility");
            }
        }

        public ObservableCollection<SearchResultTemplate> PageElements { get; set; }
        private string _searchString = string.Empty;
        private double _resultWidth;

        public double ResultWidth
        {
            get { return _resultWidth; }
            set
            {
                _resultWidth = value;
                RaisePropertyChanged("ResultWidth");
            }
        }
        public SearchViewModel()
        {
            PageElements = new ObservableCollection<SearchResultTemplate>();
            NoResultsFound = Visibility.Collapsed;
            SearchViewHelperTextVisibility = Visibility.Visible;
            SearchResultsListVisibility = Visibility.Collapsed;
        }

        public async void AdvancedSearch(Query searchQuery)
        {
            PageElements.Clear();        

            // remove from here
            PageElements.Add(new SearchResultTemplate(new SearchResult("7dd03643ef834684a8691be532b4fec5", SearchResult.ResultType.Type)));
            PageElements.Add(new SearchResultTemplate(new SearchResult("2e9fee2571094c05a1999b44e9aee36d", SearchResult.ResultType.Type)));
            SearchResultsListVisibility = Visibility.Visible;
            // to here



            /*
            var searchResults = await SessionController.Instance.NuSysNetworkSession.AdvancedSearchOverLibraryElements(searchQuery);
            if (searchResults == null || searchResults.Count == 0)
            {
                if (NoResultsFound == Visibility.Collapsed)
                {
                    NoResultsFound = Visibility.Visible;
                    SearchResultsListVisibility = Visibility.Collapsed;
                }
                return;
            }
            else
            {

                foreach (var result in searchResults)
                {
                    //var id = result.ContentID;
                    //if (id != null)
                    PageElements.Add(new SearchResultTemplate(result));

                }
                SearchResultsListVisibility = Visibility.Visible;
            }
            */
        }
     
    }


}
