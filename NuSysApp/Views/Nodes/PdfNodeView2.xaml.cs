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

            vm.InkContainer[(int)pageNum] = inkCanvas.Strokes;
            inkCanvas.Strokes.Clear();
            inkCanvas.Children.Clear();
            inkCanvas.Manager = new Windows.UI.Input.Inking.InkManager();
            if (pageNum <= 0) return;
            vm.RenderedBitmapImage = vm.RenderedPages[(int)pageNum - 1];
            vm.CurrentPageNumber--;

            //      foreach (InkStroke inkStroke in vm.InkContainer[(int)pageNum -1])
            //      {
            //          inkCanvas.InkPresenter.StrokeContainer.AddStroke(inkStroke);
            //      }
                inkCanvas.Strokes = vm.InkContainer[(int)vm.CurrentPageNumber];
        }

        private void pageRight_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            var pageCount = vm.PageCount;
            var pageNum = vm.CurrentPageNumber;
            vm.InkContainer[(int)pageNum] = inkCanvas.Strokes;
            inkCanvas.Strokes.Clear();
            inkCanvas.Children.Clear();
            inkCanvas.Manager = new Windows.UI.Input.Inking.InkManager();
            if (pageNum >= (pageCount - 1)) return;
            vm.RenderedBitmapImage = vm.RenderedPages[(int)pageNum + 1];
            vm.CurrentPageNumber++;
            inkCanvas.Strokes = vm.InkContainer[(int)vm.CurrentPageNumber];
        }
    }
}
