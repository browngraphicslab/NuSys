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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfNodeView : UserControl
    {
        public PdfNodeView(PdfNodeViewModel pdfNodeViewModel)
        {
            this.InitializeComponent();
            this.DataContext = pdfNodeViewModel;
            this.SetUpBindings();
        }
        private void SetUpBindings()
        {
            var leftBinding = new Binding
            {
                Path = new PropertyPath("X"),
                Mode = BindingMode.TwoWay
            };
            this.SetBinding(Canvas.LeftProperty, leftBinding);

            var topBinding = new Binding
            {
                Path = new PropertyPath("Y"),
                Mode = BindingMode.TwoWay
            };
            this.SetBinding(Canvas.TopProperty, topBinding);
        }
        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            vm.Resize((e.Delta.Scale - 1) * vm.Width, (e.Delta.Scale - 1) * vm.Height);
            e.Handled = true;
        }

        private void Resizer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.ToggleSelection();

            e.Handled = true;

        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.Remove();
        }
    }
}
