using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class LibraryFavoritesViewModel
    {
        public ObservableCollection<LibraryElementModel> PageElements { get; set; }

        private List<LibraryElementModel> _orgList;

        //private string _searchString = string.Empty;


        public delegate void ItemsChangedEventHandler(object sender, bool favorited);
        public event ItemsChangedEventHandler OnItemsChanged;
        


        public LibraryFavoritesViewModel(ObservableCollection<LibraryElementModel> elements)
        {
            SessionController.Instance.ContentController.Values.Where(item => item.Favorited == true);
            PageElements = new ObservableCollection<LibraryElementModel>();
            //PageElements = elements.Where(item => item.Favorited == true);
            _orgList = new List<LibraryElementModel>(elements);
            foreach(var element in _orgList)
            {
                element.OnFavorited += LibraryElementModel_OnFavorited;
                if (element.Favorited == true)
                    PageElements.Add(element);
            };

            SessionController.Instance.ContentController.OnNewContent += NewContent;
            SessionController.Instance.ContentController.OnElementDelete += DeleteContent;
                
            


            // SessionController.Instance.ContentController.OnNewFavorite += NewFavorite;
        }

        private void LibraryElementModel_OnFavorited(LibraryElementModel element, bool favorited)
        {


            if (!PageElements.Contains(element))
            {
                PageElements.Add(element);
            }
            else
            {
                PageElements.Remove(element);
            }

           // OnItemsChanged?.Invoke(this, favorited);
        }

        

        private void NewContent(LibraryElementModel content)
        {
            UITask.Run(() =>
            {
                //if (content.Favorited == true)
                content.OnFavorited += LibraryElementModel_OnFavorited;

                _orgList.Add(content);

                //Search(_searchString);
            });

        }

        
        private void DeleteContent(LibraryElementModel content)
        {
            UITask.Run(() =>
            {
                //_orgList.Remove(content);
                PageElements.Remove(content);
            });
        }

        /*
        public async Task Sort(string s)
        {
            List<LibraryElementModel> ordered = null;
            switch (s.ToLower().Replace(" ", string.Empty))
            {
                //case "title":
                //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.Title);
                //    break;
                //case "nodetype":
                //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.NodeType.ToString());
                //    break;
                case "title":
                    ordered = new List<LibraryElementModel>(PageElements.OrderBy(l => ((LibraryElementModel)l).Title));
                    break;
                case "nodetype":
                    ordered = new List<LibraryElementModel>(PageElements.OrderBy(l => ((LibraryElementModel)l).Type.ToString()));
                    break;
                case "timestamp":
                    ordered = new List<LibraryElementModel>(PageElements.OrderByDescending(l => ((LibraryElementModel)l).GetTimestampTicks()));
                    break;
                default:
                    break;
            }
            if (ordered != null)
            {

                //  ObservableCollection<LibraryElementModel> newCollection = new ObservableCollection<LibraryElementModel>();
                PageElements.Clear();

                foreach (var item in ordered)
                {
                    PageElements.Add(item);
                }

            }
        }
        */

        /*
        public async Task Search(string s)
        {
            _searchString = s;
            PageElements.Clear();

            foreach (var item in _orgList)
            {
                if (item.InSearch(s))
                {
                    PageElements.Add(item);
                }
            }
        }

    */

    }
}
