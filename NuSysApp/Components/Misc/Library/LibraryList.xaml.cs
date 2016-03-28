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
           
           
            this.DataContext = vm;
            this.InitializeComponent();
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                ((LibraryBucketViewModel)library.DataContext).OnNewContents += SetItems;
            };
            ((LibraryBucketViewModel)library.DataContext).OnHighlightElement += Select;
            _propertiesWindow = propertiesWindow;
            _library = library;
            //vm.OnItemsChanged += Update;
            //Canvas.SetZIndex(Header, Canvas.GetZIndex(ListView)+1)
            foreach (var element in vm.PageElements.ToArray())
            {
                element.OnLightupContent += Select;
            }
        }

        private void Select(LibraryElementModel model, bool lightup = true)
        {
            ListView.SelectedItem = null;


            if (ListView.ItemsSource == null)
                return;

            if (lightup)
            {
                if ((ObservableCollection<LibraryElementModel>) ListView.ItemsSource != null &&
                    (((ObservableCollection<LibraryElementModel>) ListView.ItemsSource).Count ==
                     SessionController.Instance.ContentController.Count ||
                     ((ObservableCollection<LibraryElementModel>) ListView.ItemsSource).Contains(model)))
                {
                    ListView.SelectedItem = model;
                    ListView.ScrollIntoView(model);
                }
            }
        }

        public void SetItems(ICollection<LibraryElementModel> elements)
        {
            var col = ((LibraryPageViewModel) DataContext).PageElements;
            col.Clear();
            foreach (var libraryElementModel in elements)
            {
                col.Add(libraryElementModel);
            }
        }

        public async Task Sort(string s)
        {
            await ((LibraryPageViewModel)this.DataContext).Sort(s);
  //          this.SetItems(((LibraryPageViewModel)this.DataContext).PageElements);
        }

        public async Task Search(string s)
        {
            await ((LibraryPageViewModel)this.DataContext).Search(s);
            //this.SetItems(((LibraryPageViewModel)this.DataContext).PageElements);
        }

        public async void Update()
        {
            this.SetItems(((LibraryPageViewModel)this.DataContext).PageElements);
        }

        private void LibraryListItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            _x = e.GetCurrentPoint(view).Position.X;
            _y = e.GetCurrentPoint(view).Position.Y;

            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            element.FireLightupContent(true);
        }
        private void ListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            _propertiesWindow.SetElement(element);
            _propertiesWindow.Visibility = Visibility.Visible;

            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            element.FireLightupContent(true);
        }
        private async void Sort_Button_Click(object sender, RoutedEventArgs e)
        {
            string s = "nodetype";
            switch (((Button)sender).Content.ToString())
            {
                case "title":
                    s = "title";
                    break;
                case "date":
                    s = "timestamp";
                    break;
            }
            Sort(s);
        }
        private void LibraryListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            if ((SessionController.Instance.ActiveFreeFormViewer.ContentId == element.Id) || (element.Type == ElementType.Link))
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


         //   var t = (CompositeTransform)rect.RenderTransform;

      //      var wvm = SessionController.Instance.ActiveFreeFormViewer;
          //  var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(_x - 100, _y - 100, 200, 200));
            var r = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(_x, _y));
            await _library.AddNode(new Point(r.X, r.Y), new Size(300, 300), element.Type,element.Id);
        }

        private void ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            _propertiesWindow.SetElement(((LibraryElementModel)e.ClickedItem));         
        }
    }

}

  