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

namespace NuSysApp2 
{
    public class LibraryPageViewModel
    {
        // public ObservableCollection<LibraryElementModel> PageElements { get; set; }  TODO If LibraryGrid needs to be made functional, PageElements => ItemList
        public ObservableCollection<LibraryItemTemplate> ItemList { get; set; } 
        private List<LibraryElementController> _controllerList;

        private string _searchString = string.Empty;


        public delegate void ItemsChangedEventHandler();
        public event ItemsChangedEventHandler OnItemsChanged;
        public LibraryPageViewModel(ObservableCollection<LibraryElementController> controllers)
        {
            ItemList = new ObservableCollection<LibraryItemTemplate>();
            foreach (var controller in controllers)
            {
                var template = new LibraryItemTemplate(controller);
                ItemList.Add(template);   
            }
            _controllerList = new List<LibraryElementController>(controllers);
            SessionController.Instance.ContentController.OnNewContent += NewContent;
            SessionController.Instance.ContentController.OnElementDelete += DeleteContent;
        }

        private void NewContent(LibraryElementModel content)
        {
            UITask.Run(() =>
            {
                
                _controllerList.Add(SessionController.Instance.ContentController.GetLibraryElementController(content.LibraryElementId));
                Search(_searchString);
            });

        }

        private void DeleteContent(LibraryElementModel content)
        {
            UITask.Run(() =>
            {
                var controller =
                    SessionController.Instance.ContentController.GetLibraryElementController(content.LibraryElementId);
                _controllerList.Remove(controller);
                if (controller == null)
                {
                    var toRemove = new HashSet<LibraryItemTemplate>();

                    foreach (var item in ItemList)
                    {
                        if (item.ContentID == content.LibraryElementId)
                        {
                            toRemove.Add(item);
                        }
                    }
                    foreach (var item in toRemove)
                    {
                        ItemList.Remove(item);
                    }
                }
                //ItemList.Remove(new LibraryItemTemplate(controller));
            });
        }
        public async Task Sort(string s, bool reverse = false)
        {
            List<LibraryItemTemplate> ordered = null;
            switch (s)
            {
                //case "title":
                //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.Title);
                //    break;
                //case "nodetype":
                //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.NodeType.ToString());
                //    break;
                case "Title":
                    ordered = new List<LibraryItemTemplate>(ItemList.OrderBy(l => ((LibraryItemTemplate)l).Title));
                    break;
                case "Type":
                    ordered = new List<LibraryItemTemplate>(ItemList.OrderBy(l => ((LibraryItemTemplate)l).Type.ToString()));
                    break;
                case "Date/Time":
                    ordered = new List<LibraryItemTemplate>(ItemList.OrderByDescending(l => Constants.GetTimestampTicksOfLibraryElementModel((LibraryItemTemplate)l)));
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
                ItemList.Clear();

                foreach (var item in ordered)
                {
                    ItemList.Add(item);
                }
          
            }
        }
        public async Task Search(string s)
        {
            _searchString = s;
            ItemList.Clear();

            IEnumerable<string> valids;
            if (string.IsNullOrEmpty(s))
            {
                valids = SessionController.Instance.ContentController.IdList;
            }
            else
            {
                valids = await SessionController.Instance.NuSysNetworkSession.SearchOverLibraryElements(s);
            }
            if (valids == null)
            {
                return;
            }
            var hash = new HashSet<string>(ItemList.Select(item => item.ContentID));
            foreach (var item in _controllerList)
            {
                if (valids.Contains(item.LibraryElementModel.LibraryElementId) && !hash.Contains(item.LibraryElementModel.LibraryElementId))
                {
                    ItemList.Add(new LibraryItemTemplate(item));
                }
            }
        }

    }
}
