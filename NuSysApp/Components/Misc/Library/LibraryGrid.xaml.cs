using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryGrid : UserControl, LibraryViewable
    {
        public delegate void LibraryElementDragEventHandler(object sender, DragStartingEventArgs e);
        public event LibraryElementDragEventHandler OnLibraryElementDrag;

        public ObservableCollection<LibraryElement> _items;
        private int _count = 0;
        public LibraryGrid(ObservableCollection<LibraryElement> items, LibraryView library)
        {
            this.InitializeComponent();

            _items = items;

            var numRows = 8;
            var numCols = 3;

            foreach (var item in _items)
            {
                LoadThumbnails(numRows, numCols, item);
            }
            library.OnNewContents += Library_OnNewContents;
            
        }

        private void Library_OnNewContents(ICollection<LibraryElement> elements)
        {
            var numRows = 8;
            var numCols = 3;

            foreach (var newItem in elements)
            {
                LoadThumbnails(numRows, numCols, newItem);
            }
        }

        public async void Search(string s)
        {
            ObservableCollection<LibraryElement> newCollection = new ObservableCollection<LibraryElement>();
            var coll = _items;
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
            _items = newCollection;

            var numRows = 8;
            var numCols = 3;
            _count = 0;
            xGrid.Children.Clear();
            foreach (var item in _items)
            {
                LoadThumbnails(numRows, numCols, item);
            }
        }

        public void SetItems(ICollection<LibraryElement> elements)
        {
            _items = new ObservableCollection<LibraryElement>(elements);
        }

        public async void Sort(string s)
        {

            IOrderedEnumerable<LibraryElement> ordered = null;
            switch (s.ToLower().Replace(" ", string.Empty))
            {
                case "title":
                    ordered = ((ObservableCollection<LibraryElement>)_items).OrderBy(l => l.Title);
                    break;
                case "nodetype":
                    ordered = ((ObservableCollection<LibraryElement>)_items).OrderBy(l => l.ElementType.ToString());
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
                _items = newCollection;
            }

            var numRows = 8;
            var numCols = 3;
            _count = 0;
            xGrid.Children.Clear();
            foreach (var item in _items)
            {
                LoadThumbnails(numRows, numCols, item);
            }
        }

        //public void addItems()
        //{
        //    foreach (LibraryElement item in _items)
        //    {
        //        Border wrapping = new Border();
        //        wrapping.Padding = new Thickness(10);

        //        wrapping.Child = ;

        //    }

        //    int count = 0;
        //    int numRows = 5;
        //    int numCols = 5;
        //    List<FrameworkElement> children = new List<FrameworkElement>();

        //    for (int i = 0; i < numRows; i++)
        //    {
        //        for (int j = 0; j < numCols; j++)
        //        {
        //            var wrapping = children[count];
        //            Grid.SetRow(wrapping, i);
        //            Grid.SetColumn(wrapping, j);
        //            xGrid.Children.Add(wrapping);
        //            count++;
        //        }
        //    }
        //}

        private async void LoadThumbnails(int numRows, int numCols, LibraryElement newItem)
        {

            StackPanel itemPanel = new StackPanel();
            itemPanel.Orientation = Orientation.Vertical;

            itemPanel.CanDrag = true;
            itemPanel.DragStarting += delegate(UIElement a, DragStartingEventArgs b) { OnLibraryElementDrag?.Invoke(a, b); };

            if (newItem.ElementType == ElementType.Image)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://wiki.tripwireinteractive.com/images/4/47/Placeholder.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.ElementType == ElementType.Text)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://findicons.com/files/icons/1580/devine_icons_part_2/512/defult_text.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.ElementType == ElementType.Web)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://www.clker.com/cliparts/I/Y/4/e/m/C/internet-icon-md.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.ElementType == ElementType.PDF)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://iconizer.net/files/Devine_icons/orig/PDF.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.ElementType == ElementType.Audio)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://icons.iconarchive.com/icons/icons8/windows-8/512/Music-Audio-Wave-icon.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.ElementType == ElementType.Video)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://www.veryicon.com/icon/ico/System/Icons8%20Metro%20Style/Photo%20Video%20Camcoder%20pro.ico", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }



            if (newItem.Title != null)
            {
                TextBlock title = new TextBlock();
                title.Text = newItem.Title;
                itemPanel.Children.Add(title);
            }

         
                TextBlock nodeType = new TextBlock();
                nodeType.Text = newItem.ElementType.ToString();
                itemPanel.Children.Add(nodeType);
            

            //if (newItem.ContentID != null)
            //{

            //    TextBlock contentID = new TextBlock();
            //    contentID.Text = newItem.ContentID;
            //    itemPanel.Children.Add(contentID);

            //}
           
            
            var wrappedView = new Border();
            wrappedView.Padding = new Thickness(10);
            wrappedView.Child = itemPanel;
            Grid.SetRow(wrappedView, _count / numCols);
            Grid.SetColumn(wrappedView, _count % numCols);
            xGrid.Children.Add(wrappedView);
            _count++;
        }
    }
}
