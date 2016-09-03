using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyToolkit.UI;
using NusysIntermediate;
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

        // for telling the undo button that the delete button has been clicked
        public delegate void DeleteClickedHandler(object sender, IUndoable action);

        public event DeleteClickedHandler DeleteClicked;


        // used to check if library list items are single tapped or double tapped
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

        public async Task Search(string s)
        {
            await ((LibraryPageViewModel)this.DataContext).Search(s);
            //this.SetItems(((LibraryPageViewModel)this.DataContext).PageElements);
        }

        private void LibraryListItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
             var view = SessionController.Instance.SessionView;
            _x = e.GetCurrentPoint(view).Position.X-25;
            _y = e.GetCurrentPoint(view).Position.Y-25;
        }

        /// <summary>
        /// This is really just setting the instantation point for dragging and dropping list items
        /// because they can be dragged and dropped from anywhere on the template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void regionButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            LibraryListItem_OnPointerPressed(sender, e);
        }

        private async void Sort_Button_Click(object sender, RoutedEventArgs e)
        {
            var btnStr = ((Button) sender).Name;
            if (_reverseTable.ContainsKey(btnStr))
            {
                _reverseTable[btnStr] = !_reverseTable[btnStr];
            } 
            else
            {
                _reverseTable.Add(btnStr, false);
            }
                
            Sort(btnStr, _reverseTable[btnStr]);
        }

        public async Task Sort(string s, bool reverse = false)
        {
            await ((LibraryPageViewModel)this.DataContext).Sort(s, reverse);
        }
        private void LibraryListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LibraryItemTemplate itemTemplate = (LibraryItemTemplate)((Grid)sender).DataContext;
            LibraryElementModel libraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(itemTemplate.LibraryElementId);


            if ((SessionController.Instance.ActiveFreeFormViewer.LibraryElementId == libraryElementModel.LibraryElementId) || (libraryElementModel.Type == NusysConstants.ElementType.Link))
            {
                e.Handled = true;
                return;
            }
            
            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.SetIcon(libraryElementModel);
            view.LibraryDraggingRectangle.Show();
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);
            rect.RenderTransform = new CompositeTransform();
            rect.Visibility = Visibility.Collapsed;
            var t = (CompositeTransform)rect.RenderTransform;


            t.TranslateX += _x;
            t.TranslateY += _y;

            if (!SessionController.Instance.ContentController.ContainsContentDataModel(libraryElementModel.ContentDataModelId))
            {
                Task.Run(async delegate
                {
                    if (libraryElementModel.Type == NusysConstants.ElementType.Collection)
                    {
                        var request = new GetEntireWorkspaceRequest(libraryElementModel.LibraryElementId);
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                        Debug.Assert(request.WasSuccessful() == true);
                        await request.AddReturnedDataToSessionAsync();
                        await request.MakeCollectionFromReturnedElementsAsync();
                    }
                    else
                    {
                        SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(libraryElementModel.ContentDataModelId);
                    }
                });
            }
        }


        private void LibraryListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            LibraryItemTemplate itemTemplate = (LibraryItemTemplate)((Grid)sender).DataContext;
            LibraryElementModel element = SessionController.Instance.ContentController.GetLibraryElementModel(itemTemplate.LibraryElementId);

            
            // get the pointer point position, and upper left corner of the libary in relation to the sessionview
            var el = (FrameworkElement)sender;
            var pointerPoint = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);
            var upperLeftPoint = xGrid.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(new Point(0, 0));

            // if the pointer point is in the library scroll the library
            if (pointerPoint.X > upperLeftPoint.X && pointerPoint.X < upperLeftPoint.X + this.ActualWidth &&
                pointerPoint.Y > upperLeftPoint.Y && pointerPoint.Y < upperLeftPoint.Y + this.ActualHeight)
            {
                Border border = (Border)VisualTreeHelper.GetChild(ListView, 0);
                ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                scrollViewer?.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta.Translation.Y);
            }
            

            if ((SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var itemsBelow = VisualTreeHelper.FindElementsInHostCoordinates(pointerPoint, null).Where( i => i is LibraryView);

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
            LibraryElementModel element = SessionController.Instance.ContentController.GetLibraryElementModel(itemTemplate.LibraryElementId);
            if ((SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;
           

            if (rect.Visibility == Visibility.Collapsed)
                return;

            var el = (FrameworkElement)sender;
            var pointerPoint = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);
            var upperLeftPoint = xGrid.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(new Point(0, 0));

            // if the pointer point is in the library dont create the element
            //This is the final check
            if (pointerPoint.X > upperLeftPoint.X && pointerPoint.X < upperLeftPoint.X + this.ActualWidth &&
                pointerPoint.Y > upperLeftPoint.Y && pointerPoint.Y < upperLeftPoint.Y + this.ActualHeight)
            {
                return;
            }

            rect.Hide();

            var r = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(_x, _y));
            
            //Before we add the node, we need to check if the access settings for the library element and the workspace are incompatible
            // If they are different we siply return 
            var currWorkSpaceAccessType =
                SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType;

            if (element.AccessType == NusysConstants.AccessType.Private &&
                currWorkSpaceAccessType == NusysConstants.AccessType.Public)
            {
                //SessionController.Instance.SessionView.ShowAccessInvalid();
                return;
            }

            var libraryElementController =
                SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId);
            var id = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
            var currCollectionAccess =
                SessionController.Instance.ContentController.GetLibraryElementController(id)
                    .LibraryElementModel.AccessType;
            libraryElementController.AddElementAtPosition(r.X, r.Y);
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
            var buttonPanel = listItem.FindName("ButtonPanel") as Grid;

            if (buttonPanel?.Visibility == Visibility.Visible)
            {
                buttonPanel.Visibility = Visibility.Collapsed;
                return;
            }
            buttonPanel.Width = listItem.ActualWidth;
            buttonPanel.Visibility = Visibility.Visible;

            /*
            buttonPanel?.RowDefinitions.Clear();
            buttonPanel?.Children.Clear();
            var elementTemplate = ListView.SelectedItem as LibraryItemTemplate;

            if (elementTemplate == null) // This become true when the item is deleted from the library
            {
                return;
            }
            var elementModel = SessionController.Instance.ContentController.GetLibraryElementModel(elementTemplate?.LibraryElementId);

            var regionIds = SessionController.Instance.RegionsController.GetClippingParentRegionLibraryElementIds(elementModel.LibraryElementId);

            if (regionIds == null || regionIds.Count == 0)
            {
                regionsPanel?.RowDefinitions.Add(new RowDefinition());
                var textBox = new TextBlock();
                textBox.Text = "No regions associated with this element.";
                textBox.Margin = new Thickness(2);
                textBox.FontSize = 13;
                regionsPanel?.Children.Add(textBox);
                textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                Grid.SetRow(textBox, 1);
                Grid.SetColumn(textBox, 1);
                regionsPanel.Visibility = Visibility.Visible;
                return;
            }

            regionsPanel?.RowDefinitions.Add(new RowDefinition());
            var regionTextBox = new TextBlock();
            regionTextBox.Text = "Regions:";
            regionTextBox.Margin = new Thickness(70,2,2,2);
            regionTextBox.FontSize = 13;
            regionTextBox.FontWeight = FontWeights.Bold;
            regionsPanel?.Children.Add(regionTextBox);
            regionTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetRow(regionTextBox, 1);
            Grid.SetColumn(regionTextBox, 1);

            var count = 1;
            

            foreach (var regionId in regionIds)
            {
                var regionController = SessionController.Instance.ContentController.GetLibraryElementController(regionId);
                var row = new RowDefinition();
                row.Height = GridLength.Auto;
                regionsPanel?.RowDefinitions.Add(row);
                var regionButton = new Button();
                regionButton.HorizontalAlignment = HorizontalAlignment.Right;
                regionButton.Background = new SolidColorBrush(Color.FromArgb(255, 199, 222, 222));
                regionButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 17, 61, 64));
                regionButton.Margin = new Thickness(140,0,0,2);
                regionButton.MinWidth = 250;
                regionButton.Content = regionController.LibraryElementModel.Title;
                regionController.TitleChanged += delegate
                {
                    regionButton.Content = regionController.LibraryElementModel.Title;
                };
                regionButton.FontSize = 13;
                if (regionButton.IsPointerOver)
                {
                    regionButton.Background = new SolidColorBrush(Colors.White);
                    regionButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 199, 222, 222));
                    regionButton.BorderThickness = new Thickness(1);
                }
                regionButton.DoubleTapped += delegate
                {
                    var controller = SessionController.Instance.ContentController.GetLibraryElementController(regionId);
                    SessionController.Instance.SessionView.ShowDetailView(controller);
                };
                regionButton.AddHandler(PointerPressedEvent, new PointerEventHandler(regionButton_PointerPressed), true);
                regionsPanel?.Children.Add(regionButton);
                regionButton.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(regionButton, 1);
                Grid.SetRow(regionButton, count);
                count++;
            }
            regionsPanel.Width = listItem.ActualWidth;
            regionsPanel.Visibility = Visibility.Visible;

            ListView.SelectionChanged += delegate
            {
                regionsPanel.Visibility = Visibility.Collapsed;
            };
            */
        }

        private void HeaderPanel_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            // set _singleTap to false to stop single tap event from occuring
            _singleTap = false;

            // get the item template from the sender
            var itemTemplate = (sender as Grid)?.DataContext as LibraryItemTemplate;
            // get the library element model using the content id
            var element = SessionController.Instance.ContentController.GetLibraryElementModel(itemTemplate?.LibraryElementId);
            // get the library element libraryElementController using the library element id
            var controller =
                SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId);
            // show the detail viewer
            SessionController.Instance.SessionView.ShowDetailView(controller);
        }
        
        private async void LibraryListItem_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //var grid = sender as Grid;
            //var timestamp = grid?.FindName("TimeStampBox") as TextBlock;
            //var delete = grid?.FindName("DeleteBox") as Button;
            //timestamp.Visibility = Visibility.Collapsed;
            //delete.Visibility = Visibility.Visible;
            //delete.IsHitTestVisible = true;

            //await Task.Delay(4000);

            //timestamp.Visibility = Visibility.Visible;
            //delete.Visibility = Visibility.Collapsed;
            //delete.IsHitTestVisible = false;

        }
        
        private void DeleteBox_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var deleteButton = sender as Button;
            var id = deleteButton?.DataContext as string;

            // get the currWorkSpaceId
            var currWorkSpaceId =
                SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId;
            Debug.Assert(currWorkSpaceId != null);

            // if we are deleting the currentWorkSpace, return
            if ( currWorkSpaceId == id)
            {
                return;
            }

            var model = SessionController.Instance.ContentController.GetLibraryElementController(id)?.LibraryElementModel;


            if (model == null)
            {
                return;
            }

            Task.Run(async delegate
            {
                var request = new DeleteLibraryElementRequest(id);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                request.DeleteLocally();
            });


            var args = new CreateNewLibraryElementRequestArgs();
            args.LibraryElementId = model.LibraryElementId;
            args.AccessType = model.AccessType;
            args.Favorited = model.Favorited;
            args.Keywords = model.Keywords;
            args.Title = model.Title;
            args.LibraryElementType = model.Type;
            args.ContentId = model.ContentDataModelId;
          
            //Creates action with CreateNewLibraryElementArgs -- this may seem weird, but it's becausse we need the args to undo the deletion
            var action = new DeleteLibraryElementAction(args);
            //Invokes event listened to by the LibraryView, which activates the undo button
            DeleteClicked?.Invoke(this, action);
        }
        
        private void CopyButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var copyButton = sender as Button;
            var id = copyButton?.DataContext as string;

            // Simply create a deep copy. See the function call for more.
            Task.Run(async delegate
            {
                StaticServerCalls.CreateDeepCopy(id);
            });
        }
    }

}

  