﻿using System;
using System.Collections.Generic;
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

namespace MyDataGrid
{
    public sealed partial class DataGrid : UserControl
    {
        public DataGrid(DataGridViewModel vm)
        {
            this.InitializeComponent();
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                var mainGrid = (Grid)FindName("mainGrid");
                var headerGrid = (Grid)mainSP.FindName("headerGrid");
                foreach (var columnDefinition in headerGrid.ColumnDefinitions)
                {
                    columnDefinition.Width = new GridLength(mainGrid.ActualWidth / headerGrid.ColumnDefinitions.Count);
                }
                var rg = (Grid)scrollViewer.FindName("rowGrid");
                foreach (var columnDefinition in rg.ColumnDefinitions)
                {
                    columnDefinition.Width = new GridLength(mainGrid.ActualWidth / rg.ColumnDefinitions.Count);
                }

            };

           

            
            
        }

        private void UIElement_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var fe = (Rectangle) sender;
            var harshheader = (HarshHeader) fe.DataContext;
          //  Debug.WriteLine(harshheader.Title);
          //  Debug.WriteLine(harshheader.ColIndex);

            var headerGrid = FindName("headerGrid");
            var contentPresenter = (ContentPresenter)((Grid) headerGrid).Children[harshheader.ColIndex];


            var elem = FindFirstChild<Grid>(contentPresenter);
            elem.Width = elem.ActualWidth + e.Delta.Translation.X;

            var rg = (Grid)scrollViewer.FindName("rowGrid");
            rg.ColumnDefinitions[harshheader.ColIndex].Width = new GridLength(elem.ActualWidth);

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
    }
}
