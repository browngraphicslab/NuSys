using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class InkNodeView2 : UserControl
    {
        //InkDrawingAttributes _drawingAttributes;
        private InqLine[] _toBePasted;
        public InkNodeView2(InkNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }
        
        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();
        }       

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (InkNodeViewModel)this.DataContext;
            vm.CreateAnnotation();
            vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove 
            if (vm.IsAnnotation)
            {
                nodeTpl.bg.Background = new SolidColorBrush(Color.FromArgb(100, 255, 235, 205));
            }
        }

        public void UpdateInk()
        {
            var vm = (InkNodeViewModel)this.DataContext;
            var rect = nodeTpl.inkCanvas.PasteStrokes(_toBePasted);
            vm.Width = rect.Width;
            vm.Height = rect.Height;
        }

        private void InkNodeView_PromoteInk(object o, RoutedEventArgs e)
        {
            UpdateInk();
            Loaded -= InkNodeView_PromoteInk;
        }

        public void PromoteStrokes(InqLine[] lines)
        {
            _toBePasted = lines;
            Loaded += InkNodeView_PromoteInk;
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
