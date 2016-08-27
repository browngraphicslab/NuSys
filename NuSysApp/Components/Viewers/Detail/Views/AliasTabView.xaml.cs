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
        private bool isRequesting;

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
            var vm = DataContext as LinkEditorTabViewModel;
            vm?.SortByTitle();
        }

        
        private void SortCreator_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AliasTabViewModel;
            vm?.SortByCreator();
        }

        private void SortTitle_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AliasTabViewModel;
            vm?.SortByTitle();
        }

        private void SortTimestamp_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AliasTabViewModel;
            vm?.SortByTimestamp();
        }

        
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionTitle_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var aliasTemplate = textBlock?.DataContext as AliasTemplate;

            if (aliasTemplate == null)
            {
                return;
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
    }
}
