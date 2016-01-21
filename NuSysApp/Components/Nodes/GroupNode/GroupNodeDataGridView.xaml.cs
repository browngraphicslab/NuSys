using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace NuSysApp
{
    public sealed partial class GroupNodeDataGridView : AnimatableUserControl
    {
        public GroupNodeDataGridView(GroupNodeDataGridViewModel viewModel)
        {
            this.InitializeComponent();
            DataContext = viewModel;

            //DataContextChanged += delegate(FrameworkElement sender, DataContextChangedEventArgs args)
            //{
            //    if (args.NewValue is GroupNodeViewModel)
            //    {
            //        var vm = args.NewValue as NodeContainerViewModel;
            //        DataContext = new GroupNodeDataGridViewModel((NodeContainerModel)vm.Model);
            //        Debug.WriteLine("DataContext: " + DataContext);
            //    }
            //};
        }
    }
}
