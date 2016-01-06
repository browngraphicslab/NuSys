using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : AnimatableUserControl
    {
        public ImageNodeView(ImageNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                vm.Init();
                nodeTpl.inkCanvas.ViewModel.CanvasSize = new Size(vm.Width, vm.Height);
            };
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
