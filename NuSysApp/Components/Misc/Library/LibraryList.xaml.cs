using System;
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
    public sealed partial class LibraryList : UserControl, LibraryViewable
    {
        //public delegate void LibraryElementDragEventHandler(object sender, DragItemsStartingEventArgs e);
        //public event LibraryElementDragEventHandler OnLibraryElementDrag;
        private LibraryElementPropertiesWindow _propertiesWindow;

        private double _x;

        private double _y;

        private CompositeTransform _ct;
        private LibraryView _library;
        public LibraryList(LibraryView library, LibraryPageViewModel vm, LibraryElementPropertiesWindow propertiesWindow)
        {
            this.InitializeComponent();
            ListView.ItemsSource = new ObservableCollection<LibraryElementModel>();
            this.DataContext = vm;
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                ListView.ItemsSource = vm._PageElements;
                ((LibraryBucketViewModel)library.DataContext).OnNewContents += SetItems;
                ((LibraryBucketViewModel)library.DataContext).OnNewElementAvailable += AddNewElement;
            };
            _propertiesWindow = propertiesWindow;
            _library = library;
            //Canvas.SetZIndex(Header, Canvas.GetZIndex(ListView)+1)
            
        }

        public ObservableCollection<LibraryElementModel> GetItems()
        {
            return (ObservableCollection<LibraryElementModel>)ListView.ItemsSource;
        }
        private void AddNewElement(LibraryElementModel element)
        {
            //_items = new ObservableCollection<LibraryElementModel>((IEnumerable<LibraryElementModel>) ListView.ItemsSource);
            UITask.Run(delegate
            {
                ((ObservableCollection<LibraryElementModel>) ListView.ItemsSource).Add(element);
            });
        }


        //public async void Sort(string s)
        //{
        //    IOrderedEnumerable<LibraryElement> ordered = null;
        //    switch (s.ToLower().Replace(" ", string.Empty))
        //    { 
        //        case "title":
        //            ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.Title);
        //            break;
        //        case "nodetype":
        //            ordered = ((ObservableCollection<LibraryElement>)ListView.ItemsSource).OrderBy(l => l.NodeType.ToString());
        //            break;
        //        case "timestamp":
        //            break;
        //        default:
        //            break;
        //    }
        //    if (ordered != null)
        //    { 
        //        ObservableCollection<LibraryElement> newCollection = new ObservableCollection<LibraryElement>();
        //        await Task.Run(async delegate
        //        {
        //            foreach (var item in ordered)
        //            {
        //                newCollection.Add(item);
        //            }
        //        });
        //        ListView.ItemsSource = newCollection;
        //    }
        //}
        //public async void Search(string s)
        //{
        //    ObservableCollection<LibraryElement> newCollection = new ObservableCollection<LibraryElement>();
        //    var coll = ((ObservableCollection<LibraryElement>) ListView.ItemsSource);
        //    await Task.Run(async delegate
        //    {
        //        foreach (var item in coll)
        //        {
        //            if (item.InSearch(s))
        //            {
        //                newCollection.Add(item);
        //            }
        //        }
        //    });
        //    ListView.ItemsSource = newCollection;
        //}


        public void SetItems(ICollection<LibraryElementModel> elements)
        {
            ListView.ItemsSource = new ObservableCollection<LibraryElementModel>(elements);
            ((LibraryPageViewModel) this.DataContext)._PageElements = new ObservableCollection<LibraryElementModel>(elements);
        }

        public async Task Sort(string s)
        {
            await ((LibraryPageViewModel)this.DataContext).Sort(s);
            this.SetItems(((LibraryPageViewModel)this.DataContext)._PageElements);
        }

        public async Task Search(string s)
        {
            await ((LibraryPageViewModel)this.DataContext).Search(s);
            this.SetItems(((LibraryPageViewModel)this.DataContext)._PageElements);
        }

        public async Task Update()
        {
            this.SetItems(((LibraryPageViewModel)this.DataContext)._PageElements);
        }

        private void LibraryListItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            _x = e.GetCurrentPoint(view).Position.X;
            _y = e.GetCurrentPoint(view).Position.Y;

        }
        private void ListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            _propertiesWindow.setTitle(element.Title);
            _propertiesWindow.setType(element.Type.ToString());
            _propertiesWindow.setID(element.Id);
            _propertiesWindow.Visibility = Visibility.Visible;

            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;

        }

        private void LibraryListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            if ((WaitingRoomView.InitialWorkspaceId == element.Id) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }


            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);
            rect.Width = 200;
            rect.Height = 200;
            rect.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)rect.RenderTransform;


            t.TranslateX += _x - (rect.Width / 2);
            t.TranslateY += _y - (rect.Height / 2);

            if (!SessionController.Instance.ContentController.ContainsAndLoaded(element.Id))
            {
                Task.Run(async delegate
                {
                    SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(element.Id);
                });
            }
        }


        private void LibraryListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            if ((WaitingRoomView.InitialWorkspaceId == element.Id) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            var t = (CompositeTransform)rect.RenderTransform;

            t.TranslateX += e.Delta.Translation.X;
            t.TranslateY += e.Delta.Translation.Y;

            _x += e.Delta.Translation.X;
            _y += e.Delta.Translation.Y;

            _propertiesWindow.Visibility = Visibility.Collapsed;


        }

        private async void LibraryListItem_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            if ((WaitingRoomView.InitialWorkspaceId == element.Id) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;
            rect.Width = 0;
            rect.Height = 0;


            var t = (CompositeTransform)rect.RenderTransform;

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(_x - 100, _y - 100, 200, 200));
            await _library.AddNode(new Point(r.X, r.Y), new Size(r.Width, r.Height), element.Type,element.Id);
        }

        private void ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            _propertiesWindow.setTitle(((LibraryElementModel)e.ClickedItem).Title);
            _propertiesWindow.setType(((LibraryElementModel)e.ClickedItem).Type.ToString());
            //_propertiesWindow.Visibility = Visibility.Visible;
           
        }
    }

}

  