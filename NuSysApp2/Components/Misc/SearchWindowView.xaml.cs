﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp2
{
    public sealed partial class SearchWindowView : UserControl
    {
        private Dictionary<string, List<ElementModel>> _searchIndex = new Dictionary<string, List<ElementModel>>();

        private static SearchWindowView _instance;

        public SearchWindowView()
        {
            this.InitializeComponent();
            searchBox.TextChanged += SearchBoxOnTextChanged;
            DataContext = new SearchWindowViewModel();
            _instance = this;
        }

        private void SearchBoxOnTextChanged(object sender, String routedEventArgs)
        {
            var vm = (SearchWindowViewModel)DataContext;
            vm.SearchFor(searchBox.Text.ToLower());
        }

        public static void SetFocus()
        {
            _instance.searchBox.Focus(FocusState.Programmatic);
        }

        private void UIElement_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (SearchResultItem)((FrameworkElement) sender).DataContext;
            // TODO: refactor
            //SessionController.Instance.ActiveFreeFormViewer.MoveToNode(vm.LibraryId);
        }
    }
}
