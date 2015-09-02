using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class PinWindow : UserControl
    {
        public PinWindow()
        {
            this.InitializeComponent();
            Border.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 81, 220, 231));           
        }


        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pinvm = ((TextBlock)sender).DataContext as PinViewModel;
            var pinModel = (PinModel) pinvm.Model;

            var vm = (WorkspaceViewModel)this.DataContext;

            var c = new CompositeTransform
            {
                ScaleX = 1,
                ScaleY = 1,
                TranslateX = -pinModel.X + Window.Current.Bounds.Width / 2,
                TranslateY = -pinModel.Y + Window.Current.Bounds.Height / 2,
            };
            vm.CompositeTransform = c;
            e.Handled = true;
        }
    }
}
