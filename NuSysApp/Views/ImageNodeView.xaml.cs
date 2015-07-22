using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : UserControl
    {
        public ImageNodeView(ImageNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.SetUpBindings();
        }
        private void SetUpBindings()
        {
            Binding leftBinding = new Binding() { Path = new PropertyPath("X") };
            leftBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(Canvas.LeftProperty, leftBinding);

            Binding topBinding = new Binding() { Path = new PropertyPath("Y") };
            topBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(Canvas.TopProperty, topBinding);
        }


        #region Event Handlers
        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            InkNodeViewModel vm = (InkNodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void Resizer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            InkNodeViewModel vm = (InkNodeViewModel)this.DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            InkNodeViewModel vm = (InkNodeViewModel)this.DataContext;
            vm.ToggleSelection();

            e.Handled = true;

        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            InkNodeViewModel vm = (InkNodeViewModel)this.DataContext;
            vm.DeleteNode();
        }
        #endregion Event Handlers
    }
}
