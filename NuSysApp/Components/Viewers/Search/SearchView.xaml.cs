using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MyToolkit.UI;

namespace NuSysApp
{
    public partial class SearchView : AnimatableUserControl
    {
        private ElementModel _nodeModel;
        private String _tagToDelete;
        public Boolean DeleteOnFocus;
        public string Title { get; set; }
        public string Date { get; set; }

        public UserControl View { get; set; }

        private ElementViewModel _currentElementViewModel;
        public ElementController CurrentElementController { get; set; }

        public delegate void TitleChangedHandler(object source, string newTitle);
        public event TitleChangedHandler TitleChanged;

        private SearchViewModel _vm;

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
                // Metadata.ItemsSource = vm.Metadata;
            };

        }

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

        #region manipulation code
        public void Dispose()
        {
            var tempvm = (ElementViewModel)View.DataContext;
            tempvm.PropertyChanged -= NodeVMPropertChanged;
            _nodeModel = null;

        }

        

        /* Remove if not showing detail view
        public async Task<bool> ShowElement(ElementController controller)
        {
            CurrentElementController = controller;
            View = await _viewFactory.CreateFromSendable(controller);
            if (View == null)
                return false;
            _nodeModel = controller.Model;
            Title = controller.LibraryElementModel.Title;
            this.ChangeTitle(this, controller.LibraryElementModel.Title);

            controller.LibraryElementModel.OnTitleChanged += ChangeTitle;

            var tempvm = (ElementViewModel)View.DataContext;
            tempvm.PropertyChanged += NodeVMPropertChanged;
            return true;
        }
        */


        private void NodeVMPropertChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(View.DataContext is ElementViewModel))
                return;

            var tempvm = (ElementViewModel)View.DataContext;
            switch (e.PropertyName.ToLower())
            {
                case "title":
                    Title = tempvm.Title;
                    break;
                default:
                    break;
            }
        }

        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // todo errorchecking
            this.Width += e.Delta.Translation.X;
            _vm.ResultWidth = this.Width - resizer.Width;

        }

        private void closeSV_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
        #endregion manipulation code

        #region search bar text manipulation
        // when the user enters text update the suggestion list
        private void SearchBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

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

        // when the user submits a query show the query results
        private void SearchBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

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


        // display extra info when the header is tapped
        private void ResultHeader_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var highlightColor = Colors.LightSkyBlue;
            var normalColor = Colors.White;
            var header = sender as Grid;
            var info = header.FindName("ResultInfo") as FrameworkElement;
            if (info == null) return;
            // if the extra info is open, close it, and return to normal color
            if (info.Visibility == Visibility.Visible)
            {
                info.Visibility = Visibility.Collapsed;
                header.Background = new SolidColorBrush(normalColor);
            }
            else
            {
                info.Visibility = Visibility.Visible;
                header.Background = new SolidColorBrush(highlightColor);
            }
        }

        private void ListView_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var item = ListView.SelectedItem as SearchResultTemplate;
            var id = item.Id;

            e.Handled = true;
        }
    }
}
