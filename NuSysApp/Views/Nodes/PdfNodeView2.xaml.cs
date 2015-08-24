using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            var pageNum = vm.CurrentPageNumber;
            //vm.InkContainer[(int)pageNum] = inkCanvas.Strokes;
            //inkCanvas.Strokes.Clear();
            //inkCanvas.Children.Clear();
            //inkCanvas.Manager = new Windows.UI.Input.Inking.InkManager();
            if (pageNum <= 0) return;
            vm.RenderedBitmapImage = vm.RenderedPages[(int)pageNum - 1];
            vm.CurrentPageNumber--;

            ////      foreach (InkStroke inkStroke in vm.InkContainer[(int)pageNum -1])
            ////      {
            ////          inkCanvas.InkPresenter.StrokeContainer.AddStroke(inkStroke);
            ////      }
            foreach (var stroke in nodeTpl.inkCanvas.Strokes)
            {
                nodeTpl.inkCanvas.Children.Remove(stroke);
                vm.InkContainer[(int)pageNum].Add(stroke);
            }
            nodeTpl.inkCanvas.Strokes.Clear();
            foreach (var stroke in vm.InkContainer[(int)vm.CurrentPageNumber])
            {
                nodeTpl.inkCanvas.Strokes.Add(stroke);
                nodeTpl.inkCanvas.Children.Add(stroke);
            }
      //      this.DataContext = vm;
        }

        private void pageRight_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            var pageCount = vm.PageCount;
            var pageNum = vm.CurrentPageNumber;
            //vm.InkContainer[(int)pageNum] = inkCanvas.Strokes;
            //inkCanvas.Strokes.Clear();
            //inkCanvas.Children.Clear();
            //inkCanvas.Manager = new Windows.UI.Input.Inking.InkManager();
            if (pageNum >= (pageCount - 1)) return;
            vm.RenderedBitmapImage = vm.RenderedPages[(int)pageNum + 1];
            vm.CurrentPageNumber++;
            foreach (var stroke in nodeTpl.inkCanvas.Strokes)
            {
                nodeTpl.inkCanvas.Children.Remove(stroke);
                vm.InkContainer[(int)pageNum].Add(stroke);
            }
            nodeTpl.inkCanvas.Strokes.Clear();
            foreach (var stroke in vm.InkContainer[(int)vm.CurrentPageNumber])
            {
                nodeTpl.inkCanvas.Strokes.Add(stroke);
                nodeTpl.inkCanvas.Children.Add(stroke);
            }
     //      this.DataContext = vm;
            //inkCanvas.Strokes = vm.InkContainer[(int)vm.CurrentPageNumber];
        }
    }
}
