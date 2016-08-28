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
    public sealed partial class AliasTabView : UserControl
    {
        /// <summary>
        /// True if we are currently requesting a link to the server, prevents us from sending multiple of the same request
        /// </summary>
        private bool _isRequesting;

        public AliasTabView()
        {
            DataContext = new AliasTabViewModel();
            InitializeComponent();
        }

        private void LibraryListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // Do nothing for now
        }

        private void XSortButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = DataContext as LinkEditorTabViewModel; //Harsh is this supposed to be a link editor tab view model?
            vm?.SortByTitle();
        }

        
        private void SortCreator_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AliasTabViewModel;
            Debug.Assert(vm != null, "this really shouldn't ever be null or fail the cast");

            vm?.SortBy(aliasTemplate => aliasTemplate.Creator);
        }

        private void SortTitle_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AliasTabViewModel;
            Debug.Assert(vm != null, "this really shouldn't ever be null or fail the cast");

            vm?.SortBy(aliasTemplate => aliasTemplate.CollectionTitle);
        }

        private void SortTimestamp_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AliasTabViewModel;
            Debug.Assert(vm != null, "this really shouldn't ever be null or fail the cast");

            vm?.SortBy(aliasTemplate => aliasTemplate.Timestamp);
        }

        /// <summary>
        /// Method to load asynchronously the list of element models from the server.  
        /// This will be what sets the list of element models.
        /// </summary>
        /// <param name="libraryElementId"></param>
        /// <returns></returns>
        public async Task LoadLibraryElementAsync(string libraryElementId) //definitely can be refactored after the prototype and i understand the structure of the detail view better
        {
            if (!_isRequesting)
            {
                _isRequesting = true;
                Debug.Assert(!string.IsNullOrEmpty(libraryElementId)); //make sure we've been given a valid id;
                var task = Task.Run(async delegate
                {
                    var request = new GetAliasesOfLibraryElementRequest(libraryElementId);//create a request to get all the element models
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                    if (request.WasSuccessful() == true) //make sure the request was successful and finished
                    {
                        var models = request.GetReturnedElementModels(); //get the returned element models
                        UITask.Run(delegate
                        {
                            var vm = DataContext as AliasTabViewModel;
                            Debug.Assert(vm != null, "this really shouldn't ever be null or fail the cast");
                            //cast and state-check the view model

                            vm.SetElementModels(models); //set the element models on the view model
                        });
                    }
                    else
                    {
                        throw new NotImplementedException("Get aliases of library element request failed in alias tab view");//TODO alert the user that the aliases are unavailable
                    }
                });
                await task; //await the created task.  Later we can make this into an instance variable and dispose of it correctly if needed;
                _isRequesting = false;
            }
            else
            {
                return;//TODO if we save the above task as an instance variable, await it
            }
        }


        /// <summary>
        /// This opens the detail view of the collection the alias is in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkedTo_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var aliasTemplate = textBlock?.DataContext as AliasTemplate;

            if (aliasTemplate == null)
            {
                return;
            }
            // We get the controller of the end point of the link and use it to open the detail view
            var collectionId = aliasTemplate?.CollectionID;
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(collectionId);
            SessionController.Instance.SessionView.DetailViewerView.ShowElement(controller);
        }

        private void CollectionTitle_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var aliasTemplate = textBlock?.DataContext as AliasTemplate;

            if (aliasTemplate == null)
            {
                return;
            }
            // We get the controller of the end point of the link and use it to open the detail view
            var collectionId = aliasTemplate?.CollectionID;
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(collectionId);
            SessionController.Instance.SessionView.DetailViewerView.ShowElement(controller);
        }
    }
}
