﻿using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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
    }
}
