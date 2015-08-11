using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class TextNodeView2 : UserControl
    {

        public TextNodeView2(TextNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;          
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            var vm = (TextNodeViewModel)this.DataContext;
            vm.ToggleEditing();
        }

        private void OnInkClick(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();            
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (TextNodeViewModel)this.DataContext;
            vm.CreateAnnotation();
            if (!vm.IsAnnotation) return;
            nodeTpl.bg.Background = new SolidColorBrush(Color.FromArgb(100, 255, 235, 205));
            this.textBlock.Foreground = new SolidColorBrush(Colors.Black);
            this.textBox.Foreground = new SolidColorBrush(Colors.Black);
        }

    }
}
