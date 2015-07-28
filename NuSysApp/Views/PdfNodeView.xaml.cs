using System.Collections.Generic;
using System.IO;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

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
            inkCanvas.InkPresenter.IsInputEnabled = false;
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
            Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch; //This line is setting the Devices that can be used to display ink
            
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

        private async void pageLeft_Click(object sender, RoutedEventArgs e)
        {

            var vm = (PdfNodeViewModel)this.DataContext;
            var pageNum = vm.CurrentPageNumber;
            vm.InkContainer[(int)pageNum] = inkCanvas.InkPresenter.StrokeContainer;
            if (pageNum <= 0) return;
            vm.RenderedBitmapImage = vm.RenderedPages[(int)pageNum - 1];
            vm.CurrentPageNumber--;
            inkCanvas.InkPresenter.StrokeContainer.Clear();

      //      foreach (InkStroke inkStroke in vm.InkContainer[(int)pageNum -1])
      //      {
      //          inkCanvas.InkPresenter.StrokeContainer.AddStroke(inkStroke);
      //      }
            inkCanvas.InkPresenter.StrokeContainer=vm.InkContainer[(int)vm.CurrentPageNumber];
        }

        private void EditC_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.ToggleEditingC();
            inkCanvas.InkPresenter.IsInputEnabled = vm.IsEditingInk;   
            if (ManipulationMode == ManipulationModes.All)
            {
                ManipulationMode = ManipulationModes.None;
            }
            else
            {
                ManipulationMode = ManipulationModes.All;
            }
        }
        private void pageRight_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            var pageCount = vm.PageCount;
            var pageNum = vm.CurrentPageNumber;
            vm.InkContainer[(int)pageNum] = inkCanvas.InkPresenter.StrokeContainer;
            if (pageNum >= (pageCount - 1)) return;
            vm.RenderedBitmapImage = vm.RenderedPages[(int) pageNum + 1];
            vm.CurrentPageNumber++;
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            inkCanvas.InkPresenter.StrokeContainer=vm.InkContainer[(int)vm.CurrentPageNumber];
        }
    }
}
