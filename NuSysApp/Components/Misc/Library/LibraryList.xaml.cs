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
using MyToolkit.UI;
using Panel = Windows.Devices.Enumeration.Panel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryList : UserControl, LibraryViewable
    {
        
        private LibraryElementPropertiesWindow _propertiesWindow;

        private double _x;

        private double _y;

        private CompositeTransform _ct;
        private LibraryView _library;
        private Dictionary<string, bool> _reverseTable = new Dictionary<string, bool>();

        private bool _singleTap; 
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
            var itemlist = ((LibraryPageViewModel) DataContext).ItemList;
            itemlist.Clear();
            foreach (var libraryElementModel in elements)
            {
                var controller =
                    SessionController.Instance.ContentController.GetLibraryElementController(
                        libraryElementModel.LibraryElementId);
                itemlist.Add(new LibraryItemTemplate(controller));
            }
        }

        public async Task Sort(string s, bool reverse = false)
        {
            await ((LibraryPageViewModel)this.DataContext).Sort(s, reverse);
  //          this.SetItems(((LibraryPageViewModel)this.DataContext).PageElements);
        }

        public async Task Search(string s)
        {
            await ((LibraryPageViewModel)this.DataContext).Search(s);
            //this.SetItems(((LibraryPageViewModel)this.DataContext).PageElements);
        }

        public async void Update()
        {
            //this.SetItems(((LibraryPageViewModel)this.DataContext).ItemList);
        }

        private void LibraryListItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
             var view = SessionController.Instance.SessionView;
            _x = e.GetCurrentPoint(view).Position.X-25;
            _y = e.GetCurrentPoint(view).Position.Y-25;
        }
        //private void ListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        //{

        //    LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;

        //    SessionController.Instance.SessionView.ShowDetailView(SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId));
        //}
        private async void Sort_Button_Click(object sender, RoutedEventArgs e)
        {
            var btnStr = ((Button) sender).Content.ToString();
            if (_reverseTable.ContainsKey(btnStr))
                _reverseTable[btnStr] = !_reverseTable[btnStr];
            else
                _reverseTable.Add(btnStr, false);
            Sort(btnStr, _reverseTable[btnStr]);
        }
        private void LibraryListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LibraryItemTemplate itemTemplate = (LibraryItemTemplate)((Grid)sender).DataContext;
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(itemTemplate.ContentID);
            if ((SessionController.Instance.ActiveFreeFormViewer.ContentId == element.LibraryElementId) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }
            
            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.SetIcon(element);
            view.LibraryDraggingRectangle.Show();
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);
            rect.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)rect.RenderTransform;


            t.TranslateX += _x;
            t.TranslateY += _y;

            if (!SessionController.Instance.ContentController.ContainsAndLoaded(element.LibraryElementId))
            {
                Task.Run(async delegate
                {
                    SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(element.LibraryElementId);
                });
            }
        }


        private void LibraryListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            LibraryItemTemplate itemTemplate = (LibraryItemTemplate)((Grid)sender).DataContext;
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(itemTemplate.ContentID);
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var el = (FrameworkElement) sender;
            var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);
            
            var itemsBelow = VisualTreeHelper.FindElementsInHostCoordinates(sp, null).Where( i => i is LibraryView);
            if (itemsBelow.Any())
            {
                SessionController.Instance.SessionView.LibraryDraggingRectangle.Hide();
            }
            else
            {
                SessionController.Instance.SessionView.LibraryDraggingRectangle.Show();

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
            LibraryItemTemplate itemTemplate = (LibraryItemTemplate)((Grid)sender).DataContext;
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(itemTemplate.ContentID);
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;
           

            if (rect.Visibility == Visibility.Collapsed)
                return;

            rect.Hide();
            var r = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(_x, _y));
            await _library.AddNode(new Point(r.X, r.Y), new Size(300, 300), element.Type,element.LibraryElementId);
        }

        private void ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            return;
            
            var listItem = sender as ListView;
            
            var regionsPanel = listItem?.FindName("RegionsPanel") as Grid;
            
            if (regionsPanel?.Visibility == Visibility.Visible)
            {
                regionsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            regionsPanel?.RowDefinitions.Clear();
            var x = listItem.SelectedItem;
            var elementModel = ListView.SelectedItem as LibraryElementModel;
            var count = 0;

            if (elementModel?.Regions == null)
            {
                regionsPanel?.RowDefinitions.Add(new RowDefinition());
                var textBox = new TextBlock();
                textBox.Text = "No regions associated with this element.";
                regionsPanel?.Children.Add(textBox);
                Grid.SetRow(textBox, 0);
                regionsPanel.Visibility = Visibility.Visible;
                return;
            }
           
            foreach (var regionModel in elementModel.Regions)
            {
                regionsPanel?.RowDefinitions.Add(new RowDefinition());
                var textBox = new TextBlock();
                textBox.Text = regionModel.Name;
                regionsPanel?.Children.Add(textBox);
                Grid.SetRow(textBox, count);
                count++;
            }

            regionsPanel.Visibility = Visibility.Visible;
            
        }


        private void LibraryListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            
        }

        private async void HeaderPanel_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _singleTap = true;
            await Task.Delay(200);
            if (!_singleTap) return;

            var listItem = (Grid)sender;
            var regionsPanel = listItem.FindName("RegionsPanel") as Grid;

            if (regionsPanel?.Visibility == Visibility.Visible)
            {
                regionsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            regionsPanel?.RowDefinitions.Clear();
            regionsPanel?.Children.Clear();
            var elementTemplate = ListView.SelectedItem as LibraryItemTemplate; 
            var elementModel = SessionController.Instance.ContentController.GetContent(elementTemplate?.ContentID);

            if (elementModel?.Regions == null || elementModel?.Regions.Count == 0)
            {
                regionsPanel?.RowDefinitions.Add(new RowDefinition());
                var textBox = new TextBlock();
                textBox.Text = "No regions associated with this element.";
                textBox.FontSize = 13;
                regionsPanel?.Children.Add(textBox);
                textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                Grid.SetRow(textBox, 1);
                Grid.SetColumn(textBox, 1);
                regionsPanel.Visibility = Visibility.Visible;
                return;
            }

            //regionsPanel?.RowDefinitions.Add(new RowDefinition());
            //var panelTitle = new TextBlock();
            //panelTitle.Text = "Regions:";
            //regionsPanel?.Children.Add(panelTitle);
            //panelTitle.HorizontalAlignment = HorizontalAlignment.Left;
            //Grid.SetRow(panelTitle, 0);
            //Grid.SetColumn(panelTitle, 1);
            var count = 0;

            foreach (var regionModel in elementModel.Regions)
            {
                var regionController = SessionController.Instance.RegionsController.GetRegionController(regionModel.Id);
                var row = new RowDefinition();
                row.Height = GridLength.Auto;
                regionsPanel?.RowDefinitions.Add(row);
                var textBox = new Button();
                textBox.Content = regionModel.Name;
                regionController.TitleChanged += delegate
                {
                    textBox.Content = regionController.Model.Name;
                };
                textBox.FontSize = 13;
                if (textBox.IsPointerOver)
                {
                    textBox.Background = new SolidColorBrush(Colors.DarkOliveGreen);
                }
                textBox.DoubleTapped += delegate
                {
                    SessionController.Instance.SessionView.ShowDetailView(SessionController.Instance.ContentController.GetLibraryElementController(elementModel.LibraryElementId));
                    var controller = SessionController.Instance.RegionsController.GetRegionController(regionModel.Id);
                    SessionController.Instance.SessionView.ShowDetailView(controller);
                };
                regionsPanel?.Children.Add(textBox);
                textBox.HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetColumn(textBox, 1);
                Grid.SetRow(textBox, count);
                count++;
            }
            regionsPanel.Width = listItem.ActualWidth;
            regionsPanel.Visibility = Visibility.Visible;
        }

        private void HeaderPanel_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _singleTap = false;
            LibraryItemTemplate itemTemplate = (LibraryItemTemplate)((Grid)sender).DataContext;
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(itemTemplate.ContentID);
            SessionController.Instance.SessionView.ShowDetailView(SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId));
        }
    }

}

  