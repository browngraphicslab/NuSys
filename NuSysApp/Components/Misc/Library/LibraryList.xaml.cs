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

        public LibraryList(LibraryView library, LibraryPageViewModel vm, LibraryElementPropertiesWindow propertiesWindow)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                ListView.ItemsSource = vm._PageElements;
                ((LibraryBucketViewModel)library.DataContext).OnNewContents += SetItems;
                ((LibraryBucketViewModel)library.DataContext).OnNewElementAvailable += AddNewElement;
            };
            _propertiesWindow = propertiesWindow;
            //Canvas.SetZIndex(Header, Canvas.GetZIndex(ListView)+1);
        }

        public ObservableCollection<NodeContentModel> GetItems()
        {
            return (ObservableCollection<NodeContentModel>)ListView.ItemsSource;
        }
        private void AddNewElement(NodeContentModel element)
        {
            //_items = new ObservableCollection<NodeContentModel>((IEnumerable<NodeContentModel>) ListView.ItemsSource);
            ((ObservableCollection<NodeContentModel>)ListView.ItemsSource).Add(element);
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


        public void SetItems(ICollection<NodeContentModel> elements)
        {
            ListView.ItemsSource = new ObservableCollection<NodeContentModel>(elements);
            ((LibraryPageViewModel) this.DataContext)._PageElements = new ObservableCollection<NodeContentModel>(elements);
        }

        /*
        private void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
             OnLibraryElementDrag?.Invoke(sender,e);
            /*
            var element = (LibraryElement) e.Items[0];
            e.Cancel = true;
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            rect.Width = 200;
            rect.Height = 200;
            view.ManipulationDelta += DraggingElementManipulation;
            
        }
    */

        private void DraggingElementManipulation(object sender, ManipulationDeltaRoutedEventArgs manipulationDeltaRoutedEventArgs)
        {
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            //Canvas.SetTop();
        }

        private void ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            _propertiesWindow.setTitle(((NodeContentModel)e.ClickedItem).ContentName);
            _propertiesWindow.setType(((NodeContentModel)e.ClickedItem).Type.ToString());
            //_propertiesWindow.Visibility = Visibility.Visible;
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

            var rect = view.LibraryDraggingRectangle;

            rect.Width = 200;

            rect.Height = 200;



            //Moves rectangle to position of click.

            _ct = new CompositeTransform();

            rect.RenderTransform = _ct;

            _x = e.GetCurrentPoint(view).Position.X;

            _y = e.GetCurrentPoint(view).Position.Y;

            _ct.TranslateX += _x - (rect.Width / 2);

            _ct.TranslateY += _y - (rect.Height / 2);



            //arbitrary z index

            Canvas.SetZIndex(rect, 3);

            Grid listViewGrid = (Grid)sender;

            listViewGrid.CapturePointer(e.Pointer);

            listViewGrid.PointerMoved += ListViewGrid_PointerMoved;

            listViewGrid.PointerReleased += ListViewGrid_PointerReleased;

            e.Handled = true;

        }



        private void ListViewGrid_PointerMoved(object sender, PointerRoutedEventArgs e)

        {

            var view = SessionController.Instance.SessionView;

            double dx = e.GetCurrentPoint(view).Position.X - _x;

            double dy = e.GetCurrentPoint(view).Position.Y - _y;



            _x = e.GetCurrentPoint(view).Position.X;

            _y = e.GetCurrentPoint(view).Position.Y;

            _ct.TranslateX += dx;

            _ct.TranslateY += dy;



            e.Handled = true;

        }







        private void ListViewGrid_PointerReleased(object sender, PointerRoutedEventArgs e)

        {

            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;



            rect.Width = 0;

            rect.Height = 0;



             var element = (NodeContentModel)((Grid)sender).DataContext;

            if (SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse != null)
            {
                var releasepoint =
                    SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(
                        new Point(_x, _y));

                Message m = new Message();

                m["contentId"] = element.Id;

                m["x"] = releasepoint.X - 200;

                m["y"] = releasepoint.Y - 200;

                m["width"] = 400;

                m["height"] = 400;

                m["nodeType"] = element.Type.ToString();

                m["autoCreate"] = true;

                m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;

                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));

            }

            e.Handled = true;

        }

        private void ListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _propertiesWindow.Visibility = Visibility.Visible;
        }
    }

}

  