﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class LinkEditorTab : UserControl
    {
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

        private void CreateLinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var content = LinkToBox.SelectedItem as LibraryItemTemplate;
            var vm = DataContext as LinkEditorTabViewModel;
            vm?.CreateLink(content?.ContentID);
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
    }
}
