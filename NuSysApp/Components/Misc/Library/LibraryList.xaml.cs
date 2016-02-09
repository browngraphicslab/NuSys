﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
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
        public delegate void LibraryElementDragEventHandler(object sender, DragItemsStartingEventArgs e);
        public event LibraryElementDragEventHandler OnLibraryElementDrag;
        public LibraryList(List<LibraryElement> items, LibraryView library)
        {
            this.InitializeComponent();
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                ListView.ItemsSource = new ObservableCollection<LibraryElement>(items);
                library.OnNewContents += SetItems;
                library.OnNewElementAvailable += AddNewElement;
            };

            //Canvas.SetZIndex(Header, Canvas.GetZIndex(ListView)+1);
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


        private void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
             //OnLibraryElementDrag?.Invoke(sender,e);
            var element = (LibraryElement) e.Items[0];
            e.Cancel = true;
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            rect.Width = 200;
            rect.Height = 200;
            view.ManipulationDelta += DraggingElementManipulation;
        }

        private void DraggingElementManipulation(object sender, ManipulationDeltaRoutedEventArgs manipulationDeltaRoutedEventArgs)
        {
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            //Canvas.SetTop();
        }
    }
}
