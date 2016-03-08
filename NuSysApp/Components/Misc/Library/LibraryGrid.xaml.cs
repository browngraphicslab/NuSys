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
using MyToolkit.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryGrid : UserControl, LibraryViewable
    {
        //public delegate void LibraryElementDragEventHandler(object sender, DragStartingEventArgs e);
        //public event LibraryElementDragEventHandler OnLibraryElementDrag;

        public ObservableCollection<LibraryElement> _items;
        private int _count = 0;

        private LibraryElementPropertiesWindow _propertiesWindow;

        private int _numRows;
        private double _x;

        private double _y;
        private CompositeTransform _ct;

        public LibraryGrid(LibraryView library, LibraryPageViewModel vm, LibraryElementPropertiesWindow propertiesWindow)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _items = vm._PageElements;

            _propertiesWindow = propertiesWindow;
            for (int i = 1; i < _items.Count/3 + 1; i++)
            {
                 xGrid.RowDefinitions.Add(new RowDefinition());
            }
            _numRows = _items.Count/3 + 1;
            var numCols = 3;
            
            foreach (var item in _items)
            {
                LoadThumbnails(_numRows, numCols, item);
            }
            ((LibraryBucketViewModel)library.DataContext).OnNewContents += Library_OnNewContents;
            
        }

        private void Library_OnNewContents(ICollection<LibraryElement> elements)
        {
            xGrid.RowDefinitions.Clear();
            for (int i = 1; i < elements.Count / 3 + 1; i++)
            {
                xGrid.RowDefinitions.Add(new RowDefinition());
            }
            _numRows = elements.Count / 3 + 1;
            var numCols = 3;

            foreach (var newItem in elements)
            {
                LoadThumbnails(_numRows, numCols, newItem);
            }
        }

        public async Task Search(string s)
        {
            await ((LibraryPageViewModel)this.DataContext).Search(s);

            var numCols = 3;
            _count = 0;
            xGrid.Children.Clear();
            foreach (var item in ((LibraryPageViewModel)this.DataContext)._PageElements)
            {
                LoadThumbnails(_numRows, numCols, item);
            }
        }

        public void SetItems(ICollection<LibraryElement> elements)
        {
            ((LibraryPageViewModel)this.DataContext)._PageElements = new ObservableCollection<LibraryElement>(elements);
        }

        public async Task Sort(string s)
        {
            await ((LibraryPageViewModel)this.DataContext).Sort(s);


            var numCols = 3;
            _count = 0;
            xGrid.Children.Clear();
            foreach (var item in ((LibraryPageViewModel)this.DataContext)._PageElements)
            {
                LoadThumbnails(_numRows, numCols, item);
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
            itemPanel.DoubleTapped += ItemPanel_DoubleTapped;
            itemPanel.Orientation = Orientation.Vertical;
            itemPanel.DataContext = newItem;

            //itemPanel.CanDrag = true;
            //itemPanel.DragStarting += delegate(UIElement a, DragStartingEventArgs b) { OnLibraryElementDrag?.Invoke(a, b); };

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

            wrappedView.DataContext = newItem;

            wrappedView.PointerPressed += GridItemOnPointerPressed;

            Grid.SetRow(wrappedView, _count / numCols);

            Grid.SetColumn(wrappedView, _count % numCols);

            xGrid.Children.Add(wrappedView);
            _count++;
        }

        private void GridItemOnPointerPressed(object sender, PointerRoutedEventArgs e)

        {

            var view = SessionController.Instance.SessionView;

            var rect = view.LibraryDraggingRectangle;

            rect.Width = 200;

            rect.Height = 200;



            Border wrappedView = (Border)sender;

            StackPanel itemPanel = (StackPanel)wrappedView.Child;



            //Moves rectangle to position of click.

            _ct = new CompositeTransform();

            rect.RenderTransform = _ct;

            _x = e.GetCurrentPoint(view).Position.X;

            _y = e.GetCurrentPoint(view).Position.Y;

            _ct.TranslateX += _x - (rect.Width / 2);

            _ct.TranslateY += _y - (rect.Height / 2);

            //arbitrary z index

            Canvas.SetZIndex(rect, 3);







            wrappedView.CapturePointer(e.Pointer);

            wrappedView.PointerMoved += wrappedView_PointerMoved;

            wrappedView.PointerReleased += wrappedView_PointerReleased;

            e.Handled = true;

        }

        private void wrappedView_PointerMoved(object sender, PointerRoutedEventArgs e)

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



        private void wrappedView_PointerReleased(object sender, PointerRoutedEventArgs e)

        {

            var view = SessionController.Instance.SessionView;

            var rect = view.LibraryDraggingRectangle;

            rect.Width = 0;

            rect.Height = 0;



            LibraryElement element = (LibraryElement)((Border)sender).DataContext;



            if (SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse != null)
            {
                var releasepoint =
                    SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(
                        new Point(_x, _y));

                Message m = new Message();

                m["contentId"] = element.ContentID;

                m["x"] = releasepoint.X - 200;

                m["y"] = releasepoint.Y - 200;

                m["width"] = 400;

                m["height"] = 400;

                m["nodeType"] = element.ElementType.ToString();

                m["autoCreate"] = true;

                m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;

                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));

            }

            e.Handled = true;

        }


        private void ItemPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            LibraryElement clickedElement = (LibraryElement)((StackPanel)sender).DataContext;
            _propertiesWindow.setTitle(clickedElement.Title);
            _propertiesWindow.setType(clickedElement.ElementType.ToString());
            _propertiesWindow.Visibility = Visibility.Visible;
        }

        public async Task Update()
        {
            this.SetItems(((LibraryPageViewModel)this.DataContext)._PageElements);
        }
    }
}
