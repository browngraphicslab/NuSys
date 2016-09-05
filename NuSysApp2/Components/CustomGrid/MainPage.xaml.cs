﻿using System;
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
using NuSysApp;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyDataGrid
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.DataContext = this.MakeDummyInput(100, 4);
            ObservableCollection<ObservableCollection<string>> input = (ObservableCollection<ObservableCollection<string>>)this.DataContext;
            ObservableCollection<string> headerCollection = input[0];
            input.Remove(headerCollection);
            ObservableCollection<ObservableCollection<string>> elementCollection =
                (ObservableCollection<ObservableCollection<string>>) input;

            ObservableCollection<GridHeader> headers = new ObservableCollection<GridHeader>();
            ObservableCollection<GridRowCell> cells = new ObservableCollection<GridRowCell>();
            for (int i = 0; i < headerCollection.Count; i++)
            {
                var newHeader = new GridHeader { ColIndex = i, Title = headerCollection[i]};
                headers.Add(newHeader);
            }

            for (int i = 0; i < elementCollection.Count; i++)
            {
                var element = elementCollection[i];

                for (int j = 0; j < headerCollection.Count; j++)
                {
                    var newCell = new GridRowCell();
                    newCell.ColIndex = j;
                    newCell.RowIndex = i;
                    newCell.Title = element[1];
                    cells.Add(newCell);
                }
                
            }

            var vm = new DataGridViewModel();
            vm.Header = headers;
            vm.Data = cells;
            vm.NumCols = vm.Header.Count;
            vm.NumRows = vm.Data.Count/vm.Header.Count;
            DataContext = vm;
            this.InitializeComponent();

            var dg = new DataGrid(vm, -1, null);
            dg.Width = 500;
            dg.Height = 500;
            main.Children.Add(dg);

        }

        public ObservableCollection<GridRowCell> MakeDummyRows(int rowCount, int colCount)
        {
            var collection = new ObservableCollection<GridRowCell>();
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    var cell = new GridRowCell();
                    cell.Title = i.ToString() + j.ToString() + "this is too long to not overflow";
                    cell.RowIndex = i;
                    cell.ColIndex = j;
                    collection.Add(cell);

                }
            }
            return collection;
        }

        public ObservableCollection<ObservableCollection<string>> MakeDummyInput(int rowCount, int colCount)
        {
            ObservableCollection<ObservableCollection<string>> collection = new ObservableCollection<ObservableCollection<string>>();

            ObservableCollection<string> headCollection = new ObservableCollection<string>();
            headCollection.Add("Title");
            headCollection.Add("Date");
            headCollection.Add("Time");
            headCollection.Add("Location");

            collection.Add(headCollection);

            Random r = new Random();
            for (int i = 0; i < rowCount; i++)
            {
                ObservableCollection<string> data = new ObservableCollection<string>();                
                for (int j = 0; j < colCount; j++)
                {
                    data.Add(r.Next(0, 100).ToString());
                }

                collection.Add(data);
            }

            return collection;

        }

        //public void Reset(DataGridViewModel viewModel, int sortedIndex, List<ColumnDefinition> cols)
        //{
        //    main.Children.Clear();
        //    var dg = new DataGrid(viewModel, this, sortedIndex, cols);
        //    dg.Width = 500;
        //    dg.Height = 500;
        //    main.Children.Add(dg);
        //}

        public string makeString(Random random)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);

            return finalString;
        }
    }
}