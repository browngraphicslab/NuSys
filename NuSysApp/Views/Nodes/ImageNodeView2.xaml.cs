﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView2 : UserControl
    {
        public ImageNodeView2(ImageNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }

        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (ImageNodeViewModel)this.DataContext;
            vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove   
            e.Handled = true;
        }

        /// <summary>
        /// Catches the double-tap event so that the floating menus can't be lost.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FloatingButton_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
