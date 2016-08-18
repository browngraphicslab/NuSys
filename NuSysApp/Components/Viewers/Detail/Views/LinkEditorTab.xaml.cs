using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Navigation;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LinkEditorTab : UserControl
    {
        /// <summary>
        /// Contains the libraryItemTemplate chosen by the user from the autosuggest box, null if no element is currently chosen
        /// </summary>
        private LibraryItemTemplate _chosenLibraryItemTemplate;

        /// <summary>
        /// True if we are currently requesting a link to the server, prevents us from sending multiple of the same request
        /// </summary>
        private bool isRequesting;

        public LinkEditorTab()
        {
            DataContext = new LinkEditorTabViewModel();
            InitializeComponent();

        }

        private void LibraryListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // Do nothing for now
        }

        private void XSortButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = DataContext as LinkEditorTabViewModel;
            vm?.SortByTitle();
        }

        /// <summary>
        /// Called when the user taps the create link button, creates a request
        /// to the server to create the desired link. Fails if the autosuggest box
        /// does not contain any text, but should be disabled in that case.
        /// Checks to make sure invalid requests are not sent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CreateLinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var content = _chosenLibraryItemTemplate;
            if (content == null)
            {
                Debug.Fail("we probably should never get here, check that you didn't programatically change the text in the LinkToBox AutoSuggestBox");
                createLinkButton.IsEnabled = false;
                return;
            }
            // if we currently are requesting a link from the server don't send a new request
            if (isRequesting)
            {
                ShowCreateLinkErrorText("Currently Requesting a Link!");
                return;
            }

            var vm = DataContext as LinkEditorTabViewModel;

            // check to make sure the link does not already exist
            var inLibElemId = vm.CurrentLibraryElementId;
            var outLibElemId = content.LibraryElementId;

            // if there is a link between two libraryelements then possible link will not be null
            var possibleLink = SessionController.Instance.LinksController.GetLinkLibraryElementControllerBetweenContent(inLibElemId,
                outLibElemId);

            // if possible link is not null then return and show an error message
            if (possibleLink != null)
            {
                ShowCreateLinkErrorText("That Link Already Exists!");
                return;
            }

            Debug.Assert(content != null && content.LibraryElementId != null);
            isRequesting = true;
            await vm.CreateLinkAsync(content.LibraryElementId);
            isRequesting = false;
            // collapse the xCreateLinkErroxBox in case it was open
            xCreateLinkErrorBox.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows error text on the detail view if the create link button fails for some reason.
        /// Make sure to hide the error text correctly.
        /// </summary>
        /// <param name="text">The error text you wish to show.</param>
        private void ShowCreateLinkErrorText(string text)
        {
            xCreateLinkErrorBox.Text = text;
            if (xCreateLinkErrorBox.Visibility == Visibility.Collapsed)
            {
                xCreateLinkErrorBox.Visibility = Visibility.Visible;
            }
        }

        private void SortLinkedTo_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LinkEditorTabViewModel;
            vm?.SortByLinkedTo();
        }

        private void SortTitle_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LinkEditorTabViewModel;
            vm?.SortByTitle();
        }


        private void X_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var id = (sender as Image)?.DataContext as string;
            var vm = DataContext as LinkEditorTabViewModel;
            vm?.DeleteLink(id);
        }

        /// <summary>
        /// When the user enters text, update the suggestion list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // If the change was due to user input
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // disable the createLinkButton, in case the user is changing their link selection
                createLinkButton.IsEnabled = false;

                // close the error message, the user has probably read the error and is performing a fix now
                xCreateLinkErrorBox.Visibility = Visibility.Collapsed;

                // set chose LibaryItemTemplate to null to reflect the change in selection
                _chosenLibraryItemTemplate = null;

                // use vm to get the current library element id
                var vm = DataContext as LinkEditorTabViewModel;

                // filter the data shown by the autosuggest box to only library elements whose titles contain the text entered
                // and whose library element ids don't match the id of the library element currently shown in the detail veiwer
                var filteredData = (DataContext as LinkEditorTabViewModel).LibraryElements.Where(t => t.Title.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant()) && t.LibraryElementId != vm.CurrentLibraryElementId);
                // set the LinkToBox.ItemsSource to the filtered data to update the suggestions
                LinkToBox.ItemsSource = filteredData;
            }
        }

        /// <summary>
        /// When the user submits a query, show the query results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // if the focus is in the suggestion linst when the query is submitted the chosen Suggestion property is the item that was selected from the list
            if (args.ChosenSuggestion != null)
            {
                createLinkButton.IsEnabled = true;
                _chosenLibraryItemTemplate = args.ChosenSuggestion as LibraryItemTemplate;
            }
            // if the focus is in the textbox when the query is submitted the chosen Suggestion Property is null
            else
            {
                // get the current library element id
                var libraryElementId =
                        (SessionController.Instance.SessionView.DetailViewerView.DataContext as DetailViewerViewModel)
                        .CurrentElementController.LibraryElementModel.LibraryElementId;
                // get the first item which could match the users search string and isn't the current element shown in the detail viwer
                //or null if no items match the search string
                var item = (DataContext as LinkEditorTabViewModel)?.LibraryElements.FirstOrDefault(t =>
                                     t.Title.ToLowerInvariant().Contains(args.QueryText.ToLowerInvariant()) &&
                                     t.LibraryElementId != libraryElementId);
                if (item != null)
                {
                    // set the text to the item's title, and enable the createLinkButton
                    sender.Text = item.Title;
                    createLinkButton.IsEnabled = true;
                    _chosenLibraryItemTemplate = item;
                }
                else
                {
                    sender.Text = "No Results Found";
                }
            }
        }
    }
}
