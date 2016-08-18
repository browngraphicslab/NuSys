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
using NusysIntermediate;
using WinRTXamlToolkit.Controls.Extensions;

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
        private HashSet<FrameworkElement> _openInfo;
        private Point2d _searchExportPos;

        public SearchView()
        {
            this.InitializeComponent();

            DataContextChanged += delegate (FrameworkElement sender, DataContextChangedEventArgs args)
            {

                if (!(DataContext is SearchViewModel))
                    return;
                _vm = (SearchViewModel)DataContext;
                _openInfo = new HashSet<FrameworkElement>();

                // set the view equal to the size of the window
                this.ResizeView(true, true);
                // when the size of the winow changes reset the view
                SessionController.Instance.SessionView.SizeChanged += SessionView_SizeChanged;

                SessionController.Instance.SessionView.MainCanvas.PointerPressed += MainCanvas_PointerPressed;

                _vm.SearchExportButtonVisibility = Visibility.Collapsed;

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
            _vm.SearchExportButtonVisibility = Visibility.Collapsed;
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
            // hide helper text and any open info boxes
            HideHelperText();
            foreach (var element in _openInfo) element.Visibility = Visibility.Collapsed;

            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                // Use args.QueryText to determine what to do.
                _vm.AdvancedSearch(QueryArgsBuilder.GetQueryArgs(args.QueryText));
            }
        }

        public void SetFocusOnSearchBox()
        {
            SearchBox.Focus(FocusState.Programmatic);
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
                if (_openInfo.Contains(info))
                {
                    _openInfo.Remove(info);
                }
            }
            else
            {
                info.Visibility = Visibility.Visible;
                _openInfo.Add(info);
            }
        }

        private void ListView_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _isSingleTap = false;
            var item = ListView.SelectedItem as SearchResultTemplate;
            var id = item.LibraryElementId;

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
            if ((SessionController.Instance.ActiveFreeFormViewer.LibraryElementId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
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

            if (!SessionController.Instance.ContentController.ContainsContentDataModel(element.ContentDataModelId))
            {
                Task.Run(async delegate
                {
                    SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(element.ContentDataModelId);
                });
            }
        }


        private void ListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            SearchResultTemplate result = (sender as Grid)?.DataContext as SearchResultTemplate;
            LibraryElementModel element = result?.Model;
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);


            // scroll the scroll viewer if sp.X is inside the scrollviewer
            if (sp.X < Width)
            {
                Border border = (Border)VisualTreeHelper.GetChild(ListView, 0);
                ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta.Translation.Y);
                }
            }

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
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
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

            //Before we add the node, we need to check if the access settings for the library element and the workspace are incompatible
            // If they are different we simply return 
            var currWorkSpaceAccessType =
                SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType;

            if (element.AccessType == NusysConstants.AccessType.Private &&
                currWorkSpaceAccessType == NusysConstants.AccessType.Public)
            {
                return;
            }

            await AddSearchResulttoSession(result);
        }

        /// <summary>
        /// Adds the search result to the collection at point _x, _y
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AddSearchResulttoSession(SearchResultTemplate result)
        {
            // get the library element model which needs to be added to the stack
            var lem = SessionController.Instance.ContentController.GetLibraryElementModel(result.LibraryElementId);

            // if the library element model doesn't exist, or is a link, don't add it to the session
            if (lem == null || lem.Type == NusysConstants.ElementType.Link)
            {
                return;
            }

            // transforms the point from the maincanvas to the workspace
            var workspacePoint = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(new Point(_x, _y));

            // create a new element request args, and pass in the required fields
            var newElementRequestArgs = new NewElementRequestArgs
            {
                // set the position
                X = workspacePoint.X,
                Y = workspacePoint.Y,

                // size
                Width = Constants.DefaultNodeSize,
                Height = Constants.DefaultNodeSize,

                // ids
                ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                LibraryElementId = lem.LibraryElementId
            };

            // execute the request
            var request = new NewElementRequest(newElementRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedElementToSession();
        }

        private void ShowHelpButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ShowHelperText();
        }

        /// <summary>
        /// Adds a library dragging rectangle to represent where the exported collection will be
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSearchExportButton_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

            // Since we are adding a collection, we should make the dragging rectangle reflect this
            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.SwitchType(NusysConstants.ElementType.Collection);
            view.LibraryDraggingRectangle.Show();
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);

            // Make the rectangle movable and set its position
            rect.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)rect.RenderTransform;
            t.TranslateX += _searchExportPos.X;
            t.TranslateY += _searchExportPos.Y;
            e.Handled = true;
        }

        /// <summary>
        /// Moves the library dragging rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSearchExportButton_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // Obtain the library dragging rectangle  
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;

            // Update its transform
            var t = (CompositeTransform)rect.RenderTransform;
            t.TranslateX += e.Delta.Translation.X;
            t.TranslateY += e.Delta.Translation.Y;

            // Update the position instance variable
            _searchExportPos.X += e.Delta.Translation.X;
            _searchExportPos.Y += e.Delta.Translation.Y;

            // Handled!
            e.Handled = true;
        }

        /// <summary>
        /// Creates a collection based on the search results, and places it where the cursor was left off 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void XSearchExportButton_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // Hide the library dragging rect
            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;
            rect.Hide();

            // Add a collection to the dropped location
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var dropPoint = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(_searchExportPos);
            await ExportSearchResultsToCollection(dropPoint);
            e.Handled = true;

        }

        /// <summary>
        /// When the search export button is clicked, first find the position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSearchExportButton_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            if (_searchExportPos == null)
            {
                _searchExportPos = new Point2d(0,0);
            }
            _searchExportPos.X = e.GetCurrentPoint(view).Position.X - 25;
            _searchExportPos.Y = e.GetCurrentPoint(view).Position.Y - 25;
            e.Handled = true;
        }

        /// <summary>
        /// Exports the search results to a collection, and places the new collection at the passed in point
        /// @tdgreen, plz comment this, thx. -zkirsche 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private async Task ExportSearchResultsToCollection(Point r)
        {

            Task.Run(async delegate
            {
                // the library element id of the collection we are creating, used as the parent collection id when adding elements to it later in the method
                var collectionLibElemId = SessionController.Instance.GenerateId();

                // We determine the access type of the tool generated collection based on the collection we're in and pass that in to the request
                NusysConstants.AccessType newCollectionAccessType;
                var currWorkSpaceAccessType = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType;
                if (currWorkSpaceAccessType == NusysConstants.AccessType.Public)
                {
                    newCollectionAccessType = NusysConstants.AccessType.Public;
                }
                else
                {
                    newCollectionAccessType = NusysConstants.AccessType.Private;
                }
                // create a new library element args class to assist in creating the collection
                var createNewLibraryElementRequestArgs = new CreateNewLibraryElementRequestArgs
                {
                    ContentId = SessionController.Instance.GenerateId(),
                    LibraryElementType = NusysConstants.ElementType.Collection,
                    Title = "Search Results for '" + SearchBox.Text + "'",
                    LibraryElementId = collectionLibElemId,
                    AccessType = newCollectionAccessType
                };

                // create a new content request args to assist in creating the collection
                var createNewContentRequestArgs = new CreateNewContentRequestArgs
                {
                    LibraryElementArgs = createNewLibraryElementRequestArgs
                };

                // create the content request, and execute it, adding the collection to the library
                var request = new CreateNewContentRequest(createNewContentRequestArgs);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                request.AddReturnedLibraryElementToLibrary();

                // Add all the elements to the newly created collection
                foreach (var searchResult in _vm.PageElements.ToList().GetRange(0, Math.Min(_vm.PageElements.Count, 20))) //todo indicate to the user that only 20 results are added
                {

                    // get the library element model which needs to be added to the collection
                    var lem = SessionController.Instance.ContentController.GetLibraryElementModel(searchResult.LibraryElementId);

                    // if the library element model doesn't exist, or is a link don't add it to the collection
                    if (lem == null || lem.Type == NusysConstants.ElementType.Link)
                    {
                        continue;
                    }

                    // create a new element request args, and pass in the required fields
                    var newElementRequestArgs = new NewElementRequestArgs
                    {
                        // set the position
                        X = 50000,
                        Y = 50000,

                        // size
                        Width = Constants.DefaultNodeSize,
                        Height = Constants.DefaultNodeSize,

                        // ids
                        ParentCollectionId = collectionLibElemId,
                        LibraryElementId = lem.LibraryElementId
                    };

                    // create and execute the request
                    var requestElemToCollection = new NewElementRequest(newElementRequestArgs);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(requestElemToCollection);
                    await requestElemToCollection.AddReturnedElementToSessionAsync();
                }

                // add the collection to the current session
                var collectionLEM =
                    SessionController.Instance.ContentController.GetLibraryElementController(collectionLibElemId);
                collectionLEM.AddElementAtPosition(r.X, r.Y);

            });
        }

    }
}
