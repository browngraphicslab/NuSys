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
using NusysIntermediate;

namespace NuSysApp
{
    public class LibraryFavoritesViewModel
    {
        //public ObservableCollection<LibraryElementModel> PageElements { get; set; }
        public ObservableCollection<LibraryItemTemplate> ItemList { get; set; } 
        private List<LibraryElementController> _controllerList;

        //private string _searchString = string.Empty;


        public delegate void ItemsChangedEventHandler(object sender, bool favorited);
        public event ItemsChangedEventHandler OnItemsChanged;
        


        public LibraryFavoritesViewModel(ObservableCollection<LibraryElementController> controllers)
        {
            SessionController.Instance.ContentController.ContentValues.Where(item => item.Favorited == true);
            ItemList = new ObservableCollection<LibraryItemTemplate>();
            //PageElements = elements.Where(item => item.Favorited == true);
            _controllerList = new List<LibraryElementController>(controllers);
            foreach(var controller in _controllerList)
            {
                SessionController.Instance.ContentController.GetLibraryElementController(controller.LibraryElementModel.LibraryElementId).Favorited += LibraryElementModel_OnFavorited;
                if (controller.LibraryElementModel.Favorited)
                    ItemList.Add(new LibraryItemTemplate(controller));
            };

            SessionController.Instance.ContentController.OnNewContent += NewContent;
            SessionController.Instance.ContentController.OnElementDelete += DeleteContent;
                
            


            // SessionController.Instance.ContentController.OnNewFavorite += NewFavorite;
        }

        private void LibraryElementModel_OnFavorited(object sender, bool favorited)
        {
            var element = (sender as LibraryElementController)?.LibraryElementModel;
            if (element == null)
            {
                return;
            }
            var controller =
                SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId);
            var template = new LibraryItemTemplate(controller);

            if (!ItemList.Contains(template))
            {
                ItemList.Add(template);
            }
            else
            {
                ItemList.Remove(template);
            }

           // OnItemsChanged?.Invoke(this, favorited);
        }

        

        private void NewContent(LibraryElementModel content)
        {
            UITask.Run(() =>
            {
                //if (content.Favorited == true)
                var controller = SessionController.Instance.ContentController.GetLibraryElementController(content.LibraryElementId);
                controller.Favorited += LibraryElementModel_OnFavorited;
                if (content.Favorited)
                {
                    LibraryElementModel_OnFavorited(content, true);
                }
                _controllerList.Add(controller);

                //Search(_searchString);
            });

        }

        
        private void DeleteContent(LibraryElementModel content)
        {
            UITask.Run(() =>
            {
                //_orgList.Remove(content);
                //var controller = SessionController.Instance.ContentController.GetLibraryElementController(content.LibraryElementId);
                //if (controller == null)
                //{
                    foreach (var item in ItemList.ToList())
                    {
                        if (item.ContentID == content.LibraryElementId)
                        {
                            ItemList.Remove(item);
                        }
                    }
                //}
                //ItemList.Remove(new LibraryItemTemplate(controller));
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
