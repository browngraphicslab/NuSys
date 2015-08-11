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
            var tempChild = new List<UIElement>();
            for (var i = 0; i < inkCanvas.Children.Count; i++)
            {
                tempChild.Add(inkCanvas.Children.ElementAt(i));
            }
            vm.InkContainer[(int)pageNum] = tempChild;
            inkCanvas.Children.Clear();
            if (pageNum <= 0) return;
            vm.RenderedBitmapImage = vm.RenderedPages[(int)pageNum - 1];
            vm.CurrentPageNumber--;

            //      foreach (InkStroke inkStroke in vm.InkContainer[(int)pageNum -1])
            //      {
            //          inkCanvas.InkPresenter.StrokeContainer.AddStroke(inkStroke);
            //      }
            for (var i = 0; i < vm.InkContainer[(int)vm.CurrentPageNumber].Count; i++) {
                inkCanvas.Children.Add(vm.InkContainer[(int)vm.CurrentPageNumber][i]);
            }
        }

        private void pageRight_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            var pageCount = vm.PageCount;
            var pageNum = vm.CurrentPageNumber;
            var tempChild = new List<UIElement>();
            for (var i = 0; i < inkCanvas.Children.Count; i++)
            {
                tempChild.Add(inkCanvas.Children.ElementAt(i));
            }
            vm.InkContainer[(int)pageNum] = tempChild;
            inkCanvas.Children.Clear();
            if (pageNum >= (pageCount - 1)) return;
            vm.RenderedBitmapImage = vm.RenderedPages[(int)pageNum + 1];
            vm.CurrentPageNumber++;
            for (var i = 0; i < vm.InkContainer[(int)vm.CurrentPageNumber].Count; i++) {
                inkCanvas.Children.Add(vm.InkContainer[(int)vm.CurrentPageNumber][i]);
            }
        }
    }
}
