using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class GroupNodeExpandedView : AnimatableUserControl
    {
        public GroupNodeExpandedView()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;


        }

        private async  void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var factory = new GroupItemThumbFactory();
            var vm = (GroupNodeViewModel) DataContext;
            var count = 0;
            var numRows = 2;
            var numCols = 4;
            foreach (var child in vm.AtomViewList)
            {
                var childModel = (child.DataContext as GroupItemViewModel).Model;
                var view = await factory.CreateFromSendable(childModel, null);
                var wrappedView = new Border();
                wrappedView.Padding = new Thickness(10);
                wrappedView.Child = view;
                Grid.SetRow(wrappedView, count/numCols);
                Grid.SetColumn(wrappedView, count % numCols);
                xGrid.Children.Add(wrappedView);
                count++;
            }
        }
    }
}
