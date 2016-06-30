using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using MyToolkit.UI;

namespace NuSysApp
{
    public partial class SearchView : AnimatableUserControl
    {
        public string Title { get; set; }
        public string Date { get; set; }

        public UserControl View { get; set; }

        private SearchViewModel _vm;

        private double _x;
        private double _y;
        private bool _isSingleTap;

        public SearchView()
        {
            this.InitializeComponent();

            DataContextChanged += delegate (FrameworkElement sender, DataContextChangedEventArgs args)
            {

                if (!(DataContext is SearchViewModel))
                    return;
                _vm = (SearchViewModel)DataContext;

                // set the view equal to the size of the window
                this.ResizeView(true, true);
                // when the size of the winow changes reset the view
                SessionController.Instance.SessionView.SizeChanged += SessionView_SizeChanged;

                SessionController.Instance.SessionView.MainCanvas.PointerPressed += MainCanvas_PointerPressed;
                // Metadata.ItemsSource = vm.Metadata;
            };

        }

        #region manipulation code

        // when the size of the winow changes reset the view
        private void SessionView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // resize the height only
            this.ResizeView(false, true);
        }

        // sets the view equal to the size of the window
        private void ResizeView(bool width, bool height)
        {
            // resize width
            if (width)
            {
                this.Width = SessionController.Instance.SessionView.ActualWidth / 4;
            }
            // resize height
            if (height)
            {
                this.Height = SessionController.Instance.SessionView.ActualHeight;
            }

            // do this every time
            this.MaxHeight = SessionController.Instance.SessionView.ActualHeight;
            this.MaxWidth = SessionController.Instance.SessionView.ActualWidth - resizer.ActualWidth - 30;
            this.MinWidth = resizer.ActualWidth;
            _vm.ResultWidth = this.Width - resizer.Width;
            Canvas.SetTop(this, 0);
            Canvas.SetLeft(this, 0);
        }

        private void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var mainCanvas = SessionController.Instance.SessionView.MainCanvas;
            // return if the pointer is pressed inside the grid
            var position = e.GetCurrentPoint(mainCanvas).Position;
            if (position.X <= this.Width)
                return;

