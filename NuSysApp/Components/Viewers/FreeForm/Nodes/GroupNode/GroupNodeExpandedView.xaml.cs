﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class GroupNodeExpandedView : AnimatableUserControl
    {
        private int _count = 0;
        private GroupItemThumbFactory _factory;

        public GroupNodeExpandedView()
        {
            this.InitializeComponent();

            _factory = new GroupItemThumbFactory();

            Loaded += delegate 
            {
                if (DataContext is GroupNodeViewModel) { 
                    var vm = (GroupNodeViewModel)DataContext;

                    var numRows = 2;
                    var numCols = 4;

                    foreach (var myItem in vm.AtomViewList)
                    {
                        LoadThumbnails(numRows, numCols, myItem);
                    }
                    vm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
                }
            };
            

        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;

            var numRows = 2;
            var numCols = 4;

            foreach (var newItem in e.NewItems)
            {
                var child = (FrameworkElement) newItem;
                var childModel = (child.DataContext as GroupItemViewModel).Controller;
                var view = await _factory.CreateFromSendable(childModel);
                var wrappedView = new Border();
                wrappedView.Padding = new Thickness(10);
                wrappedView.Child = view;
                Grid.SetRow(wrappedView, _count / numCols);
                Grid.SetColumn(wrappedView, _count % numCols);
                xGrid.Children.Add(wrappedView);
                _count++;
            }
        }

        private async void LoadThumbnails(int numRows, int numCols, object myObj)
        {
            var child = (FrameworkElement)myObj;
            var childModel = (child.DataContext as GroupItemViewModel).Controller;
            var view = await _factory.CreateFromSendable(childModel);
            var wrappedView = new Border();
            wrappedView.Padding = new Thickness(10);
            wrappedView.Child = view;
            Grid.SetRow(wrappedView, _count / numCols);
            Grid.SetColumn(wrappedView, _count % numCols);
            xGrid.Children.Add(wrappedView);
            _count++;
        }

        private async  void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            
        }
    }
}
