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
        }

        public async void AdvancedSearch(Query searchQuery)
        {
            PageElements.Clear();        

            var searchResults = await SessionController.Instance.NuSysNetworkSession.AdvancedSearchOverLibraryElements(searchQuery);
            if (searchResults == null || searchResults.Count == 0)
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
                        var controller = SessionController.Instance.ContentController.GetLibraryElementController(result);
                        var model = controller.LibraryElementModel;
                        var searchResult = new SearchResult(result,model.Type,result);            
                        PageElements.Add(new SearchResultTemplate(searchResult));
                    }

                }
            }
        }
    }


}