            Visibility = Visibility.Collapsed;

        }

        #endregion manipulation code

        #region search bar text manipulation
        // when the user enters text update the suggestion list
        private void SearchBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (_vm.NoResultsFound == Visibility.Visible)
            {
                _vm.NoResultsFound = Visibility.Collapsed;
            }

            // only get results when the user is typing
            // otherwise assume result was filled in by TextMemberPath
            // or the handler for SuggestionChosen
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Set the ItemsSource to be your filtered dataset
                // sender.ItemsSource = dataset;

                // set the ItemTemplate to customize the look of each item in list
                //sender.ItemTemplate = itemTemplate;

                // set the DisplayMemberPath to choose which property from your object to display in list
                //sender.DisplayMemberPath = displayMemberPath;

                // if no results are found set a single-line
                // sender.ItemsSource = "No results"
            }
        }

        // when the user chooses a suggestion in the suggestions list, update the textbox
        private void SearchBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set TextMemberPath property to choose which property from your text to display in textbox
            // if you do this the textbox updates automatically, should be the same as DisplayMemberPath
            // sender.TextMemberPath = textMemberPath

            // if you need more than a simple property You can use args.SelectedItem to build your text string.
            //sender.Text;        
        }

        private void HideHelperText()
        {
            _vm.SearchViewHelperTextVisibility = Visibility.Collapsed;
            xContainer.RowDefinitions.Clear();
            xContainer.RowDefinitions.Add(new RowDefinition());
            Grid.SetRow(SearchBarGrid, 0);
            ShowHelpButton.Visibility = Visibility.Visible;
        }

        private void ShowHelperText()
        {
            ShowHelpButton.Visibility = Visibility.Collapsed;
            _vm.NoResultsFound = Visibility.Collapsed;
            _vm.PageElements.Clear();
            _vm.SearchViewHelperTextVisibility = Visibility.Visible;
            xContainer.RowDefinitions.Add(new RowDefinition());
            xContainer.RowDefinitions.Add(new RowDefinition());
            Grid.SetRow(TitleBlock, 0);
            Grid.SetRow(SearchBarGrid, 1);
            Grid.SetRow(HelperText, 2);
            xContainer.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            xContainer.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
            xContainer.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
            _vm.SearchResultsListVisibility = Visibility.Collapsed;
            //TODO reset searchbar
        }

        // when the user submits a query show the query results
        private void SearchBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            HideHelperText();
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                // Use args.QueryText to determine what to do.
                _vm.AdvancedSearch(new Query(args.QueryText));
            }
        }
        #endregion search bar text manipulation

        private async void RootGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // check to see if double tap gets called
            _isSingleTap = true;
            await Task.Delay(200);
            if (!_isSingleTap) return;

            var header = sender as Grid;
            var info = header?.FindName("ResultInfo") as FrameworkElement;
            if (info == null) return;
            // if the extra info is open, close it, and return to normal color
            if (info.Visibility == Visibility.Visible)
            {
                info.Visibility = Visibility.Collapsed;
            }
            else
            {
                info.Visibility = Visibility.Visible;
            }
        }

        private void ListView_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _isSingleTap = false;
            var item = ListView.SelectedItem as SearchResultTemplate;
            var id = item.Id;

            var controller = SessionController.Instance.ContentController.GetLibraryElementController(id);
            
            SessionController.Instance.SessionView.ShowDetailView(controller);

            e.Handled = true;
        }

        private void ListItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            _x = e.GetCurrentPoint(view).Position.X - 25;
            _y = e.GetCurrentPoint(view).Position.Y - 25;
        }

        private void ListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            SearchResultTemplate result = (sender as Grid)?.DataContext as SearchResultTemplate;
            LibraryElementModel element = result?.Model;
            if ((SessionController.Instance.ActiveFreeFormViewer.ContentId == element.LibraryElementId) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.SwitchType(element.Type);
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


        private void ListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            SearchResultTemplate result = (sender as Grid)?.DataContext as SearchResultTemplate;
            LibraryElementModel element = result?.Model;
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);

            var itemsBelow = VisualTreeHelper.FindElementsInHostCoordinates(sp, null).Where(i => i is LibraryView);
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

        }

        private async void ListItem_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            SearchResultTemplate result = (sender as Grid)?.DataContext as SearchResultTemplate;
            LibraryElementModel element = result?.Model;
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

            if (_x < this.Width) return;

            await AddNode(new Point(r.X, r.Y), new Size(300, 300), element.Type, element.LibraryElementId);
        }

        public async Task AddNode(Point pos, Size size, ElementType elementType, string libraryId)
        {
            Task.Run(async delegate
            {
                if (elementType != ElementType.Collection)
                {
                    var element = SessionController.Instance.ContentController.GetContent(libraryId);
                    var dict = new Message();
                    Dictionary<string, object> metadata;

                    metadata = new Dictionary<string, object>();
                    metadata["node_creation_date"] = DateTime.Now;
                    metadata["node_type"] = elementType + "Node";

                    dict = new Message();
                    dict["title"] = element?.Title + " element";
                    dict["width"] = size.Width.ToString();
                    dict["height"] = size.Height.ToString();
                    dict["nodeType"] = elementType.ToString();
                    dict["x"] = pos.X;
                    dict["y"] = pos.Y;
                    dict["contentId"] = libraryId;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                }
                else
                {
                    await
                        StaticServerCalls.PutCollectionInstanceOnMainCollection(pos.X, pos.Y, libraryId, size.Width,
                            size.Height);
                }
            });
        }

        private void ShowHelpButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ShowHelperText();
        }



    }
}
