using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp2
{
    public sealed partial class DataGrid : UserControl
    {
        private int _sortedIndex;
        private List<ColumnDefinition> _colDefinition;
        public DataGrid(DataGridViewModel vm, int sortedIndex, List<ColumnDefinition> colDefinition)
        {
            this.InitializeComponent();
            _sortedIndex = sortedIndex;
            

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                var mainGrid = (Grid)FindName("mainGrid");
                var headerGrid = (Grid)mainSP.FindName("headerGrid");
                var rg = (Grid)cellScrollViewer.FindName("rowGrid");
                if (colDefinition != null)
                {
                    int i = 0;
                    foreach (var columnDefinition in headerGrid.ColumnDefinitions)
                    {
                        columnDefinition.Width = colDefinition[i].Width;
                        i++;
                    }
                    int j = 0;
                    foreach (var columnDefinition in rg.ColumnDefinitions)
                    {
                        columnDefinition.Width = colDefinition[j].Width;
                        j++;
                    }
                }
                else
                {
                    foreach (var columnDefinition in headerGrid.ColumnDefinitions)
                    {
                        columnDefinition.Width = new GridLength(mainGrid.ActualWidth / headerGrid.ColumnDefinitions.Count);
                    }
                    
                    foreach (var columnDefinition in rg.ColumnDefinitions)
                    {
                        columnDefinition.Width = new GridLength(mainGrid.ActualWidth / rg.ColumnDefinitions.Count);
                    }
                }

                IList<ColumnDefinition> colDefList = new List<ColumnDefinition>(headerGrid.ColumnDefinitions.Count);
                foreach (var columnDefinition in headerGrid.ColumnDefinitions)
                {
                    colDefList.Add(columnDefinition);
                }
                _colDefinition = new List<ColumnDefinition>(headerGrid.ColumnDefinitions.Count);

            };

            this.DataContext = vm;

        }

        private void UIElement_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var fe = (Rectangle) sender;
            var harshheader = (GridHeader) fe.DataContext;
            var headerGrid = (Grid) FindName("headerGrid");            
            var contentPresenter = (ContentPresenter)((Grid) headerGrid).Children[harshheader.ColIndex];


            var elem = FindFirstChild<Grid>(contentPresenter);
            if(elem.ActualWidth + e.Delta.Translation.X <= 50)
            {
                return;
            }
            elem.Width = elem.ActualWidth + e.Delta.Translation.X;


            var rg = (Grid)cellScrollViewer.FindName("rowGrid");
            rg.ColumnDefinitions[harshheader.ColIndex].Width = new GridLength(elem.ActualWidth);
            headerGrid.ColumnDefinitions[harshheader.ColIndex].Width = new GridLength(elem.ActualWidth);

        }

        T FindFirstChild<T>(FrameworkElement element) where T : FrameworkElement
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            var children = new FrameworkElement[childrenCount];

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                children[i] = child;
                if (child is T)
                    return (T)child;
            }

            for (int i = 0; i < childrenCount; i++)
                if (children[i] != null)
                {
                    var subChild = FindFirstChild<T>(children[i]);
                    if (subChild != null)
                        return subChild;
                }

            return null;
        }

        private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeWestEast, 1);
        }

        private void Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void ScrollViewer_OnViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            var hT = (TranslateTransform)headerScrollViewer.FindName("headerTransform");
            hT.X = e.NextView.HorizontalOffset * (-1);
        }

        private void TextBlock_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            //do drop stuff here
           
        }

        private void ColGrid_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var index = Grid.GetColumn((Grid)sender);

            //((DataGridViewModel) this.DataContext).Sort(index);
        }


        private void TextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {            
            var textblock = (TextBlock)sender;
            GridHeader col = (GridHeader) textblock.DataContext;
            int colIndex = col.ColIndex;
            var currContext = (DataGridViewModel)this.DataContext;

            //group cells into rows
            List<ObservableCollection<GridRowCell>> rows = new List<ObservableCollection<GridRowCell>>();
            for(int i = 0; i < currContext.NumRows; i++)
            {
                ObservableCollection<GridRowCell> row = new ObservableCollection<GridRowCell>();
                for(int j = 0; j < currContext.NumCols; j++)
                {
                    row.Add(currContext.Data[i * currContext.NumCols + j]);
                }
                rows.Add(row);
            }

            //sort
            IOrderedEnumerable<ObservableCollection<GridRowCell>> sorted_rows;
            if (_sortedIndex == colIndex)
            {
                sorted_rows = rows.OrderByDescending(row => intCheck(row[colIndex].Title)).ThenByDescending(row => row[colIndex].Title);
            } else
            {
                sorted_rows = rows.OrderBy(row => intCheck(row[colIndex].Title)).ThenBy(row => row[colIndex].Title);
                _sortedIndex = colIndex;
            }
            
            ObservableCollection<GridRowCell> sorted_cells = new ObservableCollection<GridRowCell>();
            int k = 0;
            foreach(var row in sorted_rows)
            {
                for(int j = 0; j < currContext.NumCols; j++)
                {
                    sorted_cells.Add(new GridRowCell{
                        Title = row[j].Title,
                        RowIndex = k,
                        ColIndex = j
                    });
                }
                k++;
            }

            currContext.Data = sorted_cells;
            var headerGrid = (Grid)mainSP.FindName("headerGrid");
            if (_colDefinition != null)
            {
                _colDefinition.Clear();
            }
            foreach (var columnDefinition in headerGrid.ColumnDefinitions)
            {
                _colDefinition.Add(columnDefinition);
            }
            //_main.Reset(currContext, _sortedIndex, _colDefinition);
        }
        private int intCheck(string s)
        {
            int output;
            var success = int.TryParse(s, out output);
            return output;           
        }
    }
}
