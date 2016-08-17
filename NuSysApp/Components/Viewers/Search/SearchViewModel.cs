using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using MyToolkit.Collections;
using NusysIntermediate;

namespace NuSysApp
{
    public class SearchViewModel : BaseINPC
    {

        private Visibility _searchExportButtonVisibility;

        public Visibility SearchExportButtonVisibility
        {
            get { return _searchExportButtonVisibility; }
            set
            {
                _searchExportButtonVisibility = value;
                RaisePropertyChanged("SearchExportButtonVisibility");
            }
        }

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

        public SearchViewModel()
        {
            PageElements = new ObservableCollection<SearchResultTemplate>();
            NoResultsFound = Visibility.Collapsed;
            SearchViewHelperTextVisibility = Visibility.Visible;
            SearchResultsListVisibility = Visibility.Collapsed;
        }

        public async void AdvancedSearch(QueryArgs searchQuery)
        {
            // if the user has not submitted any characters, just show no results found and exit
            if (string.IsNullOrWhiteSpace(searchQuery.SearchString))
            {
                NoResultsFound = Visibility.Visible;
                SearchResultsListVisibility = Visibility.Collapsed;
                SearchExportButtonVisibility = Visibility.Collapsed;
                return;
            }

            // because sorting the actual observable collection breaks binding
            var tempPageElements = new ObservableCollection<SearchResultTemplate>();

            var searchResults = await SessionController.Instance.NuSysNetworkSession.AdvancedSearchOverLibraryElements(searchQuery);
            
            // if no search results were found, show no results found and hide add to collection and search results list 
            if (searchResults == null || searchResults.Count == 0)
            {
                NoResultsFound = Visibility.Visible;
                SearchResultsListVisibility = Visibility.Collapsed;
                SearchExportButtonVisibility = Visibility.Collapsed;
                return;
            }
            var idResult = new Dictionary<string, SearchResultTemplate>();

            foreach (var result in searchResults)
            {

                if (idResult.ContainsKey(result.LibraryElementId))
                {
                    //Todo possibly weight importance by result type
                    idResult[result.LibraryElementId].IncrementImportance();
                }
                else
                {
                    var template = new SearchResultTemplate(result);
                    if (template.LibraryElementId != null)
                    {
                        idResult.Add(template.LibraryElementId, template);
                        tempPageElements.Add(template);
                    }
                }
            }

            tempPageElements = new ObservableCollection<SearchResultTemplate>(tempPageElements.OrderByDescending(i => i.Importance));

            PageElements?.Clear();
            foreach (var tempElement in tempPageElements)
            {
                PageElements?.Add(tempElement);
            }


            SearchResultsListVisibility = Visibility.Visible;
            SearchExportButtonVisibility = Visibility.Visible;
        }
     
    }


}
