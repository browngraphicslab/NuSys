using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfNodeView2 : UserControl
    {
        public PdfNodeView2(PdfNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }


        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();
        }

        private void pageLeft_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.FlipLeft();

            //foreach (var stroke in nodeTpl.inkCanvas.Strokes)
            //{
            //    nodeTpl.inkCanvas.Children.Remove(stroke);
            //    vm.InkContainer[(int)pageNum].Add(stroke);
            //}
            //nodeTpl.inkCanvas.Strokes.Clear();
            //foreach (var stroke in vm.InkContainer[(int)vm.CurrentPageNumber])
            //{
            //    nodeTpl.inkCanvas.Strokes.Add(stroke);
            //    nodeTpl.inkCanvas.Children.Add(stroke);
            //}
      //      this.DataContext = vm;
        }

        private void pageRight_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.FlipRight();
 
            //foreach (var stroke in nodeTpl.inkCanvas.Strokes)
            //{
            //    nodeTpl.inkCanvas.Children.Remove(stroke);
            //    vm.InkContainer[(int)pageNum].Add(stroke);
            //}
            //nodeTpl.inkCanvas.Strokes.Clear();
            //foreach (var stroke in vm.InkContainer[(int)vm.CurrentPageNumber])
            //{
            //    nodeTpl.inkCanvas.Strokes.Add(stroke);
            //    nodeTpl.inkCanvas.Children.Add(stroke);
            //}
            //inkCanvas.Strokes = vm.InkContainer[(int)vm.CurrentPageNumber];
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
