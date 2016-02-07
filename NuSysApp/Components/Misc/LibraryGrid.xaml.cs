using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class LibraryGrid : UserControl
    {
        public ObservableCollection<LibraryElement> _items;
        private int _count = 0;
        public LibraryGrid(ObservableCollection<LibraryElement> items, LibraryView library)
        {
            this.InitializeComponent();

            _items = items;

            var numRows = 2;
            var numCols = 4;

            foreach (var item in _items)
            {
                LoadThumbnails(numRows, numCols, item);
            }
            library.OnNewContents += Library_OnNewContents;
            
        }

        private void Library_OnNewContents(ICollection<LibraryElement> elements)
        {
            var numRows = 2;
            var numCols = 4;

            foreach (var newItem in elements)
            {
                LoadThumbnails(numRows, numCols, newItem);
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

            TextBlock title = new TextBlock();
            title.Text = newItem.Title;
            TextBlock nodeType = new TextBlock();
            nodeType.Text = newItem.NodeType.ToString();
            TextBlock contentID = new TextBlock();
            contentID.Text = newItem.ContentID;

            itemPanel.Children.Add(title);
            itemPanel.Children.Add(nodeType);
            itemPanel.Children.Add(contentID);

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
