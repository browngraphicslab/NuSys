using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : UserControl
    {
        public ImageNodeView(ImageNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                var animX = new Storyboard();
                var animXAnim = new DoubleAnimation();
                animXAnim.Duration = TimeSpan.FromMilliseconds(300);
                animXAnim.EasingFunction = new ExponentialEase();
                animXAnim.From = 0.0;
                animXAnim.To = 1.0;
                animX.Children.Add(animXAnim);
                Storyboard.SetTarget(animX, this);
                Storyboard.SetTargetProperty(animX, "Opacity");
                animX.Begin();
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
