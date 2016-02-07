using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryList : UserControl, LibraryViewable
    {
        public LibraryList(List<LibraryElement> items, LibraryView library)
        {
            this.InitializeComponent();
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                ListView.ItemsSource = new ObservableCollection<LibraryElement>(items);
                library.OnNewContents += SetItems;
                library.OnNewElementAvailable += AddNewElement;
            };

        }

        public ObservableCollection<LibraryElement> GetItems()
        {
            return (ObservableCollection<LibraryElement>)ListView.ItemsSource;
        }
        private void AddNewElement(LibraryElement element)
        {
            //_items = new ObservableCollection<LibraryElement>((IEnumerable<LibraryElement>) ListView.ItemsSource);
            ((ObservableCollection<LibraryElement>)ListView.ItemsSource).Add(element);
        }

        public async void Sort(string s)
        {
            IOrderedEnumerable<LibraryElement> ordered = null;
            switch (s.ToLower().Replace(" ", string.Empty))
            { 
                case "title":
                    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.Title);
                    break;
                case "nodetype":
                    ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.NodeType.ToString());
                    break;
                case "timestamp":
                    break;
                default:
                    break;
            }
            if (ordered != null)
            { 
                ObservableCollection<LibraryElement> newCollection = new ObservableCollection<LibraryElement>();
                await Task.Run(async delegate
                {
                    foreach (var item in ordered)
                    {
                        newCollection.Add(item);
                    }
                });
                ListView.ItemsSource = newCollection;
            }
        }
        public async void Search(string s)
        {
            ObservableCollection<LibraryElement> newCollection = new ObservableCollection<LibraryElement>();
            var coll = ((ObservableCollection<LibraryElement>) ListView.ItemsSource);
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
            ListView.ItemsSource = newCollection;
        }
        public void SetItems(ICollection<LibraryElement> elements)
        {
            ListView.ItemsSource = new ObservableCollection<LibraryElement>(elements);
        }

        private void UIElement_OnDragStarting(UIElement sender, DragStartingEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
