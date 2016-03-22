using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
           DataContext = viewModel;
           this.InitializeComponent();

       
            DataGrid.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true );
        }

        private void OnPointerPressed(object source, PointerRoutedEventArgs args)
        {
            var src = (FrameworkElement) args.OriginalSource;
            if (src.DataContext is GroupNodeDataGridInfo)
            {
                var dc = (GroupNodeDataGridInfo) src.DataContext;
                //SessionController.Instance.IdToControllers[dc.Id].RequestMoveToCollection()
                // get Id here

            }
        }
    }
}
