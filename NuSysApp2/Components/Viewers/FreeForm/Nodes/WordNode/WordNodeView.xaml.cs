using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class WordNodeView : AnimatableUserControl, IThumbnailable
    {
        private WordNodeViewModel _vm;

        public WordNodeView(WordNodeViewModel vm)
        {
            _vm = vm;
            InitializeComponent();
            DataContext = vm;
            vm.Controller.Disposed += ControllerOnDisposed;
        }

        public async Task OnGoTo(int page)
        {
            await _vm.Goto(page);
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (WordNodeViewModel)DataContext;
            nodeTpl.Dispose();
            vm.Controller.Disposed -= ControllerOnDisposed;
        }

        private async void OnPageLeftClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (WordNodeViewModel)this.DataContext;
            await vm.FlipLeft();
            e.Handled = true;
        }

        private async void OnPageRightClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (WordNodeViewModel)this.DataContext;
            await vm.FlipRight();
            e.Handled = true;
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)this.DataContext;
            vm.Controller.RequestDelete();
        }

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.RequestDuplicate(vm.Model.X, vm.Model.Y);
        }

        private void PageRight_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(xRenderedPdf, width, height);
            return r;
        }

        public double GetWidth()
        {
            return xRenderedPdf.ActualWidth;
        }

        public double GetHeight()
        {
            return xRenderedPdf.ActualHeight;
        }
    }
}
