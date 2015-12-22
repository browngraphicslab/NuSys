using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : AnimatableNodeView
    {
        public ImageNodeView(ImageNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }
    }
}
