﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PinView : UserControl
    {
        public PinView(PinViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }

        private void Path_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var pinvm = this.DataContext as PinViewModel;
            var pinModel = (PinModel) pinvm.Model;
            
            var vm = pinvm.Workspace;

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
