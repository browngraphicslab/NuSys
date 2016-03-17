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
        public ObservableCollection<LibraryElementModel> _PageElements;

        public LibraryPageViewModel(ObservableCollection<LibraryElementModel> elements)
        {
            _PageElements = elements;
            SessionController.Instance.ContentController.OnNewContent += NewContent;
        }

        private void NewContent(LibraryElementModel content)
        {
            _PageElements.Add(content);
        }
        public async Task Sort(string s)
        {
            IOrderedEnumerable<LibraryElementModel> ordered = null;
            switch (s.ToLower().Replace(" ", string.Empty))
            {
                //case "title":
                //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.Title);
                //    break;
                //case "nodetype":
                //    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.NodeType.ToString());
                //    break;
                case "title":
                    ordered = _PageElements.OrderBy(l => ((LibraryElementModel)l).Title);
                    break;
                case "nodetype":
                    ordered = _PageElements.OrderBy(l => ((LibraryElementModel)l).Type.ToString());
                    break;
                case "timestamp":
                    break;
                default:
                    break;
            }
            if (ordered != null)
            {
                ObservableCollection<LibraryElementModel> newCollection = new ObservableCollection<LibraryElementModel>();
                await Task.Run(async delegate
                {
                    foreach (var item in ordered)
                    {
                        newCollection.Add(item);
                    }
                });
                _PageElements = newCollection;
            }
        }
        public async Task Search(string s)
        {
            ObservableCollection<LibraryElementModel> newCollection = new ObservableCollection<LibraryElementModel>();
            var coll = ((ObservableCollection<LibraryElementModel>)_PageElements);
            await Task.Run(async delegate
            {
                foreach (var item in coll)
                {
                    if (item.InSearch(s))
                    {
                        newCollection.Add(item);
                    }
                }
            });
            _PageElements = newCollection;
        }

    }
}
