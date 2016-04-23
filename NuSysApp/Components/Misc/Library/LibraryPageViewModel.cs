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
    public class LibraryPageViewModel
    {
        public ObservableCollection<LibraryElementModel> PageElements { get; set; }

        private List<LibraryElementModel> _orgList;

        private string _searchString = string.Empty;


        public delegate void ItemsChangedEventHandler();
        public event ItemsChangedEventHandler OnItemsChanged;
        public LibraryPageViewModel(ObservableCollection<LibraryElementModel> elements)
        {
            PageElements = elements;
            _orgList = new List<LibraryElementModel>(elements);
            SessionController.Instance.ContentController.OnNewContent += NewContent;
            SessionController.Instance.ContentController.OnElementDelete += DeleteContent;
        }

        private void NewContent(LibraryElementModel content)
        {
            UITask.Run(() =>
            {
                if(content.Type!= ElementType.Link)
                    _orgList.Add(content);
                Search(_searchString);
            });

        }

        private void DeleteContent(LibraryElementModel content)
        {
            UITask.Run(() =>
            {
                _orgList.Remove(content);
                PageElements.Remove(content);
            });
        }
        public async Task Sort(string s, bool reverse = false)
        {
            List<LibraryElementModel> ordered = null;
            switch (s)
            {
                //case "title":
                //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.Title);
                //    break;
                //case "nodetype":
                //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.NodeType.ToString());
                //    break;
                case "Title":
                    ordered = new List<LibraryElementModel>(PageElements.OrderBy(l => ((LibraryElementModel)l).Title));
                    break;
                case "Type":
                    ordered = new List<LibraryElementModel>(PageElements.OrderBy(l => ((LibraryElementModel)l).Type.ToString()));
                    break;
                case "Date/Time":
                    ordered = new List<LibraryElementModel>(PageElements.OrderByDescending(l => ((LibraryElementModel)l).GetTimestampTicks()));
                    break;
                default:
                    break;
            }
            if (ordered != null)
            {
                if (reverse)
                {
                    ordered.Reverse();
                }
                //  ObservableCollection<LibraryElementModel> newCollection = new ObservableCollection<LibraryElementModel>();
                PageElements.Clear();

                foreach (var item in ordered)
                {
                    PageElements.Add(item);
                }
          
            }
        }
        public async Task Search(string s)
        {
            _searchString = s;
            PageElements.Clear();

            var valids = await SessionController.Instance.NuSysNetworkSession.SearchOverLibraryElements(s);
            if (valids == null)
            {
                return;
            }
            var hash = new HashSet<LibraryElementModel>(PageElements);
            foreach (var item in _orgList)
            {
                if (valids.Contains(item.Id) && !hash.Contains(item))
                {
                    PageElements.Add(item);
                }
            }
        }

    }
}
